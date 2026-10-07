using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State
    {
        WaitingForPlayers,
        Countdown,
        Gameplay,
        GameOver
    }

    // Status game disinkronkan ke semua Client, hanya Server yang boleh menulis
    public NetworkVariable<State> gameState = new NetworkVariable<State>(
        State.WaitingForPlayers,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Angka hitung mundur untuk ditampilkan di UI (3, 2, 1)
    public NetworkVariable<int> countdownDisplay = new NetworkVariable<int>(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Match Settings")]
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private int targetScore = 50;
    [SerializeField] private int requiredPlayers = 2;

    public int TargetScore => targetScore;

    public event Action<State> OnGameStateChanged;
    public event Action<ulong> OnGameOver; // Mengirimkan ClientId pemenang

    private float countdownTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnDestroy()
    {
        if (Instance == this) Instance = null;
        base.OnDestroy();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gameState.OnValueChanged += HandleStateChanged;

        // Sinkronkan UI dengan state saat ini (penting untuk client yang baru masuk)
        OnGameStateChanged?.Invoke(gameState.Value);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Host sendiri mungkin sudah terhubung sebelum callback dipasang
            TryStartCountdown();
        }
    }

    public override void OnNetworkDespawn()
    {
        gameState.OnValueChanged -= HandleStateChanged;

        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        base.OnNetworkDespawn();
    }

    private void HandleStateChanged(State oldState, State newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }

    // ---------------- SERVER LOGIC ----------------

    private void OnClientConnected(ulong clientId)
    {
        TryStartCountdown();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer || NetworkManager.Singleton == null) return;

        int count = NetworkManager.Singleton.ConnectedClientsList.Count;
        if (count >= requiredPlayers) return;

        if (gameState.Value == State.Countdown)
        {
            // Lawan keluar saat hitung mundur: kembali menunggu
            SetState(State.WaitingForPlayers);
        }
        else if (gameState.Value == State.Gameplay && count == 1)
        {
            // Lawan keluar saat bermain: pemain yang tersisa menang
            EndGame(NetworkManager.Singleton.ConnectedClientsList[0].ClientId);
        }
    }

    private void TryStartCountdown()
    {
        if (!IsServer) return;
        if (gameState.Value != State.WaitingForPlayers) return;

        if (NetworkManager.Singleton.ConnectedClientsList.Count >= requiredPlayers)
        {
            SetState(State.Countdown);
        }
    }

    private void SetState(State newState)
    {
        if (!IsServer) return;

        if (newState == State.Countdown)
        {
            countdownTimer = countdownDuration; // reset setiap kali masuk Countdown
            countdownDisplay.Value = Mathf.CeilToInt(countdownDuration);
        }

        gameState.Value = newState;
    }

    private void Update()
    {
        if (!IsServer || !IsSpawned) return;

        if (gameState.Value == State.Countdown)
        {
            countdownTimer -= Time.deltaTime;
            countdownDisplay.Value = Mathf.Max(0, Mathf.CeilToInt(countdownTimer));

            if (countdownTimer <= 0f)
            {
                SetState(State.Gameplay);
            }
        }
    }

    // Dipanggil Server (oleh Palu.AddScore) setiap skor pemain berubah
    public void CheckWinConditionOnScore(ulong clientId, int currentScore)
    {
        if (!IsServer || gameState.Value != State.Gameplay) return;

        if (currentScore >= targetScore)
        {
            EndGame(clientId);
        }
    }

    private void EndGame(ulong winnerClientId)
    {
        if (!IsServer || gameState.Value == State.GameOver) return;

        SetState(State.GameOver);
        DespawnAllMoles();
        NotifyGameOverClientRpc(winnerClientId);
    }

    private void DespawnAllMoles()
    {
        foreach (Mole mole in FindObjectsByType<Mole>(FindObjectsSortMode.None))
        {
            mole.ForceDespawn();
        }
    }

    [ClientRpc]
    private void NotifyGameOverClientRpc(ulong winnerClientId)
    {
        OnGameOver?.Invoke(winnerClientId);
    }
}
