using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    PhotonView PV;

    [Header("References")]
    [SerializeField] private GameObject HUD;
    [SerializeField] private TextMeshProUGUI playersAliveText;
    [SerializeField] private TextMeshProUGUI spectatingText;
    [SerializeField] private GameObject previousSpecButton;
    [SerializeField] private GameObject nextSpecButton;

    [SerializeField] GameObject winnerScreen;
    [SerializeField] GameObject loserScreen;

    [SerializeField] private int mainMenuID;

    // Private Non-Inspector Variables
    private GameObject controller;

    private bool canSpawnPlayer = false;
    private bool hasStarted = false;
    private bool isAlive = false;

    private _Player[] players;
    private int playerID;

    private int spectateID = 0;
    private bool getSpectaters = true;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(HUD);
            return;
        }

        isAlive = true;

        CreateController();
        playerID = PhotonNetwork.CurrentRoom.PlayerCount;

        spectatingText.text = "";
        previousSpecButton.SetActive(false);
        nextSpecButton.SetActive(false);
        playersAliveText.enabled = false;
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
        if (!isAlive && PV.IsMine && getSpectaters)
        {
            SpectateCamera();
        }

        if (Input.GetKeyDown(KeyCode.L) && PV.IsMine)
        {
            LeaveRoom();
        }
    }

    void CreateController()
    {
        //GameObject playerSpawnPoint = GameController.instance.playerSpawnPoints[Random.Range(0, GameController.instance.playerSpawnPoints.Length)];
        controller = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0, new object[] { PV.ViewID });
        //GameController.instance.networkPlaySound = PhotonNetwork.Instantiate("NetworkPlaySound", Vector3.zero, Quaternion.identity).GetComponent<NetworkPlaySound>();

        if (PV.IsMine)
        {
            controller.GetComponent<_Player>().enabled = false;
        }

        isAlive = true;

        //StartCoroutine(AddPlayerCount());
    }

    public void PlayerDied()
    {
        isAlive = false;
        GameController.instance.GetPlayers();

        if (PV.IsMine)
        {
            PhotonNetwork.Destroy(controller);

            if (spectatingText != null)
            {
                spectatingText.text = "SPECTATING";
                previousSpecButton.SetActive(true);
                nextSpecButton.SetActive(true);
            }

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("PlayersAlive"))
            {
                string test = PhotonNetwork.CurrentRoom.CustomProperties["PlayersAlive"].ToString();
                int aliveCount = int.Parse(test);
                aliveCount--;
                Hashtable setPlayersAlive = new Hashtable();
                setPlayersAlive.Add("PlayersAlive", aliveCount);
                PhotonNetwork.CurrentRoom.SetCustomProperties(setPlayersAlive);
            }
        }

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
        if (PV.IsMine)
        {
            canSpawnPlayer = true;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("SpawnID") && !hasStarted && canSpawnPlayer)
        {
            if (PV.IsMine)
            {
                hasStarted = true;
                string test = PhotonNetwork.LocalPlayer.CustomProperties["SpawnID"].ToString();
                int spawnID = int.Parse(test);
                controller.transform.position = GameController.instance.playerSpawnPoints[spawnID].transform.position;

                playersAliveText.enabled = true;
                PhotonNetwork.CurrentRoom.CustomProperties["PlayersAlive"] = PhotonNetwork.CurrentRoom.PlayerCount;
                UpdateHUD();
                controller.GetComponent<_Player>().enabled = true;
            }
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        string test = PhotonNetwork.CurrentRoom.CustomProperties["PlayersAlive"].ToString();
        int aliveCount = int.Parse(test);

        UpdateHUD();

        CheckIfGameOver(aliveCount);
    }

    void UpdateHUD()
    {
        if (PV.IsMine && playersAliveText != null)
            playersAliveText.text = PhotonNetwork.CurrentRoom.CustomProperties["PlayersAlive"].ToString() + " LEFT";
    }

    public void LeaveRoom()
    {
        if (isAlive)
        {
            EventSystem<string, string>.RaiseEvent(Event_Type.PLAYER_KILLED, PV.Controller.NickName, "LEAVING");
        }

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        EventSystem.Unsubscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Unsubscribe(Event_Type.PLAYER_DEATH, PlayerDied);
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SpectateCamera()
    {
        players = FindObjectsOfType<_Player>();

        if (players.Length > 0)
        {
            if (players[spectateID] != null)
            {
                Camera.main.transform.position = new Vector3(players[spectateID].gameObject.transform.position.x, Camera.main.transform.position.y, players[spectateID].gameObject.transform.position.z);
            }

            if (players[spectateID] == null)
            {
                spectateID = 0;
                players = FindObjectsOfType<_Player>();

                if (players.Length <= 0 || players[spectateID] == null)
                {
                    getSpectaters = false;
                    Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
                }
                else
                {
                    Camera.main.transform.position = new Vector3(players[spectateID].gameObject.transform.position.x, Camera.main.transform.position.y, players[spectateID].gameObject.transform.position.z);
                }
            }
        }
        else if (players.Length <= 0)
        {
            getSpectaters = false;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
        }
    }

    public void ChangeSpectator(int _upDown)
    {
        players = FindObjectsOfType<_Player>();

        spectateID = spectateID + _upDown;

        if (spectateID < 0)
        {
            spectateID = players.Length - 1;
        }
        else if (spectateID > players.Length - 1)
        {
            spectateID = 0;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PV.IsMine)
        {
            if (!hasStarted)
            {
                if (playerID != 1)
                {
                    playerID--;
                }
            }

            _Player[] alivePlayers = FindObjectsOfType<_Player>();

            PhotonNetwork.CurrentRoom.CustomProperties["PlayersAlive"] = alivePlayers.Length - 1;

            UpdateHUD();

            CheckIfGameOver(alivePlayers.Length - 1);

            spectateID = 0;
        }
    }

    public void CheckIfGameOver(int _aliveCount)
    {
        if (hasStarted && PV.IsMine)
        {
            if (_aliveCount == 1)
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
                if (playersAliveText != null)
                    playersAliveText.enabled = false;

                if (controller != null)
                {
                    controller.GetComponent<_Player>().GameOver();
                    controller.GetComponent<_Player>().enabled = false;
                }
            }
        }
    }
}
