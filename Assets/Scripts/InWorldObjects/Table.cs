using UnityEngine;

public class Table : MonoBehaviour
{
    public Transform chairPos;
    public Client currentClient;

    public bool isOccupied = false;

    public void SetClient(Client client)
    {
        currentClient = client;
        isOccupied = true;
    }

    public void RemoveClient()
    {
        currentClient = null;
        isOccupied = false;
    }

    public void SetPlat()
    {

    }
}
