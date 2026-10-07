using Unity.Netcode;
using UnityEngine;

public class MoleSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject molePrefab;
    [SerializeField] private Transform[] spawnPoints; // Titik-titik lubang tikus
    [SerializeField] private float spawnInterval = 2f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InvokeRepeating(nameof(SpawnRandomMole), 1f, spawnInterval);
        }
    }

    private void SpawnRandomMole()
    {
        if (!IsServer || GameManager.Instance == null) return;

        // KUNCI STATE: Batalkan spawn tikus jika status bukan Gameplay 
        // (Misal saat WaitingForPlayers, Countdown, atau GameOver)
        if (GameManager.Instance.gameState.Value != GameManager.State.Gameplay)
        {
            return;
        }

        if (spawnPoints.Length == 0 || molePrefab == null) return;

        int index = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];

        GameObject moleInstance = Instantiate(molePrefab, spawnPoint.position, Quaternion.identity);
        moleInstance.GetComponent<NetworkObject>().Spawn();
    }
}