using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector2 velocity;
    private GameObject owner;
    private Rigidbody2D rb;
    private float time;
    private bool checkEndTurn;
    [SerializeField] float bulletLifeTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        Invoke("TimeOutEndTurn", bulletLifeTime);
    }
    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        Vector2 velocity = Projectile(time);
        rb.linearVelocity = velocity;
        UpdateRotation(velocity);
    }
    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }
    Vector2 Projectile(float time)
    {
        float vectorX = velocity.x;
        float vectorY = velocity.y - Mathf.Abs(Physics2D.gravity.y) * time;
        Debug.Log(vectorX + " " + vectorY);
        return new Vector2(vectorX, vectorY);
    }
    void UpdateRotation(Vector2 currentVelocity)
    {
        float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle); 
    }
    void TimeOutEndTurn()
    {
        if(!checkEndTurn)
        {
            EndTurn();
            Destroy(gameObject);
        }
    }
    void EndTurn()
    {
        if (!checkEndTurn)
        {
            checkEndTurn = true;
            GameMenager.instance.NotifyEndTurn();
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (checkEndTurn || collision.gameObject == owner) return;
        Invoke("EndTurn", 1.5f);
        Destroy(gameObject, 1.5f); 
    }
}
