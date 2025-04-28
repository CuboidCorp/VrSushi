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

    private bool isServed = false; // Added to track if the client has been served
    private Animator animator;
    [HideInInspector] public Table target;

    // Time settings for dining
    [SerializeField] private float eatingDuration = 15f;

    public event Action<Client> OnStartWaiting;
    public event Action<Client> OnClientSatisfaction;
    public event Action<Client> OnClientDespawn;

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
            OnClientDespawn?.Invoke(this); // Trigger the despawn event
            return; //Le client va être détruit dans tous les cas
        }
    }

    private void UpdateAnimatorParameters()
    {
        // Update sitting parameters
        animator.SetBool("isSitting", isWaiting && !isSatisfied);
        animator.SetBool("isEating", isEating);
    }

    public void StartClient()
    {
        Debug.Log($"Client {gameObject.name} initialized. Moving to target position.");
        animator.SetTrigger("StartWalk");
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
        yield return WaitForAnimation("Character Armature|Sitting_End");

        animator.SetTrigger("StartWalk");
        MoveToDespawnPoint();
    }

    public void SetTargetTable(Table table)
    {
        target = table;
        targetPosition = table.chairPos;
    }

    public void SetDespawnPoint(Transform despawn)
    {
        despawnPoint = despawn;
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
        agent.isStopped = true;
        animator.SetTrigger("StopWalk");
        Debug.Log($"Client {gameObject.name} is now waiting at the target position.");

        Vector3 targetRotation = new(0, -90, 0);
        transform.rotation = Quaternion.Euler(targetRotation);

        OnStartWaiting?.Invoke(this); // Déclenche l'événement

        // Start the eating sequence after sitting
        StartCoroutine(EatingSequence());
    }

    private IEnumerator EatingSequence()
    {
        //Wait for the Sitting_Start animation to finish
        yield return WaitForAnimation("Character Armature|Sitting_Start");

        Debug.Log($"Client {gameObject.name} attend d'être servi.");
        yield return new WaitUntil(() => isServed);

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

    private IEnumerator WaitForAnimation(string animationName)
    {
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null; // Attendre la fin de l'image
        }
    }

    public void ServeClient()
    {
        isServed = true;
        Debug.Log($"Client {gameObject.name} a été servi.");
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