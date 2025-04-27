using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [SerializeField] private float clientInterval = 60f;
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform despawnPoint;
    [SerializeField] private Transform[] clientPositions;

    private List<GameObject> clients = new List<GameObject>();
    private Coroutine spawnClientCoroutine;
    private const int maxClients = 10;

    private void Start()
    {
        spawnClientCoroutine = StartCoroutine(SpawnClientRoutine());
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
        GameObject clientGo = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
        Client client = clientGo.GetComponent<Client>();
        client.SetTargetPosition(clientPositions[Random.Range(0, clientPositions.Length)]);
        client.SetDespawnPoint(despawnPoint);
        client.StartClient();
        client.OnStartWaiting += OnClientStartWaiting;
        clients.Add(clientGo);
    }

    private void OnClientStartWaiting(Client client)
    {
        StartCoroutine(HandleClientWaiting(client));
    }

    private IEnumerator HandleClientWaiting(Client client)
    {
        yield return new WaitForSeconds(3f); // Le client attend 3 secondes
        client.Satisfy();
    }

    private void RemoveClient(GameObject clientGo)
    {
        clients.Remove(clientGo);
    }
}
