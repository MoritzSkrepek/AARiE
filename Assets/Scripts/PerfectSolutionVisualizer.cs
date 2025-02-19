using QRTracking;
using UnityEngine;

public class PerfectSolutionVisualizer : MonoBehaviour
{
    public GameObject perfectSolutionInventory;
    public GameObject inventoryPlacementController;
    public GameObject KnapsackSolver;
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
            anchorScript = inventoryPlacementController.GetComponent<InventoryPlacementController>();
            originalInventoryPosition = anchorScript.objectPosition;
            knapsackSolver = KnapsackSolver.GetComponent<KnapsackSolver>();
            perfectSolution = knapsackSolver.usedItems;
            setNewPosition();
            perfectSolutionInventory.SetActive(isClicked);
            fillInventory();
        }
        else
        {
            perfectSolutionInventory.SetActive(isClicked);
        }
    }

    private void setNewPosition()
    {
        Vector3 newPosition = originalInventoryPosition + Vector3.forward * 0.395f + Vector3.up * 0.16f;
        perfectSolutionInventory.transform.position = newPosition;
        Quaternion objectRotation = Quaternion.Euler(-45f, 0f, 0f);
        perfectSolutionInventory.transform.rotation = objectRotation;
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
