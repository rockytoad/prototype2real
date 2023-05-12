using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;
using TMPro;
using Unity.Netcode.Transports.UTP;

public class LoginManagerScript : MonoBehaviour
{
    public TMP_InputField userNameInput;
    public GameObject loginPanel;
    public GameObject leaveButton;
    public GameObject scorePanel;
    public string ipAddress = "127.0.0.1";
    public TMP_InputField ipInputField;
    UnityTransport transport;
    public TMP_InputField joinCodeInput;
    public string joinCode;

    public async void Client()
    {
        //setIpAddress();
        joinCode = joinCodeInput.GetComponent<TMP_InputField>().text;
        if (RelayManagerScript.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCode))
        {
            await RelayManagerScript.Instance.JoinRelay(joinCode);
        }
        string userName = userNameInput.GetComponent<TMP_InputField>().text;
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            System.Text.Encoding.ASCII.GetBytes(userName);
        NetworkManager.Singleton.StartClient();
    }

    private void setIpAddress()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        ipAddress = ipInputField.GetComponent<TMP_InputField>().text;
        transport.ConnectionData.Address = ipAddress;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        SetUIVisible(false);
    }

    private void SetUIVisible(bool isUserLogin)
    {
        if (isUserLogin)
        {
            loginPanel.SetActive(false);
            leaveButton.SetActive(true);
            scorePanel.SetActive(true);
        }
        else
        {
            loginPanel.SetActive(true);
            leaveButton.SetActive(false);
            scorePanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        // if host is disconntected, client will leave the game
        if (clientId == 0)
        {
            Leave();
        }
    }

    public void Leave() 
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SetUIVisible(false);
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SetUIVisible(true);
        } 
    }

    private void HandleServerStarted()
    {
        //throw new System.NotImplementedException();
    }

    public async void Host()
    {
        //setIpAddress();
        if (RelayManagerScript.Instance.IsRelayEnabled)
        {
            await RelayManagerScript.Instance.CreateRelay();
        }
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }

    private bool approveConnection(string clientData, string serverData)
    {
        bool isApprove = System.String.Equals(clientData.Trim(), serverData.Trim()) ? false: true;
        return isApprove;
    }
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;
        int byteLength = connectionData.Length;
        bool isApprove = false;
        if (byteLength > 0)
        {
            string clientData = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength);
            string hostData = userNameInput.GetComponent<TMP_InputField>().text;
            isApprove = approveConnection(clientData, hostData);
        }

        // Your approval logic determines the following values
        response.Approved = isApprove;
        response.CreatePlayerObject = true;

        // The prefab hash value of the NetworkPrefab, if null the default NetworkManager player prefab is used
        response.PlayerPrefabHash = null;

        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = Vector3.zero;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;

        setSpawnLocation(clientId, response);

        // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        response.Reason = "Some reason for not approving the client";

        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
    }

    private void setSpawnLocation(ulong clientId, NetworkManager.ConnectionApprovalResponse response)
    {
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            spawnPos = new Vector3(-2, 0, 0); spawnRot = Quaternion.Euler(0, 135, 0);
        }
        else
        {
            switch (NetworkManager.Singleton.ConnectedClients.Count)
            {
                case 1:
                    spawnPos = new Vector3(0, 0, 0); spawnRot = Quaternion.Euler(0, 100, 0);
                    break;
                case 2:
                    spawnPos = new Vector3(2, 0, 0); spawnRot = Quaternion.Euler(0, 80, 0);
                    break;
            }
        }
        response.Position = spawnPos; response.Rotation = spawnRot;
    }

    //private bool isApproveConnection = false;

    //[Command("change-approve")]
    //public bool changeApproveConnectin()
    //{
    //    isApproveConnection = !isApproveConnection;
    //    Debug.Log("isApproveConnection = " + isApproveConnection);
    //    return isApproveConnection;
    //}
}
