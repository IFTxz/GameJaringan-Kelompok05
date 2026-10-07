using Unity.Netcode;
using UnityEngine;

public class Mole : NetworkBehaviour
{
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float lifetime = 2f; // Tikus hilang otomatis jika tidak dipukul

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            Invoke(nameof(DespawnMole), lifetime);
        }
    }

    public void GetHit(Palu hitter)
    {
        if (!IsServer || !IsSpawned) return;

        CancelInvoke(nameof(DespawnMole));
        hitter.AddScore(scoreValue);
        DespawnMole();
    }

    // Dipanggil GameManager saat game berakhir
    public void ForceDespawn()
    {
        CancelInvoke(nameof(DespawnMole));
        DespawnMole();
    }

    private void DespawnMole()
    {
        if (IsServer && IsSpawned)
        {
            NetworkObject.Despawn(true); // otomatis destroy
        }
    }
}