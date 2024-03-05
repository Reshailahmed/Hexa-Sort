using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    // List to store the collided parents
    private HashSet<GameObject> collidedParents = new HashSet<GameObject>();

    // Method to return the collided parents
    public GameObject[] GetCollidedParents()
    {
        // Convert the hash set to an array
        GameObject[] parents = new GameObject[collidedParents.Count];
        collidedParents.CopyTo(parents);
        return parents;
    }

    // Called when another collider enters this collider's trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has a parent
        if (other.transform.parent != null)
        {
            // Add the parent to the collidedParents set
            collidedParents.Add(other.transform.parent.gameObject);
        }
    }
}
