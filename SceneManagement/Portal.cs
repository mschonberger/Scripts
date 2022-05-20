using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, INTERFACEPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] Transform spawnPoint;
    [SerializeField] DestinationIdentifier destinationPortal;

    PlayerController player;
    Fader fader;

    public Vector2 cameraChangeMin;
    public Vector2 cameraChangeMax;
    private CameraMovement cam;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
        cam = Camera.main.GetComponent<CameraMovement>();
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destinationPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.Character.SetPositionAndSnapToTile(destinationPortal.SpawnPoint.position);

        cam.minPosition.x += cameraChangeMin.x;
        cam.minPosition.y += cameraChangeMin.y;
        cam.maxPosition.x += cameraChangeMax.x;
        cam.maxPosition.y += cameraChangeMax.y;

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);

        Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;

}

public enum DestinationIdentifier { A, B, C, D, E, F, G, H}
