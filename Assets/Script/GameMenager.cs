using System.Collections.Generic;
using UnityEngine;

public class GameMenager : MonoBehaviour
{
    public static GameMenager instance;
    public List<PlayerController> playerControllers;
    private int currentPlayerTurn = 1;
    private bool waitForAttack;
    public bool GetWaitForAttack { get { return waitForAttack; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    private void Start()
    {
        StartTurn(0);
    }

    public void StartTurn(int playerIndex)
    {
        currentPlayerTurn = playerIndex;
        waitForAttack = false;
        if (CameraController.instance != null)
        {
            CameraController.instance.SetTargetWithDefaultZoom(playerControllers[currentPlayerTurn].transform);
        }
        for (int i = 0; i < playerControllers.Count; i++)
        {
            if (playerControllers[i] != null)
            {
                playerControllers[i].SetMyTurn(i == currentPlayerTurn);
            }
        }
    }
    public void NotifyEndTurn()
    {
        if(waitForAttack)
        {
            return;
        }
        waitForAttack = true;
        if (playerControllers[currentPlayerTurn] != null)
        {
            playerControllers[currentPlayerTurn].SetMyTurn(false);
        }
        Invoke("SwichToNextTurn", 0.5f);
    }
    private void SwichToNextTurn()
    {
        int nextPlayerTurn = (currentPlayerTurn + 1) % playerControllers.Count;
        // รอ
        StartTurn(nextPlayerTurn);
    }
    public bool IsPlayerTurn(PlayerController player)
    {
        return playerControllers[currentPlayerTurn] == player && !waitForAttack;
    }
}
