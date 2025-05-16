using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Shooting Components & Settings")]
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

    [Header("Movement & Physics")]
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 1f;

    [Header("Stamina Settings")]
    [SerializeField] float playerMaxStamina = 100f;
    [SerializeField] float moveStaminaDrainRate = 40f;
    [SerializeField] float jumpStaminaCost = 30f;
    private float currentPlayerStamina;
    public Slider staminaSlider;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Vector2 onClickPosition, currentPosition, shootVelocity;
    private bool hasShootThisTurn;
    private bool isAiming;
    private bool isMyTurn;
    private bool isPlayer1;
    private bool isGrounded;
    private float horizontalInput;

    private void Awake()
    {
        lineRenderer.enabled = false;
        playerUI.SetActive(false);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (rb2D == null) rb2D = GetComponent<Rigidbody2D>();

        isPlayer1 = gameObject.CompareTag("Player1");
    }

    void Update()
    {
        if (!isMyTurn || GameMenager.instance.GetWaitForAttack || hasShootThisTurn)
        {
            if (isAiming)
            {
                StopAimingAndReset();
            }
            if (rb2D != null) rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
            horizontalInput = 0;
            return;
        }

        Attack();
        staminaSlider.value = currentPlayerStamina;

        if (!isAiming)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

            if (groundCheckPoint != null)
            {
                isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
            }
            else
            {
                isGrounded = true;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryJump();
            }
            Debug.Log(isGrounded);
        }
        else
        {
            horizontalInput = 0;
        }
    }

    void FixedUpdate()
    {
        if (!isMyTurn || GameMenager.instance.GetWaitForAttack || hasShootThisTurn || isAiming)
        {
            if (rb2D != null && !isAiming)
            {
                if (Mathf.Abs(rb2D.linearVelocity.x) > 0.01f && horizontalInput == 0)
                {
                    rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
                }
            }
            return;
        }

        HandleMovement();
    }

    void Attack()
    {

        if (Input.GetMouseButtonDown(0) && !isAiming)
        {
            onClickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isAiming = true;
            lineRenderer.enabled = true;
            playerUI.SetActive(true);
            rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
            if (audioSource != null)
            {
                AudioClip aimSound = isPlayer1 ? player1AimSound : player2AimSound;
                audioSource.clip = aimSound;
                audioSource.Play();
            }
        }

        if (isAiming && Input.GetMouseButton(0))
        {
            currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 powerAndDirCal = currentPosition - onClickPosition;
            shootVelocity = -powerAndDirCal * shootmultiple;
            shootVelocity = Vector2.ClampMagnitude(shootVelocity, maxShootPower);

            float currentAngleDeg = 0f;
            if (shootVelocity.sqrMagnitude > 0.01f)
            {
                currentAngleDeg = Mathf.Atan2(shootVelocity.y, shootVelocity.x) * Mathf.Rad2Deg;
            }

            UIUpdate(shootVelocity.magnitude, currentAngleDeg);
            DrawLine(shootVelocity);
        }

        if (isAiming && Input.GetMouseButtonUp(0))
        {
            Shoot(shootVelocity);
            StopAimingAndReset();
        }
    }

    void HandleMovement()
    {
        if (Mathf.Abs(horizontalInput) > 0.01f && currentPlayerStamina > 0)
        {
            rb2D.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb2D.linearVelocity.y);
            currentPlayerStamina -= moveStaminaDrainRate * Time.fixedDeltaTime;
            if (currentPlayerStamina < 0) currentPlayerStamina = 0;
        }
        else if (Mathf.Abs(horizontalInput) < 0.01f || currentPlayerStamina <= 0)
        {
            rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
        }
    }

    void TryJump()
    {
        if (isGrounded && currentPlayerStamina >= jumpStaminaCost)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0f);
            rb2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currentPlayerStamina -= jumpStaminaCost;
            if (currentPlayerStamina < 0) currentPlayerStamina = 0;
        }
    }

    void StopAimingAndReset()
    {
        lineRenderer.enabled = false;
        playerUI.SetActive(false);
        isAiming = false;
        shootVelocity = Vector2.zero;
        UIUpdate(0, 0);
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void Shoot(Vector2 calculatedShootVelocity)
    {
        if (calculatedShootVelocity.sqrMagnitude < 0.1f)
        {
            Debug.Log("Shot power too low.");
            return;
        }
        GameObject bulletClone = Instantiate(bullet, shootTransform.position, Quaternion.identity);
        Bullet bulletScript = bulletClone.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(calculatedShootVelocity, gameObject);
            if (CameraController.instance != null)
            {
                CameraController.instance.SetTargetWithProjectileZoom(bulletScript.transform);
            }
        }
        else
        {
            Debug.LogError("Bullet prefab is missing Bullet script!");
        }
        hasShootThisTurn = true;
    }

    void DrawLine(Vector2 currentShootVelocity)
    {
        if (lineRenderer == null || shootTransform == null || maxShootPower <= 0) return;

        Vector3 startPoint = shootTransform.position;
        float normalizedPower = currentShootVelocity.magnitude / maxShootPower;
        float desiredLineLength = normalizedPower * (maxShootPower * aimLineMaxLengthMultiple);
        desiredLineLength = Mathf.Clamp(desiredLineLength, 0, maxShootPower * aimLineMaxLengthMultiple);

        Vector3 dir = Vector3.zero;
        if (currentShootVelocity.sqrMagnitude > 0.001f)
        {
            dir = currentShootVelocity.normalized;
        }

        Vector3 endPoint = startPoint + dir * desiredLineLength;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    void UIUpdate(float shootMagnitude, float currentAngle)
    {
        if (powerText == null || angleText == null) return;

        float powerPercentage = 0f;
        if (maxShootPower > 0)
        {
            powerPercentage = Mathf.Clamp01(shootMagnitude / maxShootPower) * 100f;
        }
        powerText.text = "Power: " + powerPercentage.ToString("F0");
        angleText.text = "Angle: " + currentAngle.ToString("F1");
    }

    public void SetMyTurn(bool myTurn)
    {
        this.isMyTurn = myTurn;
        if (this.isMyTurn)
        {
            hasShootThisTurn = false;
            currentPlayerStamina = playerMaxStamina;
            if (isAiming)
            {
                StopAimingAndReset();
            }
        }
        else
        {
            if (isAiming)
            {
                StopAimingAndReset();
            }
            if (rb2D != null) rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
            horizontalInput = 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}