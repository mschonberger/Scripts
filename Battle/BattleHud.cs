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
        levelText.text = "Lvl " + monster.Level;
        hpDisplayText.text = $"{monster.HP} / {monster.MaxHP}";
        hpBar.SetHP((float)monster.HP / monster.MaxHP);
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
         if (xpBar == null) return;
         
         float normalizedXP = GetNormalizedXP();
         xpBar.transform.localScale = new Vector3 (normalizedXP, 1, 1);
    }
    
    public IEnumerator SetXPSmooth()
    {
         if (xpBar == null) yield break;
         
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
            yield return hpBar.SetHPSmooth((float)_monster.HP / _monster.MaxHP);
            hpDisplayText.text = $"{_monster.HP} / {_monster.MaxHP}";
            _monster.HPChanged = false;
        }

    }
}
