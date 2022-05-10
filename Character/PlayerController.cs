using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Sprite sprite;
    [SerializeField] string playerName;
    [SerializeField] SpriteMask tallGrassMask;

    private Vector2 input;

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    public void Update()
    {
        if (Physics2D.OverlapCircle(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.GrassLayer) != null)
            tallGrassMask.gameObject.SetActive(true);
        else
            tallGrassMask.gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0; //Deaktivieren von diagonaler Bewegung

            if (input != Vector2.zero)
            {
               StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
            Interact();
    }

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);
    
        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<INTERFACEPlayerTriggerable>();
            if (triggerable != null)
            {
                character.Animator.IsMoving = false;
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }

    void Interact()
    {
        var facingDirection = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPosition = transform.position + facingDirection;

        // Debug.DrawLine(transform.position, interactPosition, Color.red, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPosition, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    public string Name
    {
        get => playerName;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;

}
