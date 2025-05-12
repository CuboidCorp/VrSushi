using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("Spawn rate of clients per minute")] private float baseSpawnRatePerMinute = 1f;
    [SerializeField, Tooltip("Min time the client waits before leaving")] private float minClientWaitTime = 90f;
    [SerializeField, Tooltip("Maximum time the client waits before leaving")] private float maxClientWaitTime = 120f;
    [SerializeField, Tooltip("How many seconds ahead to look when evaluating rush hour intensity (helps shift rush earlier)")]
    private float rushHourLookAheadSeconds = 60f;

    [Header("References")]
    [SerializeField] private GameObject[] clientPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform despawnPoint;
    [SerializeField] private Table[] tables;
    [SerializeField] private KitchenItem[] plats;

    [SerializeField] private DayManager dayManager;




    private List<Table> availableTables;


    private readonly List<GameObject> clients = new();
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
        StopCoroutine(spawnClientCoroutine);
        spawnClientCoroutine = null;
    }

    private IEnumerator SpawnClientRoutine()
    {
        yield return new WaitForSeconds(20f); //Delai initial avant de spawn le premier client

        while (true)
        {
            float currentPercent = dayManager.CurrentTimePercent;
            float futureSeconds = currentPercent * dayManager.dayDurationInSeconds + rushHourLookAheadSeconds;
            float lookAheadPercent = Mathf.Clamp01(futureSeconds / dayManager.dayDurationInSeconds);

            float rushMultiplier = dayManager.rushHourCurve.Evaluate(lookAheadPercent);
            float spawnRate = baseSpawnRatePerMinute * rushMultiplier;

            float timeBetweenSpawns = 60f / spawnRate; // in seconds
            timeBetweenSpawns = Mathf.Clamp(timeBetweenSpawns, 5f, 120f); // Safety clamp

            if (clients.Count < MAX_CLIENTS)
            {
                SpawnClient();
            }

            yield return new WaitForSeconds(timeBetweenSpawns);
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
        GameObject clientPrefab = clientPrefabs[Random.Range(0, clientPrefabs.Length)];
        GameObject clientGo = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
        clientGo.name = $"Client_{nbClients}";
        Client client = clientGo.GetComponent<Client>();
        table.SetClient(client);
        client.SetTargetTable(table);
        client.SetDespawnPoint(despawnPoint);
        client.StartClient();
        client.OnStartWaiting += OnClientStartWaiting;
        client.OnClientFinished += OnClientFinished;
        client.OnClientDespawn += OnClientDespawn;
        clients.Add(clientGo);
    }

    private void OnClientDespawn(Client client)
    {
        //Free the table
        Table table = client.targetTable;
        table.RemoveClient();
        availableTables.Add(table);

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

    private void OnClientFinished(Client client, ClientResult result, float satisfactionLevel)
    {
        KitchenItem dish = client.targetTable.expectedPlat;
        dayManager.dayStats.RecordClient(result, satisfactionLevel, dish);
    }

}
