using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPathRecorder : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] pathPoints;
    public float totalDuration = 10f;
    public bool smoothPath = true;

    [Header("Look At Settings")]
    public Transform lookAtTarget;
    public bool useLookAtTarget = false;
    public bool smoothRotation = true;
    public float rotationSpeed = 2f;

    [Header("Preview")]
    public bool showPath = true;
    public Color pathColor = Color.yellow;
    public Color pointColor = Color.red;
    public float pointSize = 0.5f;

    private Camera cam;
    private bool isPlaying = false;
    private float currentTime = 0f;
    private Vector3[] pathPositions;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (pathPoints.Length > 0)
        {
            GeneratePathPositions();
        }

        StartPlayback();
    }

    void Update()
    {
        if (isPlaying)
        {
            UpdateCameraPosition();

            currentTime += Time.deltaTime;

            if (currentTime >= totalDuration)
            {
                StopPlayback();
            }
        }
    }

    void GeneratePathPositions()
    {
        if (pathPoints.Length < 2) return;

        // Filter out null transforms and generate positions
        List<Vector3> validPositions = new List<Vector3>();
        foreach (Transform point in pathPoints)
        {
            if (point != null)
                validPositions.Add(point.position);
        }

        pathPositions = validPositions.ToArray();
    }

    void UpdateCameraPosition()
    {
        if (pathPositions == null || pathPositions.Length < 2) return;

        // Calculate progress from 0 to 1 over the total duration
        float normalizedTime = Mathf.Clamp01(currentTime / totalDuration);

        Vector3 newPosition;

        if (smoothPath && pathPositions.Length > 2)
        {
            newPosition = GetCatmullRomPosition(normalizedTime);
        }
        else
        {
            // For 2 points or linear mode, simple lerp from first to last
            newPosition = Vector3.Lerp(pathPositions[0], pathPositions[pathPositions.Length - 1], normalizedTime);
        }

        transform.position = newPosition;

        // Handle rotation
        if (useLookAtTarget && lookAtTarget != null)
        {
            Vector3 targetDirection = lookAtTarget.position - transform.position;
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                if (smoothRotation)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = targetRotation;
                }
            }
        }
        else if (pathPositions.Length > 1)
        {
            // Look along the path direction
            Vector3 forwardDirection = GetPathDirection(normalizedTime);
            if (forwardDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(forwardDirection);
                if (smoothRotation)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = targetRotation;
                }
            }
        }
    }

    Vector3 GetLinearPosition(float t)
    {
        if (pathPositions.Length == 0) return transform.position;
        if (pathPositions.Length == 1) return pathPositions[0];

        t = Mathf.Clamp01(t);

        // For 2 points, simple lerp
        if (pathPositions.Length == 2)
        {
            return Vector3.Lerp(pathPositions[0], pathPositions[1], t);
        }

        // For multiple points
        float scaledT = t * (pathPositions.Length - 1);
        int index = Mathf.FloorToInt(scaledT);
        float localT = scaledT - index;

        // Clamp to valid indices
        index = Mathf.Clamp(index, 0, pathPositions.Length - 2);

        return Vector3.Lerp(pathPositions[index], pathPositions[index + 1], localT);
    }

    Vector3 GetCatmullRomPosition(float t)
    {
        if (pathPositions.Length < 2) return transform.position;

        // For 2 points, use linear interpolation from first to last
        if (pathPositions.Length == 2)
        {
            return Vector3.Lerp(pathPositions[0], pathPositions[1], t);
        }

        // For 3+ points, use Catmull-Rom spline through all points
        t = Mathf.Clamp01(t);
        float scaledT = t * (pathPositions.Length - 1);
        int index = Mathf.FloorToInt(scaledT);
        float localT = scaledT - index;

        // Clamp index to valid range
        index = Mathf.Clamp(index, 0, pathPositions.Length - 2);

        Vector3 p0, p1, p2, p3;

        // Handle edge cases for Catmull-Rom
        if (index == 0)
        {
            p0 = pathPositions[0] - (pathPositions[1] - pathPositions[0]); // Extrapolate backwards
            p1 = pathPositions[0];
            p2 = pathPositions[1];
            p3 = pathPositions.Length > 2 ? pathPositions[2] : pathPositions[1] + (pathPositions[1] - pathPositions[0]);
        }
        else if (index >= pathPositions.Length - 2)
        {
            int lastIndex = pathPositions.Length - 1;
            p0 = pathPositions.Length > 2 ? pathPositions[lastIndex - 2] : pathPositions[lastIndex - 1] - (pathPositions[lastIndex] - pathPositions[lastIndex - 1]);
            p1 = pathPositions[lastIndex - 1];
            p2 = pathPositions[lastIndex];
            p3 = pathPositions[lastIndex] + (pathPositions[lastIndex] - pathPositions[lastIndex - 1]); // Extrapolate forwards
        }
        else
        {
            p0 = pathPositions[index - 1];
            p1 = pathPositions[index];
            p2 = pathPositions[index + 1];
            p3 = pathPositions[index + 2];
        }

        return CatmullRom(p0, p1, p2, p3, localT);
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    Vector3 GetPathDirection(float t)
    {
        if (pathPositions.Length < 2) return Vector3.forward;

        // For simple 2-point movement, direction is straight from first to last
        if (pathPositions.Length == 2)
        {
            return (pathPositions[1] - pathPositions[0]).normalized;
        }

        // For multi-point paths, calculate direction along the curve
        float epsilon = 0.005f; // Smaller epsilon for more accurate direction
        float t1 = Mathf.Clamp01(t);
        float t2 = Mathf.Clamp01(t + epsilon);

        Vector3 currentPos, nextPos;

        if (smoothPath && pathPositions.Length > 2)
        {
            currentPos = GetCatmullRomPosition(t1);
            nextPos = GetCatmullRomPosition(t2);
        }
        else
        {
            // For linear multi-point, interpolate through all points
            currentPos = GetLinearPosition(t1);
            nextPos = GetLinearPosition(t2);
        }

        Vector3 direction = (nextPos - currentPos).normalized;
        return direction != Vector3.zero ? direction : Vector3.forward;
    }

    public void StartPlayback()
    {
        if (pathPoints.Length < 2)
        {
            Debug.LogWarning("Need at least 2 path points to start playback!");
            return;
        }

        GeneratePathPositions();

        if (pathPositions.Length < 2)
        {
            Debug.LogWarning("Need at least 2 valid (non-null) path points to start playback!");
            return;
        }

        currentTime = 0f;
        isPlaying = true;

        // Set initial position
        transform.position = pathPositions[0];

        Debug.Log("Playback started - Press SPACE to stop, R to record");
    }

    public void StopPlayback()
    {
        isPlaying = false;
        currentTime = 0f;
        Debug.Log("Playback stopped");
    }

    public void SetCameraToFirstPos()
    {
        if (pathPoints != null && pathPoints.Length > 0)
        {
            transform.position = pathPoints[0].position;
        }
        else
        {
            Debug.LogWarning("No valid path positions to set camera position.");
        }
    }

    void OnDrawGizmos()
    {
        if (!showPath || pathPoints == null || pathPoints.Length < 2) return;

        // Filter out null transforms
        List<Transform> validPoints = new List<Transform>();
        foreach (Transform point in pathPoints)
        {
            if (point != null)
                validPoints.Add(point);
        }

        if (validPoints.Count < 2) return;

        // Draw path points
        Gizmos.color = pointColor;
        foreach (Transform point in validPoints)
        {
            Gizmos.DrawWireSphere(point.position, pointSize);
        }

        // Draw path
        Gizmos.color = pathColor;

        if (validPoints.Count == 2)
        {
            // Simple line for 2 points
            Gizmos.DrawLine(validPoints[0].position, validPoints[1].position);
        }
        else
        {
            // Draw smooth or linear path for 3+ points
            for (int i = 0; i < validPoints.Count - 1; i++)
            {
                if (smoothPath && validPoints.Count > 2)
                {
                    // Draw smooth curve
                    Vector3 lastPos = validPoints[i].position;
                    for (float t = 0.1f; t <= 1f; t += 0.1f)
                    {
                        float globalT = (i + t) / (validPoints.Count - 1);
                        Vector3 currentPos = GetCatmullRomPosition(globalT);
                        Gizmos.DrawLine(lastPos, currentPos);
                        lastPos = currentPos;
                    }
                }
                else
                {
                    Gizmos.DrawLine(validPoints[i].position, validPoints[i + 1].position);
                }
            }
        }

        // Draw look at target
        if (useLookAtTarget && lookAtTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(lookAtTarget.position, Vector3.one * 0.5f);
        }
    }
}