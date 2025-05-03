using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector2 velocity;
    private Rigidbody2D rb;
    private float time;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        Vector2 velocity = Projectile(time);
        rb.linearVelocity = velocity;
        UpdateRotation(velocity);
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
}
