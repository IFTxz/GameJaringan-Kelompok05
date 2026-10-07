using Unity.Netcode;
using UnityEngine;
using TMPro;

public class Palu : NetworkBehaviour
{
    [Header("Network Sync Variables")]
    public NetworkVariable<int> score = new NetworkVariable<int>(0);

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Whack Settings")]
    [SerializeField] private LayerMask moleLayer;
    [SerializeField] private ParticleSystem hitVFX;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hostColor = Color.red;
    [SerializeField] private Color clientColor = Color.blue;
    [SerializeField] private Canvas canvas;

    [Header("Keyboard Control (pemain non-Host)")]
    [SerializeField] private float keyboardSpeed = 8f;

    // Host (ClientId 0) = mouse, pemain lain = keyboard
    private bool UseMouse => OwnerClientId == NetworkManager.ServerClientId;

    // Input hanya aktif saat fase Gameplay
    private bool IsGameplay =>
        GameManager.Instance != null &&
        GameManager.Instance.IsSpawned &&
        GameManager.Instance.gameState.Value == GameManager.State.Gameplay;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleStateChanged;
            // Set kondisi awal
            HandleStateChanged(GameManager.Instance.gameState.Value);
        }

        ApplyColor();

        score.OnValueChanged += OnScoreChanged;
        UpdateScoreUI(score.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleStateChanged;
        }
        base.OnNetworkDespawn();
        score.OnValueChanged -= OnScoreChanged;
    }

    private void HandleStateChanged(GameManager.State newState)
    {
        // Tampilkan palu HANYA saat statusnya Gameplay
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = (newState == GameManager.State.Gameplay);
        }
        if (canvas != null)
        {
            canvas.enabled = (newState == GameManager.State.Gameplay);
        }
    }
    private void ApplyColor()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = UseMouse ? hostColor : clientColor;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!IsGameplay) return; // Terkunci saat Waiting, Countdown, dan GameOver

        bool whack;

        if (UseMouse)
        {
            HandleMouseMovement();
            whack = Input.GetMouseButtonDown(0);
        }
        else
        {
            HandleKeyboardMovement();
            whack = Input.GetKeyDown(KeyCode.Space);
        }

        if (whack)
        {
            WhackServerRpc(transform.position);
        }
    }

    private void HandleMouseMovement()
    {
        Camera cam = Camera.main;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(mousePos);
        world.z = 0f;
        transform.position = world;
    }

    private void HandleKeyboardMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(x, y, 0f).normalized;
        Vector3 newPos = transform.position + dir * keyboardSpeed * Time.deltaTime;

        Camera cam = Camera.main;
        float depth = Mathf.Abs(cam.transform.position.z);
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, depth));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, depth));
        newPos.x = Mathf.Clamp(newPos.x, min.x, max.x);
        newPos.y = Mathf.Clamp(newPos.y, min.y, max.y);
        newPos.z = 0f;

        transform.position = newPos;
    }

    [ServerRpc]
    private void WhackServerRpc(Vector3 whackPosition)
    {
        // Server tetap memvalidasi state, jangan percaya client
        if (!IsGameplay) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(whackPosition, 0.65f, moleLayer);

        foreach (Collider2D col in hits)
        {
            Mole mole = col.GetComponentInParent<Mole>();
            if (mole != null)
            {
                mole.GetHit(this);
                PlayHitEffectClientRpc(whackPosition);
                return;
            }
        }
    }

    [ClientRpc]
    private void PlayHitEffectClientRpc(Vector3 pos)
    {
        if (hitVFX != null)
        {
            hitVFX.transform.position = pos;
            hitVFX.Play();
        }
    }

    public void AddScore(int amount)
    {
        if (!IsServer) return;

        score.Value += amount;

        // Server bertindak sebagai juri: cek apakah skor mencapai target
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckWinConditionOnScore(OwnerClientId, score.Value);
        }
    }

    private void OnScoreChanged(int prev, int current) => UpdateScoreUI(current);

    private void UpdateScoreUI(int val)
    {
        if (scoreText != null) scoreText.text = "Score: " + val;
    }
}