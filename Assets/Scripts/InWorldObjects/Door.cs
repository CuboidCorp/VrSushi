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
        users.Add(go);
        if (users.Count == 1)
            animator.SetTrigger("Open");
    }

    public void CloseDoor(GameObject go)
    {
        users.Remove(go);
        if (users.Count == 0)
            animator.SetTrigger("Close");
    }
}
