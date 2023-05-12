using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerSpawnScript : NetworkBehaviour
{
    MainPlayerScript mainPlayer;
    public Behaviour[] scripts;
    private Renderer[] renderers;
    void Start()
    {
        //mainPlayer = GetComponent<MainPlayerScript>();
        renderers = GetComponentsInChildren<Renderer>();
    }
    private void SetPlayerState(bool state)
    {
        foreach (var script in scripts) { script.enabled = state; }
        foreach (var renderer in renderers) { renderer.enabled = state; }
    }
    private Vector3 GetRandPos()
    {
        Vector3 randPos = new Vector3(Random.Range(-3, 3), 1, Random.Range(-3, 3));
        return randPos;
    }
    public void Respawn()
    {
        RespawnServerRpc();
    }
    [ServerRpc]
    private void RespawnServerRpc()
    {
        RespawnClientRpc(GetRandPos());
    }
    [ClientRpc]
    private void RespawnClientRpc(Vector3 spawnPos)
    {
        //mainPlayer.enabled = false; transform.position = spawnPos; mainPlayer.enabled = true;
        StartCoroutine(RespawnCoroutine(spawnPos));
    }

    IEnumerator RespawnCoroutine(Vector3 spawnPos)
    {
        SetPlayerState(false);
        transform.position = spawnPos;
        yield return new WaitForSeconds(3);
        SetPlayerState(true);
    }
}
