using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;

public class ScoreManager : MonoBehaviourPun
{
    private int score = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Point")
        {
            if (photonView.IsMine)
            {
                score++;

                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "P1SCORE", score } });
                }
                else
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "P2SCORE", score } });
                }
            }
        }
    }
}
