using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KillFeedItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] float destroyTime = 2f;

    public void Setup(string _playerSource, string _killerSource)
    {
        text.text = "<b>" + _playerSource + "</b>" + "KILLED BY " + "<i>" + _killerSource + "</i>";

        Destroy(gameObject, destroyTime);
    }
}
