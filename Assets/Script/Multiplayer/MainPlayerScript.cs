using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class MainPlayerScript : NetworkBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    Rigidbody rb;

    public TMP_Text namePrefab;
    private TMP_Text nameLabel;
    public NetworkVariable<int> postX = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<NetworkString> playerNameA = new NetworkVariable<NetworkString>(
        new NetworkString { info = "Player A" },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<NetworkString> playerNameB = new NetworkVariable<NetworkString>(
    new NetworkString { info = "Player B" },
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private LoginManagerScript loginManager;

    public struct NetworkString : INetworkSerializable
    {
        public FixedString32Bytes info;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref info);
        }
        public override string ToString()
        {
            return info.ToString();
        }
        public static implicit operator NetworkString(string v) =>
            new NetworkString() { info = new FixedString32Bytes(v)};
    }

    public override void OnNetworkSpawn()
    {
        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(namePrefab, Vector3.zero, Quaternion.identity) as TMP_Text;
        nameLabel.transform.SetParent(canvas.transform);

        postX.OnValueChanged += (int previousData, int newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : post x = " + postX.Value);
        };

        playerNameA.OnValueChanged += (NetworkString previousData, NetworkString newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : new data = " + newValue.info);
        };

        playerNameB.OnValueChanged += (NetworkString previousData, NetworkString newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : new data = " + newValue.info);
        };
        //if (IsServer)
        //{
        //    playerNameA.Value = new NetworkString() { info = new FixedString32Bytes("Player1") };
        //    string name = "Player2";
        //    playerNameB.Value = name;
        //}
        if (IsOwner)
        {
            loginManager = GameObject.FindObjectOfType<LoginManagerScript>();
            matchMakingScript = GameObject.FindObjectOfType<MatchMakingScript>();
            if (loginManager != null)
            {
                string name = loginManager.userNameInput.text;
                if (IsOwnedByServer) { playerNameA.Value = name; }
                else { playerNameB.Value = name; }
            }
            else if (matchMakingScript != null)
            {
                string name = matchMakingScript.playNameInput.text;
                if (IsOwnedByServer) { playerNameA.Value = name; }
                else { playerNameB.Value = name; }
            }
        }
    }
    private MatchMakingScript matchMakingScript;

    public override void OnDestroy()
    {
        if (nameLabel != null)
            Destroy(nameLabel.gameObject);

        base.OnDestroy();
    }

    private void Update()
    {
        Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(transform.position +
            new Vector3(0, 2.5f, 0));
        nameLabel.text = gameObject.name;
        nameLabel.transform.position = nameLabelPos;

        if (IsOwner)
        {
            postX.Value = (int)System.Math.Ceiling(transform.position.x);
            if (Input.GetKeyDown(KeyCode.K))
            {
                TestServerRpc("hello", new ServerRpcParams());
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                ClientRpcSendParams clientRpcSendParams =
                    new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } };
                ClientRpcParams clientRpcParams = new ClientRpcParams { Send = clientRpcSendParams };
                TestClientRpc("hello from server", clientRpcParams);
            }
        }

        updatePlayerInfo();
    }

    [ClientRpc]
    private void TestClientRpc(string msg, ClientRpcParams clientRpcParams)
    {
        Debug.Log("Message = " + msg);
    }

    [ServerRpc]
    private void TestServerRpc(string msg, ServerRpcParams serverRpcParams)
    {
        Debug.Log("Message = " + msg + "; from id = " + serverRpcParams.Receive.SenderClientId);
    }

    private void updatePlayerInfo()
    {
        if (IsOwnedByServer)
        {
            nameLabel.text = playerNameA.Value.ToString();
        }
        else
        {
            nameLabel.text = playerNameB.Value.ToString();
        }
    }

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        //if (!IsOwner) return;
        //if (IsOwner)
        //{
        //    float translation = Input.GetAxis("Vertical") * speed;
        //    float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
        //    translation *= Time.deltaTime;
        //    Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
        //    rb.MovePosition(rb.position + this.transform.forward * translation);
        //    rb.MoveRotation(rb.rotation * turn);
        //}
    }

    private void OnEnable()
    {
        if (nameLabel != null)
            nameLabel.enabled = true;
    }

    private void OnDisable()
    {
        if (nameLabel != null)
            nameLabel.enabled = false;
    }
}
