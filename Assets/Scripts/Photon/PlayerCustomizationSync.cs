using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class PlayerCustomizationSync : MonoBehaviourPunCallbacks
{
    PhotonView PV;
    public GameObject[] hatObjects;
    public GameObject[] chestObjects;
    public GameObject[] pantsObjects;
    public GameObject[] shoesObjects;

    private int hatIndex, chestIndex, pantsIndex, shoesIndex;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        hatIndex = PersistentPlayerData.PPD.hatIndex;
        chestIndex = PersistentPlayerData.PPD.chestIndex;
        pantsIndex = PersistentPlayerData.PPD.pantsIndex;
        shoesIndex = PersistentPlayerData.PPD.shoesIndex;

        DisableAllObjects();
        SyncPlayerLook(hatIndex, chestIndex, pantsIndex, shoesIndex, true);
    }

    public void SyncPlayerLook(int _hatIndex, int _chestIndex, int _pantsIndex, int _shoesIndex, bool updateOwn)
    {
        if (updateOwn)
        {
            hatObjects[_hatIndex].SetActive(true);
            chestObjects[_chestIndex].SetActive(true);
            pantsObjects[_pantsIndex].SetActive(true);
            shoesObjects[_shoesIndex].SetActive(true);
        }

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("hatIndex", hatIndex);
            hash.Add("chestIndex", chestIndex);
            hash.Add("pantsIndex", pantsIndex);
            hash.Add("shoesIndex", shoesIndex);
            PhotonNetwork.SetPlayerCustomProperties(hash);
        }
    }

    public void DisableAllObjects()
    {
        foreach (GameObject gObject in hatObjects)
        {
            gObject.SetActive(false);
        }

        foreach (GameObject gObject in chestObjects)
        {
            gObject.SetActive(false);
        }

        foreach (GameObject gObject in pantsObjects)
        {
            gObject.SetActive(false);
        }

        foreach (GameObject gObject in shoesObjects)
        {
            gObject.SetActive(false);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            DisableAllObjects();
            SyncPlayerLook((int)changedProps["hatIndex"], (int)changedProps["chestIndex"], (int)changedProps["pantsIndex"], (int)changedProps["shoesIndex"], true);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SyncPlayerLook(hatIndex, chestIndex, pantsIndex, shoesIndex, false);
    }
}
