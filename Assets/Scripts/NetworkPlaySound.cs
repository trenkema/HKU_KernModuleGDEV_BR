using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlaySound : MonoBehaviour
{
    [SerializeField] AudioClip[] audioClipLibrary;
    [SerializeField] AudioSource audioSource;

    PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

   public void PlaySoundAtLocation(Vector3 _location, AudioClip _audioClip)
    {
        for (int i = 0; i < audioClipLibrary.Length; i++)
        {
            if (_audioClip == audioClipLibrary[i])
            {
                PV.RPC("RPC_PlaySoundAtLocation", RpcTarget.Others, _location, i);
            }
        }
    }

    public void PlaySoundAtOwnTransform(AudioClip _audioClip)
    {
        for (int i = 0; i < audioClipLibrary.Length; i++)
        {
            if (_audioClip == audioClipLibrary[i])
            {
                PV.RPC("RPC_PlaySoundAtOwnTransform", RpcTarget.Others, i);
            }
        }
    }

    [PunRPC]
    private void RPC_PlaySoundAtLocation(Vector3 _location, int _index)
    {
        transform.position = new Vector3(_location.x, 0f, _location.z);
        audioSource.PlayOneShot(audioClipLibrary[_index]);
    }

    [PunRPC]
    private void RPC_PlaySoundAtOwnTransform(int _index)
    {
        audioSource.PlayOneShot(audioClipLibrary[_index]);
    }
}
