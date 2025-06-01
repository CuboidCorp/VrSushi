using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [Header("Settings")]
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

    private DayManager dayManager;

    private List<Table> availableTables;


    private readonly List<GameObject> clients = new();
    private List<float> clientSpawnTimes = new();

    private Coroutine spawnClientCoroutine;
    private const int MAX_CLIENTS = 6;
    private int nbClients = 1;


    private void Start()
    {
        dayManager = DayManager.Instance;
        ComputeClientSpawnTimes();
        spawnClientCoroutine = StartCoroutine(SpawnClientRoutine());
        availableTables = new List<Table>(tables);
        dayManager.OnDayEnd.AddListener(OnDayEnd);
    }

    private void OnDisable()
    {
        StopCoroutine(spawnClientCoroutine);
        spawnClientCoroutine = null;
    }

    private void OnDayEnd()
    {
        StopCoroutine(spawnClientCoroutine);
    }

    private void ComputeClientSpawnTimes()
    {
        clientSpawnTimes.Clear();

        int sampleCount = 1000;
        float totalCurveValue = 0f;
        float[] curveValues = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)(sampleCount - 1);
            float curveValue = dayManager.rushHourCurve.Evaluate(t);
            curveValues[i] = curveValue;
            totalCurveValue += curveValue;
        }

        float[] cumulativeDistribution = new float[sampleCount];
        float runningTotal = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            runningTotal += curveValues[i];
            cumulativeDistribution[i] = runningTotal / totalCurveValue;
        }

        for (int c = 0; c < GameData.Instance.nbClients; c++)
        {
            float target = (c + 0.5f) / GameData.Instance.nbClients;
            int index = System.Array.FindIndex(cumulativeDistribution, val => val >= target);
            float timePercent = index / (float)(sampleCount - 1);
            float spawnTime = timePercent * dayManager.dayDurationInSeconds;
            clientSpawnTimes.Add(spawnTime);
        }

        foreach (float spawnTime in clientSpawnTimes)
        {
            Debug.Log($"Client will spawn at: {spawnTime} seconds");
        }
        clientSpawnTimes.Sort();
    }


    private IEnumerator SpawnClientRoutine()
    {
        int nextClientIndex = 0;

        while (nextClientIndex < clientSpawnTimes.Count)
        {
            float currentTime = dayManager.CurrentTimePercent * dayManager.dayDurationInSeconds;

            if (currentTime >= clientSpawnTimes[nextClientIndex])
            {
                if (clients.Count < MAX_CLIENTS)
                {
                    SpawnClient();
                    nextClientIndex++;
                }
            }

            yield return null;
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


        GameObject clientPrefab = clientPrefabs[Random.Range(0, clientPrefabs.Length)];
        GameObject clientGo = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
        clientGo.name = $"Client_{nbClients}";
        Client client = clientGo.GetComponent<Client>();
        client.clientId = nbClients;
        table.SetClient(client);
        client.SetTargetTable(table);
        client.SetDespawnPoint(despawnPoint);
        client.StartClient();
        client.OnStartWaiting += OnClientStartWaiting;
        client.OnClientFinished += OnClientFinished;
        client.OnClientDespawn += OnClientDespawn;
        clients.Add(clientGo);
        nbClients++;
    }

    private void OnClientDespawn(Client client)
    {
        //Free the table
        Table table = client.targetTable;
        table.RemoveClient();
        availableTables.Add(table);
        Debug.Log($"Table {table.name} is now available.");

        //Remove the client from the list
        GameObject clientGo = client.gameObject;
        clients.Remove(clientGo);

        if (client.clientId == GameData.Instance.nbClients)
        {
            dayManager.DayEnd();
        }

        Destroy(clientGo);
    }

    private void OnClientStartWaiting(Client client)
    {
        //Le client affiche sur la table un plat random parmi la liste des plats
        KitchenItem plat = plats[Random.Range(0, plats.Length)];
        float waitTime = GameData.Instance.clientWaitTimeMultiplier * Random.Range(minClientWaitTime, maxClientWaitTime);
        client.targetTable.SetPlat(plat, waitTime);
    }

    private void OnClientFinished(Client client, ClientResult result, float satisfactionLevel)
    {
        client.targetTable.RemovePlat();

        KitchenItem dish = client.targetTable.expectedPlat;
        dayManager.dayStats.RecordClient(result, satisfactionLevel, dish);
    }

}
