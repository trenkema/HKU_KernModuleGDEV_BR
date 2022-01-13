using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class _Player : MonoBehaviourPunCallbacks, IDamageable
{
    [Header("Player Settings")]
    [SerializeField] float speed = 10f;
    [SerializeField] float currentHealth = 100;
    [SerializeField] float maxHealth = 100;

    [Header("References")]
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider healthSlider;
    [SerializeField] GameObject playerHUD;
    [SerializeField] GameObject gunHolder;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Animator playerHUD_Animator;

    // Private Non-Inspector Variables
    PhotonView PV;

    private Vector3 movement;
    private Rigidbody rb;

    private bool gameOver = false;

    private float timeInZone = 0f;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            rb = GetComponent<Rigidbody>();
            currentHealth = maxHealth;
            healthSlider.gameObject.SetActive(true);
            healthSlider.value = GetHealthPercentage();
            healthText.text = "+ " + currentHealth;
            PV.Controller.NickName = PV.Controller.UserId;
            playerHUD_Animator.SetTrigger("StartGame");
            Debug.Log("SPAWNED");
        }
        else
        {
            Destroy(playerHUD);
        }

        EventSystem.Subscribe(Event_Type.END_GAME, GameOver);
    }

    private void OnDisable()
    {
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        Look();

        movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        if (!gameOver)
            MoveCharacter(movement);
    }

    public void Look()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z);

        // Rotate Weapon In Right Direction
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f);
        Vector3 lookPos = Camera.main.ScreenToWorldPoint(mousePos);
        lookPos = lookPos - transform.position;
        float angle = Mathf.Atan2(lookPos.z, lookPos.x) * Mathf.Rad2Deg;
        gunHolder.transform.rotation = Quaternion.AngleAxis(-angle, Vector3.up);
    }

    public void MoveCharacter(Vector3 _direction)
    {
        rb.velocity = _direction * speed;
    }

    public void HealHealth(int _amount)
    {
        PV.RPC("RPC_HealHealth", RpcTarget.All, _amount);
    }

    [PunRPC]
    void RPC_HealHealth(int _amount)
    {
        if (!PV.IsMine)
            return;

        currentHealth += _amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        healthText.text = "+ " + currentHealth;
        healthSlider.value = GetHealthPercentage();
    }

    public void TakeDamage(int _damage, string _killerSource)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, _damage, _killerSource);
    }

    [PunRPC]
    void RPC_TakeDamage(int _damage, string _killerSource)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= _damage;
        healthText.text = "+ " + currentHealth;
        healthSlider.value = GetHealthPercentage();

        if (currentHealth <= 0)
        {
            Died(_killerSource);
            healthText.text = "";
            healthSlider.gameObject.SetActive(false);
        }
    }

    public void Died(string _killerSource)
    {
        if (!PV.IsMine)
            return;

        string killerText = _killerSource.ToUpper();
        EventSystem<string, string>.RaiseEvent(Event_Type.PLAYER_KILLED, PV.Controller.NickName.ToUpper(), killerText);
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
        EventSystem.RaiseEvent(Event_Type.PLAYER_DEATH);
    }

    public void GameOver()
    {
        if (PV.IsMine)
        {
            gameOver = true;
            rb.velocity = Vector3.zero;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!PV.IsMine)
            return;

        var phaseComponent = other.GetComponent<Phase>();

        if (phaseComponent != null && !gameOver)
        {
            timeInZone += Time.deltaTime;

            if (timeInZone >= phaseComponent.timeToTakeDamage)
            {
                timeInZone = 0f;
                TakeDamage(phaseComponent.damage, "THE ZONE");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PV.IsMine)
            return;

        if (other.GetComponent<Phase>())
        {
            timeInZone = 0f;
        }
    }

    private float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
