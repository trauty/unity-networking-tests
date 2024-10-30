using Unity.Netcode;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private GameObject sceneCamera;

    public void StartHost()
    {
        networkManager.StartHost();
        Destroy(sceneCamera);
        Destroy(gameObject);
    }

    public void StartClient()
    {
        networkManager.StartClient();
        Destroy(sceneCamera);
        Destroy(gameObject);
    }
}
