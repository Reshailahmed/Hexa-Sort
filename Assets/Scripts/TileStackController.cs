using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStackController : MonoBehaviour
{
    private bool isDragging = false;
    private BaseHexagon currentBaseHexagon = null;
    private Vector3 mOffset;
    private float mZCoord;
    private Vector3 originalPosition;
    private BaseHexagon hoveredHexagon;
    private bool lockHexagon;

    // The size of the collider
    public Vector3 colliderSize = new Vector3(20f, 1f, 20f);

    public Stack<string> prefabNamesStack;

    private void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(transform.position).y;
        mOffset = transform.position - GetMouseAsWorldPoint();

        //Debug.LogError("prefabNamesStack:");
        //foreach (string prefabName in prefabNamesStack)
        //{
        //    Debug.LogError(prefabName);
        //}
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDrag()
    {
        // Cast a ray downwards from the stack to check for base hexagons underneath
        Ray ray = new Ray(transform.position, -Vector3.up);

        // Visualize the ray in the Scene view during gameplay
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            BaseHexagon hexagon = hit.collider.GetComponent<BaseHexagon>();
            if (hexagon != null)
            {
                // If the stack is hovering over a different hexagon than before,
                // revert the material of the previously hovered hexagon (if any)
                if (hoveredHexagon != hexagon)
                {
                    RestoreMaterial();
                    hoveredHexagon = hexagon;
                }

                // Change the material of the hovered hexagon to highlight material
                hexagon.originalMaterial = hexagon.GetComponent<Renderer>().material;
                hexagon.GetComponent<Renderer>().material = GameManager.Instance.GetHighlightMaterial();
            }
            else
            {
                // If the stack is not hovering over any hexagon, restore the material
                RestoreMaterial();
            }
        }
        else
        {
            // If the stack is not hovering over any hexagon, restore the material
            RestoreMaterial();
        }

        isDragging = true;
        Vector3 newPosition = GetMouseAsWorldPoint() + mOffset;

        // Calculate the distance from the initial touch point
        float distanceFromInitialTouch = Mathf.Abs(newPosition.z - transform.position.z);

        // Define a lerp factor curve
        AnimationCurve lerpCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 2f); // Adjust the curve as needed

        // Evaluate the lerp factor from the curve based on the distance
        float lerpFactor = lerpCurve.Evaluate(Mathf.Clamp01(distanceFromInitialTouch / 10f)); // Adjust the divisor as needed

        // Apply the lerp factor to the Z position
        newPosition.z = Mathf.Lerp(transform.position.z, newPosition.z, lerpFactor);

        // Update the position
        transform.position = new Vector3(newPosition.x, 20, newPosition.z);
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Raycast to check for base hexagons underneath the stack
        RaycastHit hit;
        // Cast a ray downwards from the stack to check for base hexagons underneath
        Ray ray = new Ray(transform.position, -Vector3.up);
        // Visualize the ray in the Scene view during gameplay
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);

        if (Physics.Raycast(ray, out hit))
        {
            BaseHexagon hexagon = hit.collider.GetComponent<BaseHexagon>();
            if (hexagon != null)
            {
                // Snap the stack to the base hexagon
                SnapToHexagon(hexagon);
                hexagon.GetComponent<Renderer>().material = GameManager.Instance.GetBaseMaterial();
                hoveredHexagon = hexagon;
                return;
            }
        }

        // If no base hexagon is found under the stack, reset its position
        transform.position = originalPosition;
    }

    // Snap the stack to the specified base hexagon
    private void SnapToHexagon(BaseHexagon hexagon)
    {
        transform.position = hexagon.transform.position + new Vector3(0, 1.5f, 0);
        currentBaseHexagon = hexagon;
        lockHexagon = true;
        hexagon.GetComponent<MeshCollider>().enabled = false;
        transform.SetParent(hexagon.transform);
        GameManager.Instance.StackRemoved();
        GameManager.Instance.CheckSurroundingTiles(this.gameObject);
    }

    public BaseHexagon GetCurrentBaseHexagon()
    {
        return currentBaseHexagon;
    }

    // Dynamically add a collider to the GameObject if it doesn't have one
    private void Awake()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        if (!collider)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
        // Set collider size
        collider.size = colliderSize;
        // Set collider center to match the parent position
        collider.center = transform.position - transform.parent.position;
        // Adjust z-coordinate of the collider center
        collider.center = new Vector3(-collider.center.x, collider.center.y, -collider.center.z);

        originalPosition = transform.position;
    }

    private void RestoreMaterial()
    {
        if (hoveredHexagon != null)
        {
            hoveredHexagon.GetComponent<Renderer>().material = GameManager.Instance.GetBaseMaterial();
            hoveredHexagon = null; // Reset the hovered hexagon
        }
    }

    //public void SetPrefabNamesStack(Stack<string> stack)
    //{
    //    prefabNamesStack = stack;
    //}

    //public int GetTileCount()
    //{
    //    return prefabNamesStack.Count;
    //}

    //public string GetTopTile()
    //{
    //    if (prefabNamesStack.Count > 0)
    //    {
    //        return prefabNamesStack.Peek();
    //    }
    //    else
    //    {
    //        return null; // Return a default color if the stack is empty
    //    }
    //}


    //public void TransferSamePrefabTiles(TileStackController targetStack)
    //{
    //    //prefabNamesStack.Push(targetStack.prefabNamesStack.Pop());
    //    Debug.Log("Before");
    //    PrintStackContents(targetStack.prefabNamesStack, targetStack.transform.parent.name);
    //    targetStack.prefabNamesStack.Pop();
    //    Debug.Log("After");
    //    PrintStackContents(targetStack.prefabNamesStack, targetStack.transform.parent.name);
    //    //handle reward
    //    //GameManager.Instance.RewardSystem(this.gameObject.transform.GetChild(this.transform.childCount - 1).gameObject);

    //    if (targetStack.prefabNamesStack.Count == 0)
    //    {
    //        targetStack.transform.parent.GetComponent<MeshCollider>().enabled = true;
    //        targetStack.prefabNamesStack.Clear();
    //        DestroyGameObject(targetStack.gameObject);
    //        Debug.Log("GameObjectDestroyed: " + targetStack);
    //    }

    //    //PrintStackContents(prefabNamesStack, transform.parent.name);
    //    //PrintStackContents(targetStack.prefabNamesStack, targetStack.transform.parent.name);

    //    GameManager.Instance.CheckSurroundingTiles(this.gameObject);
    //    if (targetStack)
    //    {
    //        GameManager.Instance.CheckSurroundingTiles(targetStack.gameObject);
    //    }
    //    //string topColor = GetTopTile();
    //    //if (targetStack != null && topColor == targetStack.GetTopTile())
    //    //{
    //    //    targetStack.AddSameColorTiles(prefabNamesStack);
    //    //    prefabNamesStack.Clear();
    //    //}
    //}    
    public string GetTopColor()
    {
        if (transform.childCount != 0)
        {
            if (transform.GetChild(transform.childCount - 1).childCount > 0)
            {
                return transform.GetChild(transform.childCount - 1).name;
            }
            else
            {
                return null; // Return a default color if the stack is empty
            }
        }
        else
            return null;
    }

    // Method to print the contents of the stack to the console
    public void PrintStackContents(Stack<string> S, string stackName)
    {
        Debug.Log("stackName: " + stackName);
        foreach (string item in S)
        {
            Debug.Log(item);
        }
    }

    public void DestroyGameObject(GameObject gameObject)
    {
        Destroy(gameObject);
    }
}
