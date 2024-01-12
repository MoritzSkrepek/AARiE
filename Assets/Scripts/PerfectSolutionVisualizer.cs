using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectSolutionVisualizer : MonoBehaviour
{
    public GameObject inventoryObject;
    public GameObject inventoryPlacementObject;
    public GameObject KnapsackAlgoObject;

    private PlaceObjectOnLookedAtDesk anchorScript;
    private KnapsackScript knapsackScript;
    private Vector3 originalInventoryPosition;
    private int[,] perfectSolution;
    private bool isClicked = false;

    public void Start()
    {
        anchorScript = inventoryPlacementObject.GetComponent<PlaceObjectOnLookedAtDesk>();
        originalInventoryPosition = anchorScript.objectPosition;
        knapsackScript = KnapsackAlgoObject.GetComponent<KnapsackScript>();
        perfectSolution = knapsackScript.usedItems;
        //printItems();
        setNewPosition();
        fillInventory();
    }

    private void printItems()
    {
        for (int row = 0; row < 3; row++)
        {
            string rowString = "";
            for (int col = 0; col < 3; col++)
            {
                rowString += perfectSolution[row, col] + " ";
            }
            Debug.Log("Row " + row + ": " + rowString);
        }
    }

    private void setNewPosition()
    {
        //old: 0.25
        Vector3 newPosition = originalInventoryPosition + Vector3.forward * 0.5f + Vector3.up * 0.205f;
        inventoryObject.transform.position = newPosition;
        Quaternion objectRotation = Quaternion.Euler(-45f, 0f, 0f);
        inventoryObject.transform.rotation = objectRotation;
        inventoryObject.SetActive(true);
    }

    private void fillInventory()
    {
        
    }
}
