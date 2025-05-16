using UnityEngine;
using UnityEngine.Tilemaps;

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
    [SerializeField] float explosionRadius = 1.5f;
    [SerializeField] Tilemap destructibleTilemap;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }
    public void Explode()
    {
        //if (explosionEffectPrefab != null)
        //{
        //    Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        //}
        GameObject tilemapObject = GameObject.Find("Grid");
        destructibleTilemap = tilemapObject.transform.Find("Ground").GetComponent<Tilemap>();
        Vector3 explosionCenter = transform.position;
        int radiusInCells = Mathf.CeilToInt(explosionRadius / destructibleTilemap.cellSize.x);

        Vector3Int centerCell = destructibleTilemap.WorldToCell(explosionCenter);

        for (int x = -radiusInCells; x <= radiusInCells; x++)
        {
            for (int y = -radiusInCells; y <= radiusInCells; y++)
            {
                Vector3Int currentCell = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                Vector3 cellWorldCenter = destructibleTilemap.GetCellCenterWorld(currentCell);
                float distanceToCenter = Vector2.Distance(new Vector2(explosionCenter.x, explosionCenter.y), new Vector2(cellWorldCenter.x, cellWorldCenter.y));

                if (distanceToCenter <= explosionRadius)
                {
                    if (destructibleTilemap.GetTile(currentCell) != null)
                    {
                        destructibleTilemap.SetTile(currentCell, null);
                    }
                }
            }
        }
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
        float distanceX = velocity.x * time;
        float distanceY = velocity.y - Mathf.Abs(Physics2D.gravity.y) * time;
        return new Vector2(distanceX, distanceY);
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

        PlayerHealth targetHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (targetHealth == owner) return;

        if (targetHealth != null && (owner == null || targetHealth.gameObject != owner))
        {
            bool isHeadshot = collision.collider.CompareTag("PlayerHead");
            bool isBodyShot = collision.collider.CompareTag("PlayerBody");

            if (isHeadshot || isBodyShot)
            {
                Debug.Log($"Hit {isHeadshot}");
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
        Explode();
    }
}