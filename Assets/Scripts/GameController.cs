using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

public class GameController : MonoBehaviourPunCallbacks
{
    private PhotonView PV;

    public static GameController instance;

    [HideInInspector] public GunManager gunManager;
    public NetworkPlaySound networkPlaySound;

    [Header("Lobby Settings")]
    [SerializeField] private GameObject waitBlackScreen;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI timerToStartText;
    [Space(5)]
    [SerializeField] private float maxWaitTime;
    [SerializeField] private float maxFullGameWaitTime;
    [SerializeField] int minPlayersToStart;
    public GameObject[] playerSpawnPoints;

    [Header("Base Weapon Button Settings")]
    [SerializeField] private Color startWeaponButtonSelectedColor;
    [SerializeField] private Color startWeaponButtonUnSelectedColor;
    [SerializeField] Image[] baseWeaponButtonImages;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private int amountOfEnemiesPerSpawn = 1;
    [SerializeField] private GameObject[] enemySpawnPoints;

    [Header("Phase Settings")]
    [SerializeField] private float phaseXMovement;
    [SerializeField] private float phaseZMovement;
    [SerializeField] private GameObject[] wallPhasesParents;
    [SerializeField] private GameObject[] wallPhasesBarriers;
    [SerializeField] private GameObject[] wallPhasesZoneVisual;
    [SerializeField] private GameObject[] wallPhasesDamageableZone;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI phaseTimer;
    [SerializeField] private Material phaseClosedMaterial;
    [SerializeField] private float inbetweenPhaseTime;
    [SerializeField] private float timeToClosePhase;

    // Private Non-Inspector Variables
    private float timerToWaitForPhase;
    private float timerToClosePhase;
    private int phaseIndex = 0;
    private bool waitingForZone;
    private bool zoneStarted;
    private bool finalPhaseReached;

    private bool readyToCountDown;
    private bool readyToStart;
    private bool startingGame;
    private float fullGameTimer;
    private float timerToStartGame;
    private float notFullGameTimer;

    private int playerCount;
    private int roomSize;
    private bool gameOver = false;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        waitBlackScreen.SetActive(true);

        fullGameTimer = maxFullGameWaitTime;
        notFullGameTimer = maxWaitTime;
        timerToStartGame = maxWaitTime;

        timerToWaitForPhase = inbetweenPhaseTime;
        timerToClosePhase = timeToClosePhase;

        phaseIndex = 0;
        finalPhaseReached = false;
        zoneStarted = false;
        waitingForZone = true;

        EventSystem.Subscribe(Event_Type.END_GAME, GameOver);

        PlayerCountUpdate();
    }

    private void OnDisable()
    {
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
    }

    // Waiting Stuff
    void PlayerCountUpdate()
    {
        playerCount = PhotonNetwork.PlayerList.Length;
        roomSize = PhotonNetwork.CurrentRoom.MaxPlayers;
        playerCountText.text = "WAITING FOR PLAYERS: " + playerCount + " / " + roomSize;

        if (playerCount == roomSize)
        {
            readyToStart = true;
        }
        else if (playerCount >= minPlayersToStart)
        {
            readyToCountDown = true;
        }
        else
        {
            readyToCountDown = false;
            readyToStart = false;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerCountUpdate();

        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPC_SendTimer", RpcTarget.Others, timerToStartGame);
        }
    }

    [PunRPC]
    private void RPC_SendTimer(float _timeIn)
    {
        timerToStartGame = _timeIn;
        notFullGameTimer = _timeIn;

        if (_timeIn < fullGameTimer)
        {
            fullGameTimer = _timeIn;
        }
    }

    private void SetNewPhasePosition(int _phaseIndex)
    {
        if (_phaseIndex == 0)
        {
            Vector3 wallPhasePosition = wallPhasesParents[_phaseIndex].transform.position;
            float wallPositionOffsetX = Random.Range(-phaseXMovement, phaseXMovement);
            float wallPositionOffsetZ = Random.Range(-phaseZMovement, phaseZMovement);
            wallPhasesParents[_phaseIndex].transform.position = new Vector3(wallPhasePosition.x + wallPositionOffsetX, wallPhasePosition.y, wallPhasePosition.z + wallPositionOffsetZ);
        }
        else
        {
            Vector3 wallPhasePreviousPosition = wallPhasesParents[_phaseIndex - 1].transform.position;
            float wallPositionOffsetX = Random.Range(-phaseXMovement, phaseXMovement);
            float wallPositionOffsetZ = Random.Range(-phaseZMovement, phaseZMovement);
            wallPhasesParents[_phaseIndex].transform.position = new Vector3(wallPhasePreviousPosition.x + wallPositionOffsetX, wallPhasePreviousPosition.y, wallPhasePreviousPosition.z + wallPositionOffsetZ);
        }

        PV.RPC("RPC_SetNewPhasePosition", RpcTarget.Others, _phaseIndex, wallPhasesParents[_phaseIndex].transform.position);
    }

    [PunRPC]
    private void RPC_SetNewPhasePosition(int _phaseIndex, Vector3 _newPosition)
    {
        wallPhasesParents[_phaseIndex].transform.position = _newPosition;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerCountUpdate();
    }

    private void Update()
    {
        WaitingForMorePlayers();

        if (!finalPhaseReached && startingGame && waitingForZone && !zoneStarted)
        {
            WaitForPhase();
        }
        else if (!finalPhaseReached && startingGame && !waitingForZone && zoneStarted)
        {
            StartPhase(phaseIndex);
        }
    }

    void WaitingForMorePlayers()
    {
        if (playerCount <= 1)
        {
            ResetTimer();
        }
        if (readyToStart)
        {
            fullGameTimer -= Time.deltaTime;
            timerToStartGame = fullGameTimer;
        }
        else if (readyToCountDown)
        {
            notFullGameTimer -= Time.deltaTime;
            timerToStartGame = notFullGameTimer;
        }

        string tempTimer = string.Format("{0:00}", timerToStartGame);
        timerToStartText.text = "Starting In: " + tempTimer;

        if (timerToStartGame <= 0f)
        {
            if (startingGame)
                return;

            StartGame();
        }
    }

    private void WaitForPhase()
    {
        timerToWaitForPhase -= Time.deltaTime;
        string tempTimer = string.Format("{0:00}", timerToWaitForPhase);
        phaseTimer.text = "Zone Coming In: " + tempTimer;

        if (timerToWaitForPhase <= 0f)
        {
            if (zoneStarted)
                return;

            waitingForZone = false;
            zoneStarted = true;
            StartPhase(phaseIndex);
        }
    }

    private void StartPhase(int _phaseIndex)
    {
        timerToClosePhase -= Time.deltaTime;
        string tempTimer = string.Format("{0:00}", timerToClosePhase);
        phaseTimer.text = "Zone Closes In: " + tempTimer;
        wallPhasesZoneVisual[_phaseIndex].SetActive(true);

        if (timerToClosePhase <= 0f)
        {
            if (waitingForZone)
                return;

            SetZoneActive(_phaseIndex);
        }
    }

    private void SetZoneActive(int _phaseIndex)
    {
        // Set Zone Damage On
        // Change Zone Color To Damage Color
        if (phaseIndex != 0)
        {
            wallPhasesZoneVisual[_phaseIndex - 1].SetActive(false);
            wallPhasesDamageableZone[_phaseIndex - 1].SetActive(false);
            wallPhasesBarriers[_phaseIndex - 1].SetActive(false);
        }

        wallPhasesBarriers[_phaseIndex].SetActive(true);
        wallPhasesZoneVisual[_phaseIndex].GetComponent<MeshRenderer>().material = phaseClosedMaterial;
        wallPhasesDamageableZone[_phaseIndex].SetActive(true);

        if (phaseIndex != wallPhasesParents.Length - 1)
        {
            phaseIndex++;
            ResetZoneTimer();
        }
        else
        {
            finalPhaseReached = true;
            phaseTimer.text = "Final Zone Closed";
        }
    }

    private void ResetZoneTimer()
    {
        timerToWaitForPhase = inbetweenPhaseTime;
        timerToClosePhase = timeToClosePhase;
        waitingForZone = true;
        zoneStarted = false;

        if (PhotonNetwork.IsMasterClient)
        {
            SetNewPhasePosition(phaseIndex);
        }
    }

    void ResetTimer()
    {
        timerToStartGame = maxWaitTime;
        notFullGameTimer = maxWaitTime;
        fullGameTimer = maxFullGameWaitTime;
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            List<int> spawnPoints = new List<int>();

            for (int i = 0; i < playerSpawnPoints.Length; i++)
            {
                spawnPoints.Add(i);
            }

            foreach (Player pl in PhotonNetwork.PlayerList)
            {
                int randomIndex = Random.Range(0, spawnPoints.Count);
                Hashtable hash = new Hashtable();
                hash.Add("SpawnID", spawnPoints[randomIndex]);
                pl.SetCustomProperties(hash);

                spawnPoints.RemoveAt(randomIndex);
            }
        }

        waitBlackScreen.GetComponent<Animator>().SetTrigger("StartGame");

        EventSystem.RaiseEvent(Event_Type.START_GAME);

        GetPlayers();

        startingGame = true;

        Destroy(playerCountText);
        Destroy(timerToStartText);

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        SetNewPhasePosition(phaseIndex);

        PhotonNetwork.CurrentRoom.IsOpen = false;

        StartCoroutine(SpawnEnemy()); // Start Spawning Enemies
    }

    IEnumerator SpawnEnemy()
    {
        while (!gameOver)
        {
            for (int i = 0; i < amountOfEnemiesPerSpawn; i++)
            {
                PhotonNetwork.Instantiate("Enemy", enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)].transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    public List<_Player> GetPlayers()
    {
        List<_Player> players = new List<_Player>();

        _Player[] alivePlayers = FindObjectsOfType<_Player>();

        for (int i = 0; i < alivePlayers.Length; i++)
        {
            players.Add(alivePlayers[i]);
        }

        return players;
    }

    public void SetBaseWeapon(int _index)
    {
        if (gunManager != null)
        {
            gunManager.SetSpawnWeapon(_index);
        }

        foreach (var item in baseWeaponButtonImages)
        {
            item.color = startWeaponButtonUnSelectedColor;
        }

        baseWeaponButtonImages[_index].color = startWeaponButtonSelectedColor;
    }

    public void GameOver()
    {
        gameOver = true;

        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }
}
