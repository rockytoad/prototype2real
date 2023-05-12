using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class MoveCamera : NetworkBehaviour
{
    public Transform cameraPosition;

    private void Start()
    {
        if (!IsOwner) return;
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
