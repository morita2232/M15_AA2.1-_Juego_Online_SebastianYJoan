using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.VersionControl;
using Unity.Services.Lobbies.Models;
using System;

public class CharacterMover : NetworkBehaviour
{
    public GameObject spawnObject;
    public float speed;
    float horizontal;
    float vertical;
    InputSystem_Actions inputs;
    private void Start()
    {
        if (!IsOwner)
        {
            this.enabled = false;
            //Destroy(GetComponent<Rigidbody>());
        }
        else
        {
            inputs = new InputSystem_Actions();
            inputs.Player.Enable();
        }
        RelayManager.players.Add(this);
    }
    void Update()
    {
        Vector2 dirInput = inputs.Player.Move.ReadValue<Vector2>();
        Vector3 dir = new Vector3(dirInput.x * Time.deltaTime * speed, 0, dirInput.y * Time.deltaTime * speed);
        transform.position += dir;

        if (inputs.Player.Jump.WasPressedThisFrame())
        {
            SpawnObjectServerRPC();
        }
        if (inputs.Player.Attack.WasPressedThisFrame())
        {
            string message = "Test";
            Debug.Log("You sent: " + message);
            SendMessageServerRpc(message, OwnerClientId);
        }
    }

    [ServerRpc]
    void SpawnObjectServerRPC()
    {
        GameObject go = Instantiate(spawnObject, transform.position, transform.rotation);
        go.GetComponent<NetworkObject>().Spawn();
    }
    [ServerRpc]
    void SendMessageServerRpc(string message, ulong sender, long target = -1)
    {
        SendMessageClientRpc(message, sender, target);
    }
    [ClientRpc]
    void SendMessageClientRpc(string message, ulong sender, long target = -1)
    {
        if (target < 0)
        {
            Message(sender, message);
        }
        else
        {
            foreach (CharacterMover player in RelayManager.players)
            {
                if (player.OwnerClientId == (ulong)target)
                    player.Message(sender, "(private) " + message);
            }
        }
    }

    public void Message(ulong from, string message)
    {
        Debug.Log("Player " + from + ": " + message);
    }
}
