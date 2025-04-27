using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Client : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private Transform despawnPoint;
    private bool isSatisfied = false;
    private bool isDespawned = false;
    private bool isWaiting = false;
    private bool isEating = false;
    private Animator animator;

    // Time settings for dining
    [SerializeField] private float eatingDuration = 15f;

    public event Action<Client> OnStartWaiting;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateAnimatorParameters();

        if (!isSatisfied && HasReachedDestination())
        {
            if (!isWaiting)
            {
                Debug.Log($"Client {gameObject.name} has reached the target position and is starting to wait.");
                StartWaiting();
            }
        }
        else if (isSatisfied && !isDespawned && HasReachedDestination())
        {
            Debug.Log($"Client {gameObject.name} has reached the despawn point and will be destroyed.");
            Destroy(gameObject); // Détruit le client une fois arrivé au despawnPoint
            isDespawned = true;
        }
    }

    private void UpdateAnimatorParameters()
    {
        float speed = agent.velocity.magnitude;
        bool isMoving = speed > 0.1f;

        animator.SetBool("isWalking", isMoving);

        // Update sitting parameters
        animator.SetBool("isSitting", isWaiting && !isSatisfied);
        animator.SetBool("isEating", isEating);
    }

    public void StartClient()
    {
        Debug.Log($"Client {gameObject.name} initialized. Moving to target position.");
        MoveToTargetPosition();
    }

    public void Satisfy()
    {
        StopAllCoroutines();
        isWaiting = false;
        isEating = false;

        Debug.Log($"Client {gameObject.name} is satisfied and moving to the despawn point.");

        // Let the Sitting_End animation play before moving
        StartCoroutine(LeaveAfterAnimation());
    }

    private IEnumerator LeaveAfterAnimation()
    {
        // Wait for the sitting end animation to finish
        // Assuming Sitting_End takes about 1 second
        yield return new WaitForSeconds(1f);

        // Now move to the despawn point
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
            isSatisfied = true;
        }
        else
        {
            Debug.LogWarning($"Client {gameObject.name} has no despawn point set.");
        }
    }

    private bool HasReachedDestination()
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

        // Start the eating sequence after sitting
        StartCoroutine(EatingSequence());
    }

    private IEnumerator EatingSequence()
    {
        // Wait for the Sitting_Start animation to complete (about 1-2 seconds)
        yield return new WaitForSeconds(1.5f);

        // Client starts eating
        isEating = true;
        Debug.Log($"Client {gameObject.name} is eating.");

        // Client eats for the specified duration
        yield return new WaitForSeconds(eatingDuration);

        // Client finishes eating but stays seated
        isEating = false;
        Debug.Log($"Client {gameObject.name} has finished eating.");

        // Wait a bit in Sitting_Idle before being satisfied
        yield return new WaitForSeconds(2f);

        // Auto-satisfy the client after they've finished eating
        // (You might want to replace this with your own satisfaction logic)
        if (!isSatisfied)
        {
            Satisfy();
        }
    }

    // Add this method to trigger the hit reaction when needed
    public void TriggerHitReaction()
    {
        if (animator != null)
        {
            animator.SetTrigger("gotHit");
        }
    }
}