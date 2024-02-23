using QRTracking;
using UnityEngine;

public class PerfectSolutionVisualizer : MonoBehaviour
{
    public GameObject inventoryObject;
    public GameObject inventoryPlacementObject;
    public GameObject KnapsackAlgoObject;
    public Transform inventory;

    private InventoryPlacementController anchorScript;
    private KnapsackSolver knapsackSolver;
    private Vector3 originalInventoryPosition;
    private int[,] perfectSolution;
    private bool isClicked = false;
    private int numRows = 3;
    private int numColumns = 3;

    public void Start()
    {
        isClicked = !isClicked;
        if (isClicked)
        {
            anchorScript = inventoryPlacementObject.GetComponent<InventoryPlacementController>();
            originalInventoryPosition = anchorScript.objectPosition;
            knapsackSolver = KnapsackAlgoObject.GetComponent<KnapsackSolver>();
            perfectSolution = knapsackSolver.usedItems;
            setNewPosition();
            inventoryObject.SetActive(isClicked);
            fillInventory();
        }
        else
        {
            inventoryObject.SetActive(isClicked);
        }
    }

    private void setNewPosition()
    {
        //alt: 0.45, 0.205
        //neu: 0.3, 0.19
        Vector3 newPosition = originalInventoryPosition + Vector3.forward * 0.395f + Vector3.up * 0.16f;
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
                if (perfectSolution[i, j] == 0)
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
