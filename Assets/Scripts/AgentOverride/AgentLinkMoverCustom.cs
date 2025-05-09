using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.AI.Navigation;
public enum OffMeshLinkMoveMethod
{
    Teleport,
    NormalSpeed,
    Parabola,
    Curve
}

/// <summary>
/// Move an agent when traversing a OffMeshLink given specific animated methods
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AgentLinkMoverCustom : MonoBehaviour
{
    public OffMeshLinkMoveMethod m_Method = OffMeshLinkMoveMethod.Parabola;
    public AnimationCurve m_Curve = new AnimationCurve();
    public Door doorScript;

    IEnumerator Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        doorScript = FindAnyObjectByType<Door>();
        agent.autoTraverseOffMeshLink = false;
        while (true)
        {
            if (agent.isOnOffMeshLink)
            {
                if (m_Method == OffMeshLinkMoveMethod.NormalSpeed)
                    yield return StartCoroutine(NormalSpeed(agent));
                else if (m_Method == OffMeshLinkMoveMethod.Parabola)
                    yield return StartCoroutine(Parabola(agent, 2.0f, 0.5f));
                else if (m_Method == OffMeshLinkMoveMethod.Curve)
                    yield return StartCoroutine(Curve(agent, 0.5f));
                agent.CompleteOffMeshLink();
            }

            yield return null;
        }
    }

    IEnumerator NormalSpeed(NavMeshAgent agent)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

        //TODO : Syst�me de rotation quand un agent traverse un OffMeshLink

        NavMeshLink currentLink = data.owner as NavMeshLink;

        if (currentLink != null && currentLink.gameObject.name == "DoorLink" && doorScript != null)
        {
            doorScript.OpenDoor(gameObject);
        }

        while (agent.transform.position != endPos)
        {
            agent.transform.position =
                Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
            yield return null;
        }
        if (currentLink != null && currentLink.gameObject.name == "DoorLink" && doorScript != null)
        {
            doorScript.CloseDoor(gameObject); //Fermeture de la porte
        }

    }

    IEnumerator Parabola(NavMeshAgent agent, float height, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }

    IEnumerator Curve(NavMeshAgent agent, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = m_Curve.Evaluate(normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }
}
