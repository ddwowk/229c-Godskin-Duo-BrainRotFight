using System.Collections.Generic;
using UnityEngine;

public class GameMenager : MonoBehaviour
{
    public static GameMenager instance;
    public List<PlayerController> playerControllers;
    private int currentPlayerTurn = 1;
    private bool waitForAttack;
    public bool GetWaitForAttack { get { return waitForAttack; } }

    [Header("Audio")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip winnerMusic;

    [Header("UI")]
    [SerializeField] private GameObject player1WinUI;
    [SerializeField] private GameObject player2WinUI;

    private bool gameHasEnded = false;

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

        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null) musicAudioSource = gameObject.AddComponent<AudioSource>();
            musicAudioSource.playOnAwake = false;
            musicAudioSource.loop = false;
        }
    }

    private void Start()
    {
        StartTurn(0);
        if (player1WinUI != null) player1WinUI.SetActive(false);
        if (player2WinUI != null) player2WinUI.SetActive(false);
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

    public void HandlePlayerDeath(string playerName, GameObject winCanvas, GameObject player1RefFromDyingPlayer, GameObject player2RefFromDyingPlayer, Vector3 deathPosition)
{
    if (gameHasEnded)
    {
        return;
    }

    int alivePlayers = 0;
    PlayerController winningPlayer = null;
    GameObject dyingPlayer = GameObject.Find(playerName); 
    if (dyingPlayer == null)
    {
        return;
    }

    foreach (PlayerController pc in playerControllers)
    {
        if (pc == null) { continue; }

        bool isSelfActive = pc.gameObject.activeSelf;
        bool isNotDyingPlayer = pc.gameObject != dyingPlayer;

        if (isSelfActive && isNotDyingPlayer)
        {
            alivePlayers++;
            winningPlayer = pc; 
        }
    }

    if (alivePlayers == 1 && winningPlayer != null)
    {
        gameHasEnded = true;

        if (musicAudioSource != null && winnerMusic != null)
        {
            musicAudioSource.PlayOneShot(winnerMusic);
        }

        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
        }
        if (winningPlayer.CompareTag("Player1")) 
        {
            if (player1WinUI != null)
            {
                player1WinUI.SetActive(true);
            }
            if (player2WinUI != null) player2WinUI.SetActive(false);
        }
        else if (winningPlayer.CompareTag("Player2")) 
        {
            if (player2WinUI != null)
            {
                player2WinUI.SetActive(true);
            }

            if (player1WinUI != null) player1WinUI.SetActive(false);
        }
    }
}
    public void NotifyEndTurn()
    {
        if (waitForAttack || gameHasEnded)
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
        if (gameHasEnded) return;

        int nextPlayerTurn = (currentPlayerTurn + 1) % playerControllers.Count;

        int attempts = 0;
        while (playerControllers[nextPlayerTurn] == null || !playerControllers[nextPlayerTurn].gameObject.activeSelf)
        {
            nextPlayerTurn = (nextPlayerTurn + 1) % playerControllers.Count;
            attempts++;
            if (attempts > playerControllers.Count)
            {
                return;
            }
        }

        StartTurn(nextPlayerTurn);
    }

    public bool IsPlayerTurn(PlayerController player)
    {
        return playerControllers[currentPlayerTurn] == player && !waitForAttack;
    }
}
