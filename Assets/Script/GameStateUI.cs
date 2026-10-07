using UnityEngine;
using TMPro;

// Menampilkan "Waiting for Opponent..." dan hitung mundur 3, 2, 1
public class GameStateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waitingText;
    [SerializeField] private TextMeshProUGUI countdownText;
    private void Start()
    {
        // Sembunyikan teks sejak awal sebelum game dimulai (sebelum connect)
        if (waitingText != null) waitingText.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || !gm.IsSpawned) return;

        GameManager.State state = gm.gameState.Value;

        waitingText.gameObject.SetActive(state == GameManager.State.WaitingForPlayers);
        countdownText.gameObject.SetActive(state == GameManager.State.Countdown);

        if (state == GameManager.State.Countdown)
        {
            countdownText.text = gm.countdownDisplay.Value.ToString();
        }
    }
}
