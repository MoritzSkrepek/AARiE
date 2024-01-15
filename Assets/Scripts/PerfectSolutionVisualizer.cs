using QRTracking;
using UnityEngine;

public class PerfectSolutionVisualizer : MonoBehaviour
{
    public GameObject inventoryObject;
    public GameObject inventoryPlacementObject;
    public GameObject KnapsackAlgoObject;
    public Transform inventory;

    private PlaceObjectOnLookedAtDesk anchorScript;
    private KnapsackScript knapsackScript;
    private Vector3 originalInventoryPosition;
    private int[,] perfectSolution;
    private bool isClicked = false;
    private int numRows = 3;
    private int numColumns = 3;

    public void Start()
    {
        isClicked = !isClicked;
        if (isClicked == true)
        {
            anchorScript = inventoryPlacementObject.GetComponent<PlaceObjectOnLookedAtDesk>();
            originalInventoryPosition = anchorScript.objectPosition;
            knapsackScript = KnapsackAlgoObject.GetComponent<KnapsackScript>();
            perfectSolution = knapsackScript.usedItems;
            printItems();
            setNewPosition();
            inventoryObject.SetActive(true);
            fillInventory();
        }
        else
        {
            inventoryObject.SetActive(false);
        }
    }

    private void printItems()
    {
        Debug.Log("Perfekte Lösung:");
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
        Vector3 newPosition = originalInventoryPosition + Vector3.forward * 0.5f + Vector3.up * 0.205f;
        inventoryObject.transform.position = newPosition;
        Quaternion objectRotation = Quaternion.Euler(-45f, 0f, 0f);
        inventoryObject.transform.rotation = objectRotation;
    }

    private void fillInventory()
    {
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numColumns; j++)
            {
                int id = perfectSolution[i, j];
                if (perfectSolution[i,j] == 0)
                    continue;
                else
                {
                    string qrCodeName = "QRCode" + (i * numColumns + j + 1);
                    Transform qrCodeTransform = inventory.Find(qrCodeName);
                    if (qrCodeTransform != null)
                    {
                        Transform childTransform = qrCodeTransform.Find(id.ToString());
                        Debug.Log(childTransform.name);
                        if (childTransform != null) 
                        {
                            childTransform.gameObject.SetActive(true);
                        }
                        else
                        {
                            Debug.LogError($"Child with id {id} not found in QRCode {qrCodeName}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"QRCode {qrCodeName} not found in the inventory");
                    }
                }
            }
        }
    }
}
