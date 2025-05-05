using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 onClickPosition, currentPosition,shootVelocity;
    [SerializeField] private TextMeshProUGUI powerText, angleText;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private int maxShootPower;
    [SerializeField] private int shootmultiple;

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float aimLineMaxLengthMultiple = 0.1f;
    
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootTransform;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip player1AimSound;
    [SerializeField] private AudioClip player2AimSound;

    private bool hasShootThisTurn;
    private bool isAiming;
    private bool isMyTurn;
    private bool isPlayer1;

    private void Awake()
    {
        lineRenderer.enabled = false;
        playerUI.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        isPlayer1 = gameObject.CompareTag("Player1"); // Assuming you have tags to differentiate players
    }
    void Update()
    {
        if (!isMyTurn || GameMenager.instance.GetWaitForAttack || hasShootThisTurn)
        {
            if (isAiming)
            {
                isAiming = false;
                shootVelocity = Vector2.zero;
                UIUpdate(0,0);
            }
            return;
        }
        Attack();
    }
    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            onClickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isAiming = true;
            lineRenderer.enabled = true;
            playerUI.SetActive(true);
        }

        if (isAiming && Input.GetMouseButton(0))
        {
            currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 powerAndDirCal = currentPosition - onClickPosition;
            shootVelocity = -powerAndDirCal * shootmultiple;
            shootVelocity = Vector2.ClampMagnitude(shootVelocity, maxShootPower);
            float currentAngle = 0f;
            currentAngle = Mathf.Atan2(shootVelocity.y, shootVelocity.x) * Mathf.Rad2Deg;
            UIUpdate(shootVelocity.magnitude, currentAngle);
            DrawLine(shootVelocity);

            // Play aiming sound
            if (!audioSource.isPlaying)
            {
                AudioClip aimSound = isPlayer1 ? player1AimSound : player2AimSound;
                audioSource.clip = aimSound;
                audioSource.Play();
            }
        }

        if (isAiming && Input.GetMouseButtonUp(0))
        {
            Shoot(shootVelocity);
            lineRenderer.enabled = false;
            playerUI.SetActive(false);
            isAiming = false;
            shootVelocity = Vector2.zero;
            UIUpdate(0, 0);
            audioSource.Stop();
        }
    }
    void Shoot(Vector2 shootVelocity)
    {
        GameObject bulletClone = Instantiate(bullet, shootTransform.position, Quaternion.identity);
        Bullet bulletScript = bulletClone.GetComponent<Bullet>();
        bulletScript.Initialize(shootVelocity, gameObject);
        CameraController.instance.SetTargetWithProjectileZoom(bulletScript.transform);
        hasShootThisTurn = true;
    }
    void DrawLine(Vector2 currentShootVelocity)
    {
        Vector3 startPoint = shootTransform.position;
        float lineLength = (currentShootVelocity.magnitude / maxShootPower) * (maxShootPower * aimLineMaxLengthMultiple); 
        lineLength = Mathf.Clamp(lineLength, 0, maxShootPower * aimLineMaxLengthMultiple); 
        Vector3 dir = currentShootVelocity.normalized;

        Vector3 endPoint = startPoint + dir * lineLength;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }
    void UIUpdate(float shootVector, float currentangle)
    {
        float powerCal = Mathf.Clamp01(shootVector/maxShootPower) * 100;
        powerText.text = "Power: " + powerCal.ToString();
        angleText.text = "Angle: " + currentangle.ToString();
    }
    public void SetMyTurn(bool isMyTurn)
    {
        this.isMyTurn = isMyTurn;
        if (this.isMyTurn)
        {
            hasShootThisTurn = false;
        }
    }
    
}