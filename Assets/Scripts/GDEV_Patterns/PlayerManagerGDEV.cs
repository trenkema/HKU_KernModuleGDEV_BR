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

    // Private Non-Inspector Variables
    private GameObject controller;

    private bool hasStarted = false;
    private bool isAlive = false;

    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;

        CreateController();

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
        //GameObject playerSpawnPoint = GameController.instance.playerSpawnPoints[Random.Range(0, GameController.instance.playerSpawnPoints.Length)];
        controller = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        //GameController.instance.networkPlaySound = PhotonNetwork.Instantiate("NetworkPlaySound", Vector3.zero, Quaternion.identity).GetComponent<NetworkPlaySound>();

        controller.GetComponent<_PlayerGDEV>().enabled = false;

        isAlive = true;

        //StartCoroutine(AddPlayerCount());
    }

    public void PlayerDied()
    {
        isAlive = false;

        // CreateController(); To Respawn Player
    }

    //IEnumerator AddPlayerCount()
    //{
    //    float test = Random.Range(0.5f, 3f);
    //    yield return new WaitForSeconds(test);
    //    playersAliveCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["PlayersAlive"];
    //    playersAliveCount++;
    //    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayersAlive", playersAliveCount } });
    //}

    private void StartGame()
    {
        int randomSpawnpoint = Random.Range(0, GameControllerGDEV.instance.playerSpawnPoints.Length);
        controller.transform.position = GameControllerGDEV.instance.playerSpawnPoints[randomSpawnpoint].transform.position;

        controller.GetComponent<_PlayerGDEV>().enabled = true;
    }

    public void LeaveRoom()
    {
        if (isAlive)
        {
            EventSystem<string, string>.RaiseEvent(Event_Type.PLAYER_KILLED, "Enemy", "LEAVING");
        }

        EventSystem.Unsubscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Unsubscribe(Event_Type.PLAYER_DEATH, PlayerDied);
        SceneManager.LoadScene(0);
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
}
