using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{

    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    //static damit keine neue Instanz der Klasse erstellt wird beim Abrufen
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {ConditionID.psn, new Condition()
            {
            Name = "Poison",
            StartMessage = "has been poisoned.",
            OnAfterTurn = (Monster monster) =>
                {
                    monster.UpdateHP(monster.MaxHP / 8);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} was hurt by the poison");
                }
            } 
        },
        {ConditionID.brn, new Condition()
            {
            Name = "Burn",
            StartMessage = "has been burned.",
            OnAfterTurn = (Monster monster) =>
                {
                    monster.UpdateHP(monster.MaxHP / 4);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} was hurt by the burn");
                }
            }
        },
        {ConditionID.par, new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed.",
                OnBeforeMove = (Monster monster) =>
                {
                    if (Random.Range(1, 5) == 1)
                    { 
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} is paralyzed and can't attack");
                        return false; 
                    }

                    return true;
                }
            }
        },
        {ConditionID.frz, new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen and can't attack.",
                OnBeforeMove = (Monster monster) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        monster.CureStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} is not frozen anymore");
                        return true;
                    }

                    return false;
                }
            }
        },
        {ConditionID.slp, new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep and can't attack.",
                OnStart = (Monster monster) =>
                {
                    monster.StatusTime = Random.Range (1,4);
                    Debug.Log($"Will be asleep for {monster.StatusTime} rounds");
                },
                OnBeforeMove = (Monster monster) =>
                {
                    if (monster.StatusTime <= 0)
                    {
                        monster.CureStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} woke up.");
                        return true;
                    }

                    monster.StatusTime--;
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} is sleeping.. ");
                    return false;
                }
            }
        },

        //Flüchtige Statuskonditionen

        {ConditionID.confusion, new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused.",
                OnStart = (Monster monster) =>
                {
                    monster.VolatileStatusTime = Random.Range (1,5);
                    Debug.Log($"Will be confused for {monster.VolatileStatusTime} rounds");
                },
                OnBeforeMove = (Monster monster) =>
                {
                    if (monster.VolatileStatusTime <= 0)
                    {
                        monster.CureVolatileStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} is not confused anymore.");
                        return true;
                    }
                    monster.VolatileStatusTime--;

                    monster.StatusChanges.Enqueue($"{monster.Base.Name} is confused.");
                    //50% Chance erfolgreich einen Move auszuführen
                    if (Random.Range(1,3) == 1)
                        return true;

                    //Verletzt sich selbst
                    monster.UpdateHP(monster.MaxHP / 2);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} hurt itself due to confusion!");

                    return false;
                }
            }
        }
    };
       
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}