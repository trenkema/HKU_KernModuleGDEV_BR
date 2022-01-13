using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerLightControl : MonoBehaviour
{
    [SerializeField] GameObject[] playerLight;
    [SerializeField] GameObject[] stealthLight;
    [SerializeField] Image lightIcon;
    [SerializeField] Sprite lightOn, lightOff;
    [SerializeField] float lightSwitchCooldown = 15f;
    [SerializeField] GameObject cantSwitchLightIcon;
    [SerializeField] TextMeshProUGUI cooldownText;

    PhotonView PV;

    private bool isLightOn = true;
    private bool canUseLight = true;
    private float cooldownTimeLeft;
    private bool gameOver = false;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            isLightOn = playerLight[0].activeInHierarchy;

            foreach (var light in playerLight)
            {
                light.SetActive(isLightOn);
            }

            foreach (var light in stealthLight)
            {
                light.SetActive(!isLightOn);
            }

            lightIcon.gameObject.SetActive(false);
            cantSwitchLightIcon.gameObject.SetActive(false);
            cooldownText.text = "";

            EventSystem.Subscribe(Event_Type.START_GAME, StartGame);
            EventSystem.Subscribe(Event_Type.END_GAME, GameOver);
        }
    }

    private void OnDisable()
    {
        EventSystem.Unsubscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.F) && canUseLight && !gameOver)
        {
            SwitchLight();
        }

        if (!canUseLight)
        {
            UpdateLightCooldown();
        }
    }

    private void StartGame()
    {
        if (!PV.IsMine)
            return;

        lightIcon.gameObject.SetActive(true);
        lightIcon.sprite = isLightOn ? lightOn : lightOff;
    }

    private void GameOver()
    {
        gameOver = true;
        EventSystem.Unsubscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
    }

    private void SwitchLight()
    {
        isLightOn = !isLightOn;

        foreach (var light in playerLight)
        {
            light.SetActive(isLightOn);
        }

        foreach (var light in stealthLight)
        {
            light.SetActive(!isLightOn);
        }

        lightIcon.sprite = isLightOn ? lightOn : lightOff;

        canUseLight = false;

        cantSwitchLightIcon.gameObject.SetActive(true);

        cooldownTimeLeft = lightSwitchCooldown;
    }

    private void UpdateLightCooldown()
    {
        cooldownTimeLeft -= Time.deltaTime;
        string tempTimer = string.Format("{0:00}", cooldownTimeLeft);
        cooldownText.text = tempTimer;

        if (cooldownTimeLeft <= 0f)
        {
            canUseLight = true;

            ResetLightCooldown();
        }
    }

    private void ResetLightCooldown()
    {
        cooldownText.text = "";
        cantSwitchLightIcon.SetActive(false);
    }
}
