using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, BattleOver}
public enum BattleAction { Move, SwitchMonster, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject catchingDeviceSprite;

    public event Action<bool> OnBattleOver;
    public Color highlights;

    BattleState state;
    BattleState? previousState;
    int currentAction;
    int currentMove;
    int currentMember;
    bool aboutToUseChoice = true;

    MonsterParty playerParty;
    MonsterParty trainerParty;
    Monster wildMonster;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;


    public void StartBattle(MonsterParty playerParty, Monster wildMonster)
    {
        isTrainerBattle = false;
        this.playerParty = playerParty;
        this.wildMonster = wildMonster;
        player = playerParty.GetComponent<PlayerController>();
        StartCoroutine (SetupBattle());
    }

    public void StartTrainerBattle(MonsterParty playerParty, MonsterParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {

        dialogBox.EnableBlackBox(true);
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            playerUnit.Setup(playerParty.GetHealthyMonster());
            enemyUnit.Setup(wildMonster);
            //enemyUnit.Monster.CureStatus();

            dialogBox.SetMoveNames(playerUnit.Monster.Moves);

            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Monster.Base.Name} appeared."); //$ wird verwendet, um Variablen innerhalb des Strings zu nutzen -> String Interpolation

        }
        else {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"Trainer {trainer.Name} wants to fight!");

            //Erstes Gegner Monster init
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyMonster = trainerParty.GetHealthyMonster();
            enemyUnit.Setup(enemyMonster);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyMonster.Base.Name}.");

            //Erstes Spieler Monster init
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerMonster = playerParty.GetHealthyMonster();
            playerUnit.Setup(playerMonster);
            yield return dialogBox.TypeDialog($"Let's go {playerMonster.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Monster.Moves);

        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();

    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        currentMember = 0;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Monsters);
        dialogBox.EnableActionSelector(false);
        partyScreen.gameObject.SetActive(true);
    }

    IEnumerator AboutToUse(Monster newMonster)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newMonster.Base.Name}.. Do you want to switch yours?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if(playerAction == BattleAction.Move)
        {
            playerUnit.Monster.CurrentMove = playerUnit.Monster.Moves[currentMove];
            enemyUnit.Monster.CurrentMove = enemyUnit.Monster.GetRandomMove();

            int playerMovePriority = playerUnit.Monster.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Monster.CurrentMove.Base.Priority;

            bool playerGoesFirst = true;

            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Monster.Speed >= enemyUnit.Monster.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondMonster = secondUnit.Monster;

            //Erster Zug
            yield return RunMove(firstUnit, secondUnit, firstUnit.Monster.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondMonster.HP > 0)
            {
                //Zweiter Zug
                yield return RunMove(secondUnit, firstUnit, secondUnit.Monster.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        } else
        {
            if (playerAction == BattleAction.SwitchMonster)
            {
                var selectedMonster = playerParty.Monsters[currentMember];
                state = BattleState.Busy;
                yield return SwitchMonster(selectedMonster);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowDevice();
            }
            else if (playerAction == BattleAction.Run)
            {
                dialogBox.EnableActionSelector(false);
                yield return TryToEscape();            }

            //Gegner kriegt den Zug
            var enemyMove = enemyUnit.Monster.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    void BattleOver(bool won)
    {
        if (isTrainerBattle)
            isTrainerBattle = false;

        state = BattleState.BattleOver;
        playerParty.Monsters.ForEach(m => m.OnBattleOver());
        OnBattleOver(won);
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Monster.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield return sourceUnit.Hud.UpdateHP();

            if (sourceUnit.Monster.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name} was defeated.");
                sourceUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(sourceUnit);
            }
            yield break; 
        }
        yield return ShowStatusChanges(sourceUnit.Monster);

        move.PP--;
        //dialogBox.SetCurrentPP(currentMove, move);

        yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name} used {move.Base.Name}.");

        if (CheckIfMoveHits(move, sourceUnit.Monster, targetUnit.Monster))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Monster, targetUnit.Monster, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
                yield return targetUnit.Hud.UpdateHP();

                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Monster.HP > 0)
            {
                foreach (var secondary in move.Base.SecondaryEffects)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Monster, targetUnit.Monster, secondary.Target);
                }
            }

            if (targetUnit.Monster.HP <= 0)
            {
                yield return HandleMonsterFainted(targetUnit);
            }

        } else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}'s attack missed.");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Monster source, Monster target, MoveTarget moveTarget)
    {
        //Status Boost
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
               source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        //Status Konditionen
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        //Flüchtige Status Konditionen
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn); // Pausiert die Ausführung bis der Status wieder auf RunningTurn gesprungen ist

        //Statuskonditionen wie psn oder brn verletzten das Monster nach dem Zug
        sourceUnit.Monster.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return HandleMonsterFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits (Move move, Monster source, Monster target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;
        
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges (Monster monster)
    {
        while (monster.StatusChanges.Count > 0)
        {
            var message = monster.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandleMonsterFainted(BattleUnit faintedUnit) 
    {
    	yield return dialogBox.TypeDialog($"{faintedUnit.Monster.Base.Name} fainted.");
    	faintedUnit.PlayFaintAnimation();
	    yield return new WaitForSeconds(2f);
	
	    if (!faintedUnit.IsPlayerUnit) 
	    {
		    //EXP Gain
		    int xpYield = faintedUnit.Monster.Base.XPGain;
		    int enemyLevel = faintedUnit.Monster.Level;
		    float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;
		
		    int xpGain = Mathf.FloorToInt((xpYield * enemyLevel * trainerBonus) / 7);
		    playerUnit.Monster.XP += xpGain;
		    yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} gained {xpGain} Experience.");
		    yield return playerUnit.Hud.SetXPSmooth();
		
		    //Check Level Up
		
		
		    yield return new WaitForSeconds(1f);
	}
	
	CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextMonster = playerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                currentMember = 0;
                OpenPartyScreen();
            }
            else
                BattleOver(false);
        }
        else
        {
            if (!isTrainerBattle)
                BattleOver(true);
            else
            {
                var nextMonster = trainerParty.GetHealthyMonster();
                if (nextMonster != null)
                    StartCoroutine(AboutToUse(nextMonster));
                else
                    BattleOver(true);
            }
        }
            
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("That was a critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective!");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            dialogBox.SetMoveNames(playerUnit.Monster.Moves);
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartyScreenSelection();
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(true);
        dialogBox.SetDialog($"Choose a move..");
        dialogBox.EnableMoveSelector(true);
    }

    void HandleActionSelection()
    {
        dialogBox.EnableBlackBox(false);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        /*else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;*/

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        if (currentAction == 0)
            highlights = Color.red;
        else if (currentAction == 1)
            highlights = Color.blue;
        else if (currentAction == 2)
            highlights = Color.green;
        else if (currentAction == 3)
            highlights = Color.yellow;
            

        dialogBox.UpdateActionSelection(currentAction, highlights);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //Fight
                MoveSelection();
            } 
            else if (currentAction == 1)
            {
                //Team
                previousState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 2)
            {
                //Bag
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if (currentAction == 3)
            {
                //Run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
            dialogBox.SetDialog($"Choose a move..");
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
            dialogBox.SetDialog($"Choose a move..");
        }
        /*else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;*/

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Monster.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Monster.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Monster.Moves[currentMove];
            if (move.PP == 0) 
            {
                dialogBox.SetDialog("This move can't be used anymore!");
                return;
            }
            
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            dialogBox.EnableActionSelector(true);
            dialogBox.EnableBlackBox(true);

            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartyScreenSelection()
    {

        if (Input.GetKeyDown(KeyCode.RightArrow))
        { 
            ++currentMember;
            partyScreen.SetMessageText("Choose a Monster!"); 
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
            partyScreen.SetMessageText("Choose a Monster!");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
            partyScreen.SetMessageText("Choose a Monster!");
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -=2;
            partyScreen.SetMessageText("Choose a Monster!");
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Monsters.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Monsters[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("This monster is already defeated!");
                return;
            }
            if (selectedMember == playerUnit.Monster)
            {
                partyScreen.SetMessageText("This monster is already fighting!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (previousState == BattleState.ActionSelection)
            {
                previousState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchMonster));
            } else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchMonster(selectedMember));
            }

        } else if (Input.GetKeyDown(KeyCode.U))
        {
            if (playerUnit.Monster.HP <= 0)
            {
                partyScreen.SetMessageText("Choose a Monster to continue!");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            dialogBox.EnableActionSelector(true);

            if (previousState == BattleState.AboutToUse)
            {
                previousState = null;
                StartCoroutine(SendNextTrainerMonster());
            }
            else
                ActionSelection();
        }
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                previousState = BattleState.AboutToUse;
                OpenPartyScreen();
            } else
            {
                StartCoroutine(SendNextTrainerMonster());
            }
        } else if (Input.GetKeyDown(KeyCode.U))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerMonster());
        }
    }

    IEnumerator SwitchMonster(Monster newMonster)
    {
        if (playerUnit.Monster.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Monster.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newMonster);
        dialogBox.SetMoveNames(newMonster.Moves);
        yield return dialogBox.TypeDialog($"It is your turn {newMonster.Base.Name}!");

        if (previousState == null)
        {
            state = BattleState.RunningTurn;
        }
        else if (previousState == BattleState.AboutToUse)
        {
            previousState = null;
            StartCoroutine(SendNextTrainerMonster());
        }
    }

    IEnumerator SendNextTrainerMonster()
    {
        state = BattleState.Busy;

        var nextMonster = trainerParty.GetHealthyMonster();
        enemyUnit.Setup(nextMonster);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextMonster.Base.Name}.");

        state = BattleState.RunningTurn;
    }

    int TryToCatchMonster(Monster monster)
    {
        float a = (3 * monster.MaxHP - 2 * monster.HP) * monster.Base.CatchRate * ConditionsDB.GetStatusBonus(monster.Status) / (3 * monster.MaxHP);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator ThrowDevice()
    {
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal other trainers monster!");
            state = BattleState.RunningTurn;
            yield break;
        }


        state = BattleState.Busy;

        yield return dialogBox.TypeDialog($"{player.Name} throws his catching thingy.");

        var catchingDeviceObject = Instantiate(catchingDeviceSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var catchingDevice = catchingDeviceObject.GetComponent<SpriteRenderer>();

        // Animations
        yield return catchingDevice.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return catchingDevice.transform.DOMoveY(enemyUnit.transform.position.y - 1, 1.3f).WaitForCompletion();

        int shakeCount = TryToCatchMonster(enemyUnit.Monster);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return catchingDevice.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }
	
	    if (shakeCount == 4)
	    {
		    // Caught
		    yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} was caught");
		    yield return catchingDevice.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddMonster(enemyUnit.Monster);
            yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} has been added to your party.");
		
		    Destroy(catchingDevice);
            BattleOver(true);
        }   else {

            // Not Caught
            yield return new WaitForSeconds(1f);
            catchingDevice.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} broke free!");
            else
                yield return dialogBox.TypeDialog($"Almost caught it!");

            Destroy(catchingDevice);
            state = BattleState.RunningTurn;
        }
 
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Monster.Speed;
        int enemySpeed = enemyUnit.Monster.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        } else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 26;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            } else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                state = BattleState.RunningTurn;
            }
        }

    }
}
