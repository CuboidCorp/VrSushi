using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("Min time the client waits before leaving")] private float minClientWaitTime = 90f;
    [SerializeField, Tooltip("Maximum time the client waits before leaving")] private float maxClientWaitTime = 120f;

    [Header("References")]
    [SerializeField] private GameObject[] clientPrefabs;
    [SerializeField] private GameObject[] premiumClientPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform despawnPoint;
    [SerializeField] private Table[] tables;
    [SerializeField] private KitchenItem[] plats;

    [Header("Audio")]
    [SerializeField] private AudioSource clientSpawnAudioSource;

    private DayManager dayManager;

    private List<Table> availableTables;


    private readonly List<GameObject> clients = new();
    private List<(float, bool)> clientSpawnTimes = new();

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

        int totalClients = GameData.Instance.nbClients + GameData.Instance.nbClientsPremium;
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

        for (int c = 0; c < totalClients; c++)
        {
            float target = (c + 0.5f) / totalClients;
            int index = System.Array.FindIndex(cumulativeDistribution, val => val >= target);
            float timePercent = index / (float)(sampleCount - 1);
            float spawnTime = timePercent * dayManager.dayDurationInSeconds;
            clientSpawnTimes.Add((spawnTime, false)); // normal by default
        }

        // Randomly mark nbPremiumClients as premium
        for (int i = 0; i < GameData.Instance.nbClientsPremium; i++)
        {
            int idx;
            do
            {
                idx = Random.Range(0, totalClients);
            } while (clientSpawnTimes[idx].Item2);

            clientSpawnTimes[idx] = (clientSpawnTimes[idx].Item1, true);
        }

        clientSpawnTimes.Sort((a, b) => a.Item1.CompareTo(b.Item1));

        foreach ((float, bool) spawnData in clientSpawnTimes)
        {
            Debug.Log($"Client will spawn at: {spawnData.Item1}s (Premium: {spawnData.Item2})");
        }
    }


    private IEnumerator SpawnClientRoutine()
    {
        int nextClientIndex = 0;

        while (nextClientIndex < clientSpawnTimes.Count)
        {
            float currentTime = dayManager.CurrentTimePercent * dayManager.dayDurationInSeconds;

            if (currentTime >= clientSpawnTimes[nextClientIndex].Item1)
            {
                if (clients.Count < MAX_CLIENTS)
                {
                    SpawnClient(clientSpawnTimes[nextClientIndex].Item2);
                    nextClientIndex++;
                }
            }

            yield return null;
        }
    }


    private void SpawnClient(bool isPremium)
    {
        if (availableTables.Count == 0)
        {
            Debug.Log("No available tables to spawn a client.");
            return;
        }

        Debug.Log($"Spawning client {nbClients} (Premium: {isPremium})");

        Table table = availableTables[Random.Range(0, availableTables.Count)];
        availableTables.Remove(table);

        GameObject clientPrefab = isPremium ? premiumClientPrefabs[Random.Range(0, premiumClientPrefabs.Length)] : clientPrefabs[Random.Range(0, clientPrefabs.Length)];
        Debug.Log("Spawning client prefab: " + clientPrefab.name);
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

        clientSpawnAudioSource.Play();
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

    private void OnClientStartWaiting(Client client, bool isPremium)
    {
        //Le client affiche sur la table un plat random parmi la liste des plats
        KitchenItem plat = plats[Random.Range(0, plats.Length)];
        float waitTime = GameData.Instance.clientWaitTimeMultiplier * Random.Range(minClientWaitTime, maxClientWaitTime) * (isPremium ? .5f : 1f);
        client.targetTable.SetPlat(plat, waitTime);
    }

    private void OnClientFinished(Client client, ClientResult result, float satisfactionLevel)
    {
        client.targetTable.RemovePlat();

        KitchenItem dish = client.targetTable.expectedPlat;
        dayManager.dayStats.RecordClient(result, satisfactionLevel, dish);
    }

}
