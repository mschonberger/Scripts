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
