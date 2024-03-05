using UnityEngine;

public class BaseHexagon : MonoBehaviour
{
    // Original material of the hexagon
    public Material originalMaterial;

    private void OnTriggerEnter(Collider other)
    {
        TileStackController stack = other.GetComponent<TileStackController>();
        if (stack != null)
        {
            BaseHexagon currentBase = stack.GetCurrentBaseHexagon();
            if (currentBase != null)
            {
                // Place the stack on this base hexagon
                stack.transform.position = transform.position;
                stack.transform.SetParent(transform);
            }
        }
    }
}
