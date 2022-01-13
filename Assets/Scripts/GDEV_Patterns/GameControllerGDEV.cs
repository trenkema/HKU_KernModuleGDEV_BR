using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

public class GameControllerGDEV : MonoBehaviour
{
    public static GameControllerGDEV instance;

    public ObjectPooler[] objectPoolers;

    [HideInInspector] public GunManagerGDEV gunManager;

    [Header("Lobby Settings")]
    [SerializeField] private GameObject waitBlackScreen;
    [Space(5)]
    public GameObject[] playerSpawnPoints;

    [Header("Base Weapon Button Settings")]
    [SerializeField] private Color startWeaponButtonSelectedColor;
    [SerializeField] private Color startWeaponButtonUnSelectedColor;
    [SerializeField] Image[] baseWeaponButtonImages;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
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

    private bool startingGame;

    private bool gameOver = false;

    private void Awake()
    {
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

        timerToWaitForPhase = inbetweenPhaseTime;
        timerToClosePhase = timeToClosePhase;

        phaseIndex = 0;
        finalPhaseReached = false;
        zoneStarted = false;
        waitingForZone = true;

        EventSystem.Subscribe(Event_Type.END_GAME, GameOver);
    }

    private void OnDisable()
    {
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
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
    }

    private void RPC_SetNewPhasePosition(int _phaseIndex, Vector3 _newPosition)
    {
        wallPhasesParents[_phaseIndex].transform.position = _newPosition;
    }

    private void Update()
    {
        if (!finalPhaseReached && startingGame && waitingForZone && !zoneStarted)
        {
            WaitForPhase();
        }
        else if (!finalPhaseReached && startingGame && !waitingForZone && zoneStarted)
        {
            StartPhase(phaseIndex);
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

        SetNewPhasePosition(phaseIndex);
    }

    public void StartGame()
    {
        List<int> spawnPoints = new List<int>();

        for (int i = 0; i < playerSpawnPoints.Length; i++)
        {
            spawnPoints.Add(i);
        }

        int randomIndex = Random.Range(0, spawnPoints.Count);

        spawnPoints.RemoveAt(randomIndex);

        waitBlackScreen.GetComponent<Animator>().SetTrigger("StartGame");

        EventSystem.RaiseEvent(Event_Type.START_GAME);

        startingGame = true;

        SetNewPhasePosition(phaseIndex);

        StartCoroutine(SpawnEnemy()); // Start Spawning Enemies
    }

    IEnumerator SpawnEnemy()
    {
        while (!gameOver)
        {
            for (int i = 0; i < amountOfEnemiesPerSpawn; i++)
            {
                Instantiate(enemyPrefab, enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)].transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    public List<_PlayerGDEV> GetPlayers()
    {
        List<_PlayerGDEV> players = new List<_PlayerGDEV>();

        _PlayerGDEV[] alivePlayers = FindObjectsOfType<_PlayerGDEV>();

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
