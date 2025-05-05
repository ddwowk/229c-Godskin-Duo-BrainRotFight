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
         Debug.LogError($"HandlePlayerDeath: Could not find player GameObject named '{playerName}'.");
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

     Debug.Log($"HandlePlayerDeath: Checked players. Alive count (excluding {playerName}): {alivePlayers}. Potential winner: {(winningPlayer != null ? winningPlayer.gameObject.name : "None")}");

    if (alivePlayers == 1 && winningPlayer != null)
    {
        gameHasEnded = true;
         Debug.Log($"Game Over! Winner is {winningPlayer.gameObject.name}.");

        if (musicAudioSource != null && winnerMusic != null)
        {
            musicAudioSource.PlayOneShot(winnerMusic);
             Debug.Log("Played winner music.");
        }

        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
             Debug.Log($"Activated main win canvas: {winCanvas.name}");
        }
        else
        {
             Debug.LogWarning($"Main win canvas reference passed from {playerName} was null.");
        }
        if (winningPlayer.CompareTag("Player1")) 
        {
            if (player1WinUI != null)
            {
                player1WinUI.SetActive(true);
                 Debug.Log("Activated Player 1 Win UI.");
            }
             else { Debug.LogWarning("player1WinUI reference is null in GameManager.");}


            if (player2WinUI != null) player2WinUI.SetActive(false);
        }
        else if (winningPlayer.CompareTag("Player2")) 
        {
            if (player2WinUI != null)
            {
                player2WinUI.SetActive(true);
                 Debug.Log("Activated Player 2 Win UI.");
            }
            else { Debug.LogWarning("player2WinUI reference is null in GameManager.");}

            if (player1WinUI != null) player1WinUI.SetActive(false);
        }
        else
        {
             Debug.LogWarning($"Winning player '{winningPlayer.gameObject.name}' does not have tag 'Player1' or 'Player2'. Cannot activate specific win UI.");
        }
    }
     else
     {
          Debug.Log($"Win condition not met. Alive players: {alivePlayers}. Needs to be 1.");
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
            Debug.Log($"Player {nextPlayerTurn} is inactive, skipping turn.");
            nextPlayerTurn = (nextPlayerTurn + 1) % playerControllers.Count;
            attempts++;
            if (attempts > playerControllers.Count)
            {
                Debug.LogError("All players seem inactive. Cannot switch turn.");
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
