using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            GetComponent<Renderer>().material.color = Color.green;

            // Atur posisi spawn acak untuk player yang baru masuk
            float randomX = Random.Range(-3f, 3f);
            float randomZ = Random.Range(-3f, 3f);
            transform.position = new Vector3(randomX, 0.5f, randomZ);
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
    }


    private void Update()
    {
        // KUNCI UTAMA MULTIPLAYER:
        // Jalankan input & pergerakan HANYA jika objek ini milik player lokal
        if (!IsOwner) return;

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
}
