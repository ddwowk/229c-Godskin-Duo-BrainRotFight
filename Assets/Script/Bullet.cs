using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public Vector2 velocity;
    private GameObject owner;
    private Rigidbody2D rb;
    private float time;
    private bool checkEndTurn = false;
    private bool hasCollided = false;
    [SerializeField] float bulletLifeTime;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip projectileSound;
    [SerializeField] private AudioClip endTurnSound;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Invoke("TimeOutEndTurn", bulletLifeTime);
    }

    public void Initialize(Vector2 startVelocity, GameObject shooter)
    {
        this.velocity = startVelocity;
        this.owner = shooter;
        this.time = 0f;
        this.checkEndTurn = false;
        this.hasCollided = false;
        if (audioSource != null && projectileSound != null)
        {
            audioSource.clip = projectileSound;
            audioSource.Play();
        }
        Invoke("TimeOutEndTurn", bulletLifeTime);
    }

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }

    private void FixedUpdate()
    {
        if (hasCollided) return;
        time += Time.fixedDeltaTime;
        Vector2 currentVelocity = Projectile(time);
        rb.linearVelocity = currentVelocity;
        UpdateRotation(currentVelocity);
    }

    Vector2 Projectile(float time)
    {
        float vectorX = velocity.x * time;
        float vectorY = velocity.y - Mathf.Abs(Physics2D.gravity.y) * time;
        return new Vector2(vectorX, vectorY);
    }

    void UpdateRotation(Vector2 currentVelocity)
    {
        if (currentVelocity == Vector2.zero) return;
        float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
    }

    void TimeOutEndTurn()
    {
        if (!checkEndTurn && !hasCollided)
        {
            hasCollided = true;
            EndTurn();
            Destroy(gameObject);
        }
    }

    void EndTurn()
    {
        if (!checkEndTurn && GameMenager.instance != null)
        {
            checkEndTurn = true;
            if (audioSource != null && endTurnSound != null)
            {
                audioSource.clip = endTurnSound;
                audioSource.Play();
            }
            GameMenager.instance.NotifyEndTurn();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (checkEndTurn || hasCollided) return;

        Vector2 impactVelocity = rb != null ? rb.linearVelocity : Vector2.zero;

        PlayerHealth targetHealth = collision.transform.parent?.GetChild(0).GetComponent<PlayerHealth>();

        if (targetHealth != null && (owner == null || targetHealth.gameObject != owner))
        {
            bool isHeadshot = collision.gameObject.CompareTag("PlayerHead");
            bool isBodyShot = collision.gameObject.CompareTag("PlayerBody");

            if (isHeadshot || isBodyShot)
            {
                Debug.Log("Hit");
                hasCollided = true;
                targetHealth.TakeDamage( isHeadshot, impactVelocity);

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.simulated = true;
                }

                Invoke("EndTurn", 1.5f);
                Destroy(gameObject, 1.5f);
                return;
            }
        }

        else
        {
            hasCollided = true;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = true;
            }
            Invoke("EndTurn", 1.5f);
            Destroy(gameObject, 1.5f);
        }
    }
}