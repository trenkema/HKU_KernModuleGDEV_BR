using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class KillFeed : MonoBehaviour
{
    PhotonView PV;
    [SerializeField] private Transform killFeedParent;
    [SerializeField] private GameObject killFeedItemPrefab;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        EventSystem<string, string>.Subscribe(Event_Type.PLAYER_KILLED, OnPlayerKilled);
    }

    private void OnDisable()
    {
        EventSystem<string, string>.Unsubscribe(Event_Type.PLAYER_KILLED, OnPlayerKilled);
    }

    private void OnPlayerKilled(string _playerSource, string _killerSource)
    {
        PV.RPC("RPC_OnPlayerKilled", RpcTarget.All, _playerSource, _killerSource);
    }

    [PunRPC]
    private void RPC_OnPlayerKilled(string _playerSource, string _killerSource)
    {
        if (!PV.IsMine)
            return;

        GameObject killFeed = Instantiate(killFeedItemPrefab, killFeedParent);
        killFeed.GetComponent<KillFeedItem>()?.Setup(_playerSource, _killerSource);
        killFeed.transform.SetAsFirstSibling();
    }
}
