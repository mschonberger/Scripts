using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColor;

    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject blackBox;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Image> actionBorder;
    [SerializeField] List<SpriteRenderer> actionSprite;

    [SerializeField] List<Text> moveTexts;
    [SerializeField] List<Image> moveBorder;
    [SerializeField] List<Text> movePP;
    [SerializeField] List<Text> moveType;

    [SerializeField] Text yesText;
    [SerializeField] Text noText;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public void EnableBlackBox(bool enabled)
    {
        blackBox.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction, Color highlight)
    {
        for (int i=0; i<actionTexts.Count; ++i)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = highlight;
                actionBorder[i].color = highlight;
                actionSprite[i].color = highlight;
            }
            else
            {
                actionTexts[i].color = Color.black;
                actionBorder[i].color = Color.black;
                actionSprite[i].color = Color.black;
            }
        } 
    }

    public void UpdateChoiceBox(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = highlightedColor;
            noText.color = Color.black;
        } else
        {
            yesText.color = Color.black;
            noText.color = highlightedColor;
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i=0; i<moveTexts.Count; ++i)
        {
            if (i == selectedMove)
            {
                moveTexts[i].color = highlightedColor;
                moveBorder[i].color = highlightedColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
                moveBorder[i].color = Color.black;
            }
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i=0; i<moveTexts.Count; ++i)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
                moveType[i].text = $"{moves[i].Base.Type}";
                movePP[i].text = $"{moves[i].PP} / {moves[i].Base.PP}";

                if (moves[i].PP == 0)
                    movePP[i].color = Color.red;
                else if (moves[i].PP <= moves[i].Base.PP / 2)
                    movePP[i].color = new Color(1f, 0.647f, 0f);
                else
                    movePP[i].color = Color.black;
            }
            else
            {
                moveTexts[i].text = "-";
                movePP[i].text = "- / -";
                moveType[i].text = "-";
            }
        }
    }
}
