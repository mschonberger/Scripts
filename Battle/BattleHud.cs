using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Die Klasse BattleHUD wird für die Darstellung der richtigen Monsterdaten verwendet - sowohl Player als auch Enemy Units.
   Die Berechnung der korrekten HP werden von hier gestartet. */

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text hpDisplayText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject xpBar;
    [SerializeField] Color hpBarColor;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color parColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color frzColor;

    Monster _monster;
    Dictionary<ConditionID, Color> statusColors;


    public void SetData(Monster monster)
    {
        _monster = monster;

        nameText.text = monster.Base.Name;
        SetLevel();
        GetStats();
        hpDisplayText.text = $"{monster.HP} / {monster.MaxHP}";
        SetHPBarColor(_monster);
        hpBar.SetHP((float)monster.HP / monster.MaxHP, hpBarColor);
        SetXP();


        statusColors = new Dictionary<ConditionID, Color>
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.slp, slpColor },
            {ConditionID.par, parColor },
            {ConditionID.frz, frzColor },
        };

        SetStatusText();
        _monster.OnStatusChanged += SetStatusText;
    }

    void SetHPBarColor(Monster monster)
    {
        hpBarColor = Color.green;

        if ((float)monster.HP <= monster.MaxHP / 1.8 && (float)monster.HP > monster.MaxHP / 4)
        {
            hpBarColor = Color.yellow;
        }
        else if ((float)monster.HP <= monster.MaxHP / 4)
        {
           hpBarColor = Color.red;
        } 
    }

    void SetStatusText()
    {
        if (_monster.Status == null)
        {
            statusText.text = "";

        }
        else
        {
            statusText.text = _monster.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_monster.Status.Id];
        }
    }
    
    public void SetXP()
    {
         if (xpBar == null) return; //because Enemy doesn't have one
         
         float normalizedXP = GetNormalizedXP();
         xpBar.transform.localScale = new Vector3 (normalizedXP, 1, 1);
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _monster.Level;
    }

    public int[] GetStats()
    {
        int[] stats = new int[6];

        stats[0] = _monster.HP;
        stats[1] = _monster.Attack;
        stats[2] = _monster.SpAttack;
        stats[3] = _monster.Defense;
        stats[4] = _monster.SpDefense;
        stats[5] = _monster.Speed;

        return stats;
    }

    public IEnumerator SetXPSmooth(bool reset = false)
    {
         if (xpBar == null) yield break;

        if (reset)
            xpBar.transform.localScale = new Vector3(0, 1, 1);
         
         float normalizedXP = GetNormalizedXP();
         yield return xpBar.transform.DOScaleX(normalizedXP, 1.5f).WaitForCompletion();
         
    }

    float GetNormalizedXP()
    {
         int currentLevelXP = _monster.Base.GetXPForLevel(_monster.Level);
         int nextLevelXP = _monster.Base.GetXPForLevel(_monster.Level + 1);
         
         float normalizedXP = (float)(_monster.XP - currentLevelXP) / (nextLevelXP - currentLevelXP);    
         return Mathf.Clamp01(normalizedXP);
    }

    public IEnumerator UpdateHP()
    {
        if (_monster.HPChanged)
        {
            SetHPBarColor(_monster);
            yield return hpBar.SetHPSmooth((float)_monster.HP / _monster.MaxHP, hpBarColor);
            hpDisplayText.text = $"{_monster.HP} / {_monster.MaxHP}";
            _monster.HPChanged = false;
        }

    }
}
