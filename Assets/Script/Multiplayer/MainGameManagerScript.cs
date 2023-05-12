using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainGameManagerScript : MonoBehaviour
{
    public void OnServerButtonClicked()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void OnClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
    }
}
