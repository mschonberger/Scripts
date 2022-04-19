using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Die Klasse HPBar wird für die Darstellung der Lebenspunkte eines Monsters verwendet. */

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    public void SetHP(float hpNormalized, Color hpBarColor)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
        health.GetComponent<Image>().color = hpBarColor;
    }

    public IEnumerator SetHPSmooth(float newHP, Color hpBarColor)
    {
        float currentHP = health.transform.localScale.x; //Aktueller Stand der HP
        float changeAmount = currentHP - newHP; // Wert der abgezogen werden muss

        while (currentHP - newHP > Mathf.Epsilon) // Loop der läuft bis die Differenz zwischen aktuellem Wert und des neuen Wert ein sehr kleiner Wert ist  
        {
            currentHP -= changeAmount * Time.deltaTime;
            health.transform.localScale = new Vector3(currentHP, 1f);
            yield return null;
        }
        health.GetComponent<Image>().color = hpBarColor;
        health.transform.localScale = new Vector3(newHP, 1f);
    }
}