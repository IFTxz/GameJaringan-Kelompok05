using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet;
using FishNet.Managing;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Awake()
    {
        hostButton.onClick.AddListener(OnHostButtonClicked);
        clientButton.onClick.AddListener(OnClientButtonClicked);
    }

    private void OnHostButtonClicked()
    {
        // Menjalankan Server dan Client lokal sekaligus (Host) di FishNet
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
        UpdateUIStatus("Status: Connected as HOST");
    }

    private void OnClientButtonClicked()
    {
        // Menjalankan Client saja di FishNet
        InstanceFinder.ClientManager.StartConnection();
        UpdateUIStatus("Status: Connecting as CLIENT...");
    }

    private void UpdateUIStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }
}