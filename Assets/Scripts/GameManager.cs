using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance of the GameManager
    public static GameManager Instance;

    // Material used for highlighting
    public Material highlightMaterial;
    public Material baseMaterial;

    public bool rewardBool;

    public List<PlacableTileSpawner> spawners; // List to hold references to all PlacableTileSpawner objects
    List<GameObject> stacksToCheck = new List<GameObject>();

    private void Awake()
    {
        // Ensure only one instance of the GameManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to get the highlight material
    public Material GetHighlightMaterial()
    {
        return highlightMaterial;
    }

    public Material GetBaseMaterial()
    {
        return baseMaterial;
    }

    // Method to be called when a stack is removed from a position
    public void StackRemoved()
    {
        // Check if all position gameobjects are empty
        foreach (PlacableTileSpawner spawner in spawners)
        {
            if (!spawner.IsPositionEmpty())
            {
                // If any position still has a stack, return early
                return;
            }
        }

        // If all positions are empty, spawn new stacks at each position
        foreach (PlacableTileSpawner spawner in spawners)
        {
            spawner.SpawnPrefabs();
        }
    }

    #region TransferTiles
    public void CheckSurroundingTiles(GameObject stack)
    {
        //RewardSystem(stack.transform.GetChild(stack.transform.childCount - 1).gameObject);
        TileStackController currentStack = stack.GetComponent<TileStackController>();
        GameObject[] neighbourTiles = currentStack.transform.parent.GetChild(0).GetComponent<CollisionDetector>().GetCollidedParents();
        string currentStackTopColor = currentStack.GetTopColor();

        Debug.LogError("---------Start---------");
        foreach (GameObject neighbourTile in neighbourTiles)
        {
            if (neighbourTile.transform.childCount > 1)
            {
                TileStackController neighbourStack = neighbourTile.transform.GetChild(1).GetComponent<TileStackController>();
                Debug.LogError("Comparison: " + currentStack.transform.parent.name + " " + currentStackTopColor + "   " + neighbourTile.name + " " + neighbourStack.GetTopColor());
                if (neighbourStack.GetTopColor() == currentStackTopColor)
                {
                    if (!stacksToCheck.Contains(stack))
                    {
                        stacksToCheck.Add(stack);
                    }
                    TransferTiles(neighbourStack, currentStack);
                    break;
                }
            }
        }
        Debug.LogError("---------End---------");
    }

    // Method to transfer tiles of the same color from one stack to another
    public void TransferTiles(TileStackController existingStack, TileStackController newStack)
    {
        //check to always transfer tiles from the stack containing less colors to the stack containing more
        if (existingStack.transform.childCount <= newStack.transform.childCount)
        {
            // Transfer tiles of the same prefab from existing stack to new stack
            TransferPrefabs(existingStack.gameObject, newStack.gameObject);
            //newStack.TransferSamePrefabTiles(existingStack);
        }
        else
        {
            // Transfer tiles of the same prefab from existing stack to new stack
            TransferPrefabs(newStack.gameObject, existingStack.gameObject);
            //existingStack.TransferSamePrefabTiles(newStack);
        }
    }

    public void TransferPrefabs(GameObject existingStack, GameObject newStack)
    {
        Transform existingStackTransform = existingStack.transform;
        Transform newStackTransform = newStack.transform;

        //TransferTilesWithAnim(existingStackTransform.GetChild(existingStackTransform.childCount - 1), newStackTransform);
        while (existingStackTransform.GetChild(existingStackTransform.childCount - 1).childCount > 0)
        {
            Transform existingTile = existingStackTransform.GetChild(existingStack.transform.childCount - 1).GetChild(0);
            Transform newTile = newStackTransform.GetChild(newStack.transform.childCount - 1);
            existingTile.position = newTile.GetChild(newTile.childCount - 1).position + new Vector3(0, 1.5f, 0);
            existingTile.parent = newStackTransform.GetChild(newStack.transform.childCount - 1);
        }

        //delete that color from the donor stack
        Destroy(existingStackTransform.GetChild(existingStack.transform.childCount - 1).gameObject);

        // Schedule the execution of the if statement at the end of the frame
        StartCoroutine(DelayedExecution());

        IEnumerator DelayedExecution()
        {
            yield return null; // Wait for the end of the frame

            // Now the child has been destroyed and the if statement can be executed
            if (existingStack.transform.childCount == 0)
            {
                existingStack.transform.parent.gameObject.GetComponent<MeshCollider>().enabled = true;
                Destroy(existingStack);
            }
            CheckSurroundingTiles(existingStack);
            CheckSurroundingTiles(newStack);

            RewardSystem();
        }
    }

    //public void TransferTilesWithAnim(Transform existingStackTransform, Transform newStackTransform)
    //{
    //    StartCoroutine(AnimateTiles(existingStackTransform, newStackTransform));
    //}

    //private IEnumerator AnimateTiles(Transform existingStackTransform, Transform newStackTransform)
    //{
    //    // Gather existing tiles
    //    List<Transform> existingTiles = new List<Transform>();
    //    while (existingStackTransform.childCount > 0)
    //    {
    //        Transform existingTile = existingStackTransform.GetChild(0);
    //        existingTiles.Add(existingTile);
    //        existingTile.SetParent(newStackTransform.GetChild(newStackTransform.childCount - 1)); // Detach from parent
    //    }

    //    // Define target positions and rotations
    //    List<Vector3> targetPositions = new List<Vector3>();
    //    List<Quaternion> targetRotations = new List<Quaternion>();
    //    foreach (Transform existingTile in existingTiles)
    //    {
    //        float f = 1.5f;
    //        Transform newTile = newStackTransform.GetChild(newStackTransform.childCount - 1);
    //        Vector3 targetPosition = newTile.GetChild(newTile.childCount - 1).position + new Vector3(0, f, 0);
    //        Quaternion targetRotation = Quaternion.Euler(0,0,180) * newTile.GetChild(newTile.childCount - 1).rotation;
    //        targetPositions.Add(targetPosition);
    //        targetRotations.Add(targetRotation);
    //        f++;
    //    }

    //    // Duration of the animation
    //    float duration = 1.0f; // Adjust as needed

    //    // Animation progress
    //    float progress = 0.0f;
    //    while (progress < 1.0f)
    //    {
    //        progress += Time.deltaTime / duration;

    //        // Interpolate position and rotation for each tile
    //        for (int i = 0; i < existingTiles.Count; i++)
    //        {
    //            existingTiles[i].position = Vector3.Lerp(existingTiles[i].position, targetPositions[i], progress);
    //            existingTiles[i].rotation = Quaternion.Slerp(existingTiles[i].rotation, targetRotations[i], progress);
    //        }

    //        yield return null; // Wait for the next frame
    //    }

    //    // Ensure final position and rotation match the target exactly
    //    for (int i = 0; i < existingTiles.Count; i++)
    //    {
    //        existingTiles[i].position = targetPositions[i];
    //        existingTiles[i].rotation = targetRotations[i];
    //    }
    //    yield return null;
    //}



    #endregion

    #region Point Rewarding System
    public void RewardSystem()
    {
        foreach (GameObject stack in stacksToCheck)
        {
            if (stack)
            {
                if (stack.transform.GetChild(stack.transform.childCount - 1).childCount > 0)
                {
                    Destroy(stack.transform.GetChild(stack.transform.childCount - 1).gameObject);
                }
            }
        }

        // Clear the list after processing
        stacksToCheck.Clear();
    }

    #endregion
}
