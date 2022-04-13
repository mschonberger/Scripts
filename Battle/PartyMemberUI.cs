using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text hpDisplayText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Image faceIcon;

    [SerializeField] Color highlightedColor;


    Monster _monster;

    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.Base.Name;
        levelText.text = "Lvl " + monster.Level;
        hpDisplayText.text = $"{monster.HP} / {monster.MaxHP}";
        hpBar.SetHP((float)monster.HP / monster.MaxHP);
        faceIcon.sprite = monster.Base.FaceSprite;
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = highlightedColor;
        else
            nameText.color = Color.black;
    }
}
