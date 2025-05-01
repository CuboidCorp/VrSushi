using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("Min Time between each client")] private float minClientInterval = 40f;
    [SerializeField, Tooltip("Max Time between each client")] private float maxClientInterval = 80f;
    [SerializeField, Tooltip("Min time the client waits before leaving")] private float minClientWaitTime = 20f;
    [SerializeField, Tooltip("Maximum time the client waits before leaving")] private float maxClientWaitTime = 40f;


    [Header("References")]
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform despawnPoint;
    [SerializeField] private Table[] tables;
    [SerializeField] private KitchenItem[] plats;

    private List<Table> availableTables;

    private List<GameObject> clients = new();
    private Coroutine spawnClientCoroutine;
    private const int MAX_CLIENTS = 6;
    private int nbClients = 0;


    private void Start()
    {
        availableTables = new List<Table>(tables);
        spawnClientCoroutine = StartCoroutine(SpawnClientRoutine());
    }

    private void OnDisable()
    {
        spawnClientCoroutine = null;
    }

    private IEnumerator SpawnClientRoutine()
    {
        while (true)
        {
            float randomInterval = Random.Range(minClientInterval, maxClientInterval);
            yield return new WaitForSeconds(randomInterval);
            if (clients.Count < MAX_CLIENTS)
            {
                SpawnClient();
            }
        }
    }

    private void SpawnClient()
    {
        if (availableTables.Count == 0)
        {
            Debug.Log("No available tables to spawn a client.");
            return;
        }

        Table table = availableTables[Random.Range(0, availableTables.Count)];
        availableTables.Remove(table);

        nbClients++;
        GameObject clientGo = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
        clientGo.name = $"Client_{nbClients}";
        Client client = clientGo.GetComponent<Client>();
        table.SetClient(client);
        client.SetTargetTable(table);
        client.SetDespawnPoint(despawnPoint);
        client.StartClient();
        client.OnStartWaiting += OnClientStartWaiting;
        client.OnClientSatisfaction += OnClientSatisfied;
        client.OnClientDespawn += OnClientDespawn;
        clients.Add(clientGo);
    }

    private void OnClientDespawn(Client client)
    {
        //Free the table
        Table table = client.targetTable;
        table.RemoveClient();

        //Remove the client from the list
        GameObject clientGo = client.gameObject;
        clients.Remove(clientGo);
        Destroy(clientGo);

    }

    private void OnClientStartWaiting(Client client)
    {
        //Le client affiche sur la table un plat random parmi la liste des plats
        KitchenItem plat = plats[Random.Range(0, plats.Length)];
        client.targetTable.SetPlat(plat, Random.Range(minClientWaitTime, maxClientWaitTime));
    }

    private void OnClientSatisfied(Client client, float satisfactionLevel)
    {

    }
}
