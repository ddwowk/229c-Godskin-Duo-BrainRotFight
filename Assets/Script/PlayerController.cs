using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 onClickPosition, currentPosition,shootVelocity;
    [SerializeField] private TextMeshProUGUI powerText, angleText;

    [SerializeField] private int maxShootPower;
    [SerializeField] private int shootmultiple;

    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootTransform;
    private bool isAiming;
    private void Awake()
    {
    }
    void Update()
    {
        Attack();
    }
    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            onClickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isAiming = true;
        }

        if (isAiming && Input.GetMouseButton(0))
        {
            currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 powerAndDirCal = currentPosition - onClickPosition;
            shootVelocity = -powerAndDirCal * shootmultiple;
            shootVelocity = Vector2.ClampMagnitude(shootVelocity, maxShootPower);
            UIUpdate(shootVelocity.magnitude);
        }

        if (isAiming && Input.GetMouseButtonUp(0))
        {
            Shoot(shootVelocity);
            isAiming = false;
            shootVelocity = Vector2.zero;
            UIUpdate(0);
        }
    }
    void Shoot(Vector2 shootVelocity)
    {
        GameObject bulletClone = Instantiate(bullet, shootTransform.position, Quaternion.identity);
        Bullet bulletScript = bulletClone.GetComponent<Bullet>();
        bulletScript.velocity = shootVelocity;
    }
    void UIUpdate(float shootVector)
    {
        float powerCal = Mathf.Clamp01(shootVector/maxShootPower) * 100;
        powerText.text = powerCal.ToString();
    }
}