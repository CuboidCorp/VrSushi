using UnityEngine;
using UnityEngine.AI;
using System;

public class Client : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private Transform despawnPoint;
    private bool isSatisfied = false;
    private bool isDespawned = false;
    private bool isWaiting = false;

    // Événement déclenché lorsque le client commence à attendre
    public event Action<Client> OnStartWaiting;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!isSatisfied && HasReachedDestination(targetPosition.position))
        {
            if (!isWaiting)
            {
                Debug.Log($"Client {gameObject.name} has reached the target position and is starting to wait.");
                StartWaiting();
            }
        }
        else if (isSatisfied && !isDespawned && HasReachedDestination(despawnPoint.position))
        {
            Debug.Log($"Client {gameObject.name} has reached the despawn point and will be destroyed.");
            Destroy(gameObject); // Détruit le client une fois arrivé au despawnPoint
            isDespawned = true;
        }
    }

    public void StartClient()
    {
        Debug.Log($"Client {gameObject.name} initialized. Moving to target position.");
        MoveToTargetPosition();
    }

    public void Satisfy()
    {
        isSatisfied = true;
        isWaiting = false; // Arrête l'état d'attente
        Debug.Log($"Client {gameObject.name} is satisfied and moving to the despawn point.");
        MoveToDespawnPoint();
    }

    public void SetTargetPosition(Transform target)
    {
        targetPosition = target;
        Debug.Log($"Client {gameObject.name} target position set to {target.position}.");
    }

    public void SetDespawnPoint(Transform despawn)
    {
        despawnPoint = despawn;
        Debug.Log($"Client {gameObject.name} despawn point set to {despawn.position}.");
    }

    private void MoveToTargetPosition()
    {
        if (targetPosition != null)
        {
            Debug.Log($"Client {gameObject.name} is moving to target position at {targetPosition.position}.");
            bool hasWorked = agent.SetDestination(targetPosition.position);
            if (!hasWorked)
            {
                Debug.LogError($"Client {gameObject.name} failed to set destination to {targetPosition.position}.");
            }
        }
        else
        {
            Debug.LogWarning($"Client {gameObject.name} has no target position set.");
        }
    }

    private void MoveToDespawnPoint()
    {
        if (despawnPoint != null)
        {
            Debug.Log($"Client {gameObject.name} is moving to despawn point at {despawnPoint.position}.");
            agent.isStopped = false; // Redémarre le mouvement si arrêté
            agent.SetDestination(despawnPoint.position);
        }
        else
        {
            Debug.LogWarning($"Client {gameObject.name} has no despawn point set.");
        }
    }

    private bool HasReachedDestination(Vector3 destination)
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void StartWaiting()
    {
        isWaiting = true;
        agent.isStopped = true; // Arrête le client à la targetPosition
        Debug.Log($"Client {gameObject.name} is now waiting at the target position.");
        OnStartWaiting?.Invoke(this); // Déclenche l'événement
    }
}
