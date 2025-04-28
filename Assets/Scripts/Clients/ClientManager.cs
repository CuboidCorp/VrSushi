using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [SerializeField] private float clientInterval = 60f;
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform despawnPoint;
    [SerializeField] private Table[] tablePositions;

    [SerializeField] private KitchenItem[] plats;

    private List<Table> availableTables;

    private List<GameObject> clients = new List<GameObject>();
    private Coroutine spawnClientCoroutine;
    private const int maxClients = 6;
    private int nbClients = 0;


    private void Start()
    {
        spawnClientCoroutine = StartCoroutine(SpawnClientRoutine());
        availableTables = new List<Table>(tablePositions);
    }



    private IEnumerator SpawnClientRoutine()
    {
        while (true)
        {
            if (clients.Count < maxClients)
            {
                SpawnClient();
            }
            yield return new WaitForSeconds(clientInterval);
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
        Table table = client.target;
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

    }

    private void OnClientSatisfied(Client client)
    {

    }
}
