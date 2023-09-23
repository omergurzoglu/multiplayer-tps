
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button hostButton;
    
    private void Awake()
    {
        serverButton.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartServer();
            CloseButtons();
        }));
        clientButton.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartClient();
            CloseButtons();
        }));
        hostButton.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartHost();
            CloseButtons();
        }));
        
    }

    private void CloseButtons()
    {
        serverButton.gameObject.SetActive(false); 
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }
}
