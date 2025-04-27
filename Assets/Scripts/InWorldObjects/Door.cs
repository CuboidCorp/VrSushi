using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Door : MonoBehaviour
{
    private Animator animator;
    private List<GameObject> users;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        users = new List<GameObject>();
    }

    public void OpenDoor(GameObject go)
    {
        Debug.Log($"Client {go.name} is opening the door.");
        users.Add(go);
        Debug.Log($"Users in the door: {users.Count}");
        if (users.Count == 1)
            animator.SetTrigger("Open");
    }

    public void CloseDoor(GameObject go)
    {
        Debug.Log($"Client {go.name} is closing the door.");
        users.Remove(go);
        Debug.Log($"Users in the door: {users.Count}");
        if (users.Count == 0)
            animator.SetTrigger("Close");
    }
}
