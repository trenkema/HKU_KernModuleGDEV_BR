using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class _PlayerGDEV : MonoBehaviour, IDamageable
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

    private Vector3 movement;
    private Rigidbody rb;

    private bool gameOver = false;

    private float timeInZone = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        healthSlider.gameObject.SetActive(true);
        healthSlider.value = GetHealthPercentage();
        healthText.text = "+ " + currentHealth;
        playerHUD_Animator.SetTrigger("StartGame");
        Debug.Log("SPAWNED");

        EventSystem.Subscribe(Event_Type.END_GAME, GameOver);
    }

    private void OnDisable()
    {
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
    }

    private void Update()
    {
        Look();

        movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }

    private void FixedUpdate()
    {
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

    public void TakeDamageOverTime(int _damage, int _duration, int _timeBetweenDamage, string _killerSource)
    {
        throw new System.NotImplementedException();
    }

    public void Died(string _killerSource)
    {
        string killerText = _killerSource.ToUpper();
        //EventSystem<string, string>.RaiseEvent(Event_Type.PLAYER_KILLED, "Enemy", killerText);
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
        EventSystem.RaiseEvent(Event_Type.PLAYER_DEATH);
    }

    public void GameOver()
    {
        gameOver = true;
        rb.velocity = Vector3.zero;
    }

    private void OnTriggerStay(Collider other)
    {
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
