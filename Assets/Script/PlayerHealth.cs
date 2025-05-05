using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100;
    public float headshotDamageMultiplier = 100f / 3f;
    public float bodyShotDamage = 100f / 5f;
    [SerializeField] ParticleSystem blood;
    public Slider healthSlider;
    public float delayBeforeDestroy = 2f;
    public float knockbackForce = 1f;
    [SerializeField] GameObject winCanvas,player1,player2;
    private bool isDead = false;
    private Rigidbody2D rb;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        UpdateHealthUI();
    }

    public void TakeDamage(bool isHeadshot, Vector2 hitDirection)
    {
        if (isDead) return;

        float damageToTake = isHeadshot ? (maxHealth / 3f) : (maxHealth / 5f);
        ParticleSystem bloodClone = Instantiate(blood, gameObject.transform.position, Quaternion.identity);
        Destroy(bloodClone.gameObject ,1);
        currentHealth -= damageToTake;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        if (rb != null && hitDirection != Vector2.zero)
        {
            Vector2 knockbackDirection = hitDirection.normalized;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            if (healthSlider.maxValue != maxHealth)
            {
                healthSlider.maxValue = maxHealth;
            }
            healthSlider.value = currentHealth;
        }
    }

    void Die()
    {
        isDead = true;
        if(gameObject.name == "Player1 (1)")
        {
            player2.SetActive(true);
        }
        else if (gameObject.name == "Player2 (1)")
        {
            player1.SetActive(true);
        }
        winCanvas.SetActive(true);
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders) col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        gameObject.SetActive(false);
    }

}