using UnityEngine;

public class ZoneScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth playerHeal =  collision.transform.parent.gameObject.GetComponent<PlayerHealth>();
        Debug.Log(collision.gameObject.name);
        if (playerHeal != null )
        {
            Debug.Log("Fall");
            playerHeal.currentHealth = 0;
            playerHeal.TakeDamage(true, Vector2.zero);
        }
    }
}
