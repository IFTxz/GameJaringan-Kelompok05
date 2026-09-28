using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerController : NetworkBehaviour
{
    [Header("Network Sync Variables")]
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    public NetworkVariable<int> score = new NetworkVariable<int>(0);
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI scoreText;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Register event listener OnValueChanged
        health.OnValueChanged += OnHealthChanged;
        score.OnValueChanged += OnScoreChanged;
        // Inisialisasi tampilan UI awal saat spawn
        UpdateHealthUI(health.Value);
        UpdateScoreUI(score.Value);

        if (IsOwner)
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        // Selalu cabut listener saat objek di-despawn untuk mencegah memory leak
        health.OnValueChanged -= OnHealthChanged;
        score.OnValueChanged -= OnScoreChanged;
    }
    // Callback saat health berubah
    private void OnHealthChanged(int previousValue, int newValue)
    {
        UpdateHealthUI(newValue);
    }
    // Callback saat score berubah
    private void OnScoreChanged(int previousValue, int newValue)
    {
        UpdateScoreUI(newValue);
    }
    private void UpdateHealthUI(int currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
    private void UpdateScoreUI(int currentScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        // Simulasi input keyboard untuk pengujian
        // Tekan 'K' untuk mengurangi HP
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamageServerRpc(10);
        }
        // Tekan 'L' untuk menambah Score
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddScoreServerRpc(5);
        }
    }
    // Karena WritePermission = Server, pemicuan dari Client membutuhkan ServerRpc
    [ServerRpc]
    private void TakeDamageServerRpc(int damageAmount)
    {
        health.Value = Mathf.Max(0, health.Value - damageAmount);
    }
    [ServerRpc]
    private void AddScoreServerRpc(int scoreAmount)
    {
        score.Value += scoreAmount;
    }
}