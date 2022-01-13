using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Photon.Pun;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    GameObject[] spawnPoints;

    [SerializeField]
    TextMeshProUGUI P1Score;
    [SerializeField]
    TextMeshProUGUI P2Score;
    [SerializeField]
    TextMeshProUGUI playersAliveText;

    private int currentAlivePlayers;

    PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            SpawnPlayer();
            UpdateHUD();
        }
    }

    void SpawnPlayer()
    {
        GameObject Player = PhotonNetwork.Instantiate("Player", spawnPoints[0].transform.position, Quaternion.identity);
        currentAlivePlayers = (int) PhotonNetwork.CurrentRoom.CustomProperties["PlayersAlive"];
        currentAlivePlayers++;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayersAlive", currentAlivePlayers } });
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        UpdateHUD();
    }

    void UpdateHUD()
    {
        playersAliveText.text = PhotonNetwork.CurrentRoom.CustomProperties["PlayersAlive"].ToString() + " LEFT";

        //P1Score.text = "P1 Score : " + PhotonNetwork.CurrentRoom.CustomProperties["P1SCORE"].ToString();
        //P2Score.text = "P2 Score : " + PhotonNetwork.CurrentRoom.CustomProperties["P2SCORE"].ToString();
    }
}
