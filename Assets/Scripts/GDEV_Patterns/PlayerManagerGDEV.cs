using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerManagerGDEV : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject HUD;

    [SerializeField] GameObject winnerScreen;
    [SerializeField] GameObject loserScreen;

    [SerializeField] private int mainMenuID;

    [SerializeField] GameObject playerPrefab;

    [SerializeField] MeshRenderer[] objectsToHide;

    [SerializeField] GameObject effectsOnButton;
    [SerializeField] GameObject effectsOffButton;

    // Private Non-Inspector Variables
    private GameObject controller;

    private bool hasStarted = false;
    private bool isAlive = false;

    private bool effectsOn = false;

    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;

        CreateController();

        effectsOffButton.SetActive(true);
        effectsOnButton.SetActive(false);

        winnerScreen.SetActive(false);
        loserScreen.SetActive(false);

        EventSystem.Subscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Subscribe(Event_Type.PLAYER_DEATH, PlayerDied);
    }

    private void OnDisable()
    {
        EventSystem.Unsubscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Unsubscribe(Event_Type.PLAYER_DEATH, PlayerDied);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LeaveRoom();
        }
    }

    void CreateController()
    {
        foreach (var item in objectsToHide)
        {
            item.enabled = false;
        }

        controller = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        controller.GetComponent<_PlayerGDEV>().enabled = false;

        isAlive = true;
    }

    public void PlayerDied()
    {
        isAlive = false;

        Destroy(controller);

        controller = null;

        CheckIfGameOver(0);

        // CreateController(); To Respawn Player
    }

    private void StartGame()
    {
        int randomSpawnpoint = Random.Range(0, GameControllerGDEV.instance.playerSpawnPoints.Length);
        controller.transform.position = GameControllerGDEV.instance.playerSpawnPoints[randomSpawnpoint].transform.position;

        controller.GetComponent<_PlayerGDEV>().enabled = true;

        hasStarted = true;

        StartCoroutine(AddEffects());
    }

    private IEnumerator AddEffects()
    {
        yield return new WaitForSeconds(0.5f);

        if (effectsOn)
        {
            controller.GetComponent<GunManagerGDEV>().AddSpell(SpellType.Fire);
            controller.GetComponent<GunManagerGDEV>().AddSpell(SpellType.Ice);
        }
    }

    public void LeaveRoom()
    {
        if (isAlive)
        {
            EventSystem<string, string>.RaiseEvent(Event_Type.PLAYER_KILLED, "Enemy", "LEAVING");
        }

        EventSystem.Unsubscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Unsubscribe(Event_Type.PLAYER_DEATH, PlayerDied);
        SceneManager.LoadScene(2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void CheckIfGameOver(int _aliveCount)
    {
        if (hasStarted)
        {
            if (_aliveCount == 0)
            {
                EventSystem.RaiseEvent(Event_Type.END_GAME);

                if (isAlive)
                {
                    if (winnerScreen != null)
                    {
                        winnerScreen.SetActive(true);
                        loserScreen.SetActive(false);
                    }
                }
                else if (!isAlive)
                {
                    if (loserScreen != null)
                    {
                        loserScreen.SetActive(true);
                        winnerScreen.SetActive(false);
                    }
                }

                if (controller != null)
                {
                    controller.GetComponent<_Player>().GameOver();
                    controller.GetComponent<_Player>().enabled = false;
                }
            }
        }
    }

    public void ToggleEffects()
    {
        effectsOffButton.SetActive(!effectsOffButton.activeInHierarchy);
        effectsOnButton.SetActive(!effectsOnButton.activeInHierarchy);

        effectsOn = !effectsOn;
    }
}
