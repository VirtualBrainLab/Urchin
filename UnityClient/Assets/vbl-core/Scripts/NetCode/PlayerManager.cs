using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Handles the Player object in MLAPI and deals with switching between player objects.
 * 
 * In practice, the player object is the Probe (for now), and when you click a different probe you need to switch
 * which object is the active player object.
 */
public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private Transform brainModelTransform;
    [SerializeField] private GameObject defaultPlayerPrefab;

    private List<GameObject> playerObjects;

    // Start is called before the first frame update
    void Start()
    {
        playerObjects = new List<GameObject>();
    }

    public void ClientStart()
    {
        // Does nothing right now
    }

    public void SpawnPlayerObject()
    {
        if (!IsClient) return;

        SpawnPlayerObjectServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerObjectServerRpc(ulong clientId)
    {
        if (!IsServer) return;

        // Create the player object, assign ownership, and make invisible to everybody except the requesting player
        GameObject temp = Instantiate(defaultPlayerPrefab, brainModelTransform);
        DeactivatePlayerObjects();
        temp.GetComponent<Electrode>().SetActive(true);
        NetworkObject tempNO = temp.GetComponent<NetworkObject>();
        tempNO.SpawnAsPlayerObject(clientId);
        // Deal with visibility
        foreach (ulong keyClientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            // don't hide object from the client that requested it
            if (clientId != keyClientId)
            {
                tempNO.NetworkHide(keyClientId);
            }
        }

        playerObjects.Add(temp);
    }

    public void DeactivatePlayerObjects()
    {
        foreach (GameObject go in playerObjects)
        {
            go.GetComponent<Electrode>().SetActive(false);
            Debug.Log("Setting each electrode to be inactive");
        }
    }
}
