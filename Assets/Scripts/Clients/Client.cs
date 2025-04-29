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

    private bool isServed = false;
    private bool hasOverwaited = false;
    private Animator animator;
    [HideInInspector] public Table targetTable;

    // Time settings for dining
    [SerializeField] private float eatingDuration = 15f;

    public event Action<Client> OnStartWaiting;
    public event Action<Client, float> OnClientSatisfaction;
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

    public void Satisfy(float satifactionLevel)
    {
        StopAllCoroutines();
        isWaiting = false;
        isEating = false;

        if (satifactionLevel < 0)
        {
            Debug.Log($"Client {gameObject.name} is unsatisfied and leaving.");
        }
        else
        {
            Debug.Log($"Client {gameObject.name} is satisfied and leaving.");
        }

        OnClientSatisfaction?.Invoke(this, satifactionLevel);

        StartCoroutine(LeaveAfterAnimation());
    }

    private IEnumerator LeaveAfterAnimation()
    {
        yield return WaitForAnimation("Character Armature|Sitting_End");

        animator.SetTrigger("StartWalk");
        MoveToDespawnPoint();
    }

    public void SetTargetTable(Table table)
    {
        targetTable = table;
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

        //On ajoute des listeners au events de la table 
        targetTable.OnPlatPlaced += Serve;
        targetTable.OnPlatTimeout += WaitTimeOut;
        OnStartWaiting?.Invoke(this);

        StartCoroutine(EatingSequence());
    }

    private void Serve(bool isCorrectPlat)
    {
        isServed = true;
        if (isCorrectPlat)
        {
            Debug.Log($"Client {gameObject.name} has been served correctly.");
        }
        else
        {
            Debug.Log($"Client {gameObject.name} has been served incorrectly.");
        }
    }

    private void WaitTimeOut()
    {
        hasOverwaited = true;
    }

    private IEnumerator EatingSequence()
    {
        yield return WaitForAnimation("Character Armature|Sitting_Start");

        Debug.Log($"Client {gameObject.name} attend d'être servi.");

        yield return new WaitUntil(() => isServed || hasOverwaited);

        if (hasOverwaited)
        {
            Debug.Log($"Client {gameObject.name} a overwait et part sans manger.");
            Satisfy(-5);
            yield break;
        }

        isEating = true;
        Debug.Log($"Client {gameObject.name} commence à manger.");

        yield return new WaitForSeconds(eatingDuration);

        isEating = false;
        Debug.Log($"Client {gameObject.name} a terminé de manger.");

        yield return new WaitForSeconds(2f);

        if (!isSatisfied)
        {
            Satisfy(5);
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

    public void TriggerHitReaction()
    {
        if (animator != null)
        {
            animator.SetTrigger("gotHit");
        }
    }
}