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
    [SerializeField] private float aimLineMaxLengthMultiple = 1.5f;
    
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootTransform;
    private bool hasShootThisTurn;
    private bool isAiming;
    private bool isMyTurn;

    private void Awake()
    {
        lineRenderer.enabled = false;
        playerUI.SetActive(false);
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
        }

        if (isAiming && Input.GetMouseButtonUp(0))
        {
            Shoot(shootVelocity);
            lineRenderer.enabled = false;
            playerUI.SetActive(false);
            isAiming = false;
            shootVelocity = Vector2.zero;
            UIUpdate(0, 0);
        }
    }
    void Shoot(Vector2 shootVelocity)
    {
        GameObject bulletClone = Instantiate(bullet, shootTransform.position, Quaternion.identity);
        Bullet bulletScript = bulletClone.GetComponent<Bullet>();
        bulletScript.SetOwner(gameObject);
        bulletScript.velocity = shootVelocity;
        CameraController.instance.SetTargetWithProjectileZoom(bulletScript.transform);
        hasShootThisTurn = true;
    }
    void DrawLine(Vector2 lanchVec)
    {
        List<Vector3> points = new List<Vector3>();
        float time = 0f;
        Vector3 startPoint = Vector3.zero;
        for (int i = 0; i < pointNum; i++)
        {
            float x = lanchVec.x * time;
            float y = lanchVec.y - Mathf.Abs(Physics2D.gravity.y) * time;
            points.Add(startPoint + new Vector3(x, y, 0f));
            time += pointTime;
        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
    void UIUpdate(float shootVector, float currentangle)
    {
        float powerCal = Mathf.Clamp01(shootVector/maxShootPower) * 100;
        powerText.text = powerCal.ToString();
        angleText.text = currentangle.ToString();
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