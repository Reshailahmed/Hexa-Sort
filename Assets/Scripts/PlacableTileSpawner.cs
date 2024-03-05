using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacableTileSpawner : MonoBehaviour
{
    public GameObject[] prefabs; // Array to hold different colored prefabs
    private int maxPrefabCount = 10; // Maximum number of prefabs to spawn
    private int numberOfColors;
    private int numberOfPrefabsPerColor;
    public float prefabSpacing = 0.2f; // Spacing between each spawned prefab on the Y-axis

    private void Start()
    {
        SpawnPrefabs();
    }

    public void SpawnPrefabs()
    {
        // Shuffle the prefabs array to get random distribution
        ShuffleArray(prefabs);

        int totalSpawned = 0;
        float currentYPosition = 0f;

        // Create a parent GameObject for the stack
        GameObject stackParent = new GameObject("Stack");
        stackParent.transform.parent = transform;
        stackParent.transform.position = transform.position;
        // Attach TileStackController script to the Stack parent
        TileStackController stackController = stackParent.AddComponent<TileStackController>();

        // Create a stack to hold the names of the prefab colors
        Stack<string> prefabNamesStack = new Stack<string>();

        maxPrefabCount = Random.Range(1, 10);
        numberOfColors = Random.Range(1, prefabs.Length);
        while (numberOfColors > 0)
        {
            GameObject ColorParent = new GameObject("ColorParent");
            ColorParent.transform.parent = stackParent.transform;
            ColorParent.transform.position = transform.position;
            ColorParent.name = prefabs[numberOfColors].name;

            prefabNamesStack.Push(ColorParent.name); // Push the prefab name onto the stack

            numberOfPrefabsPerColor = Random.Range(1, maxPrefabCount - totalSpawned);
            while (numberOfPrefabsPerColor > 0)
            {
                // Instantiate prefabs of the current color
                GameObject prefab = Instantiate(prefabs[numberOfColors], transform.position + Vector3.up * currentYPosition, Quaternion.Euler(90f, 0f, 0f), ColorParent.transform);
                prefab.name = ColorParent.name; // Set the name of the prefab in the hierarchy
                currentYPosition += prefabSpacing; // Update Y position for next prefab

                numberOfPrefabsPerColor--;
            }
            maxPrefabCount -= totalSpawned;
            numberOfColors--;
        }

        // Assign the stack of prefab names to the TileStackController
        //stackController.SetPrefabNamesStack(prefabNamesStack);
    }

    // Fisher-Yates shuffle algorithm to shuffle array elements
    private void ShuffleArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    public bool IsPositionEmpty()
    {
        return transform.childCount == 0;
    }
}
