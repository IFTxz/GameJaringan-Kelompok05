using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ParticleSystem muzzleFlashVFX;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

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
            float randomX = Random.Range(-3f, 3f);
            float randomZ = Random.Range(-3f, 3f);
            transform.position = new Vector3(randomX, 0.5f, randomZ);
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

        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space))
        {
            // Kirim perintah dari Client ke Server
            ShootServerRpc();
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        // Membaca input keyboard (WASD / Panah)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Memindahkan posisi karakter saja
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
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
    [ServerRpc]
    private void ShootServerRpc()
    {
        if (bulletPrefab == null || firePoint == null) return;
        // Server melakukan Spawn peluru nyata di dunia permainan
        GameObject bulletInstance = Instantiate(bulletPrefab, firePoint.position,
       firePoint.rotation);

        // Daftarkan peluru ke dalam jaringan NGO
        NetworkObject bulletNetworkObject = bulletInstance.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn();
        // Minta semua Client untuk memutar efek visual & suara tembakan
        PlayShootEffectsClientRpc();
    }

    [ClientRpc]
    private void PlayShootEffectsClientRpc()
    {
        // Putar efek partikel di lokasi tembakan jika ada
        if (muzzleFlashVFX != null)
        {
            muzzleFlashVFX.Play();
        }
        // Opsional: Tambahkan efek suara tembakan (AudioSource.PlayClipAtPoint)
    }
    public void TakeDamage(int amount)
    {
        if (!IsServer) return;
        // Kurangi nilai NetworkVariable health
        health.Value = Mathf.Max(0, health.Value - amount);
    }
}