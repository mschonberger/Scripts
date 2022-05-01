using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Die Klasse GameLayer wird für die Differenzierung der einzelnen Spielebenen genutzt.

Ebenen und ihre Funktion:
SolidObjects = Dient als Blockade bzw. Einschränkung für Spielerbewegung
Grass= Um Flächen zu markieren, wo wilde Monster erscheinen können
Interactable = Erlaubt es mit dem Spieler zu interagieren
Player = Ebene auf dem der Spieler zu finden ist
Field of View = Für die Sichtweite von Trainern oder um Events auszulösen.
*/

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fieldOfViewLayer;
    [SerializeField] LayerMask portalLayer;

    public static GameLayers i { get; set; }

    public void Awake()
    {
        i = this;
    }

    public LayerMask SolidLayer
    {
        get => solidObjectsLayer;
    }

    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }

    public LayerMask GrassLayer
    {
        get => grassLayer;
    }

    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }

    public LayerMask FieldOfViewLayer
    {
        get => fieldOfViewLayer;
    }

    public LayerMask PortalLayer
    {
        get => portalLayer;
    }

    public LayerMask TriggerableLayers
    {
        get => grassLayer | fieldOfViewLayer | portalLayer;
    }
}
