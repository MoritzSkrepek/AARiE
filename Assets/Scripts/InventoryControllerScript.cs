using QRTracking;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject QRCodeManager;
    public GameObject knapsackSolverGameObject;

    private ConcurrentDictionary<int, QRItem> activeQRObjects;

    private List<QRItem> activeItems = new List<QRItem>();

    // Inventory Object parameters
    private GameObject inventoryObject;
    public float verticalOffset = 0.1f; // Adjust this offset as needed
    private Bounds inventoryBounds;

    // Grid parameters for a 3x3 grid
    private int numRows = 3; // Adjust as needed
    private int numColumns = 3; // Adjust as needed
    private int[,] idGrid; // 2D array to store IDs

    void Start()
    {
        // Initialize inventory bounds and grid once at the start
        activeQRObjects = QRCodeManager.GetComponent<QRCodesVisualizer>().activeQRObjects;
        
        UpdateInventoryBounds();
        InitializeIDGrid();
    }

    public void SetInventoryObject(GameObject obj)
    {
        inventoryObject = obj;
    }

    void Update()
    {
        activeQRObjects = QRCodeManager.GetComponent<QRCodesVisualizer>().activeQRObjects;
        Debug.Log("QR OBJECTS: " + activeQRObjects);
        activeItems.Clear();
        Debug.Log("Cleared");
        lock (activeQRObjects)
        {
            Debug.Log("Lock");

            var enumerator = activeQRObjects.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Debug.Log("FOHSJFSDF");
                activeItems.Add(enumerator.Current.Value);
            }
        }

        foreach (var obj in activeItems)
        {
            //HIER FEHLER FIXX BITTEEEEEEEEEEE
            Debug.Log("QR Code " + obj.qrData.id + " is outside bounds");
            Debug.Log("Is QRCode in bounds: " + inventoryBounds.Contains(obj.qrData.position));
            // If the object is inside bounds and can fit within the grid, calculate its grid position
            if (inventoryBounds.Contains(obj.qrData.position))
            {
                Debug.Log("QR Code " + obj.qrData.id + " is inside the inventory bounds");
                // get QR ID from QR-Code
                // Calculate the starting grid position
                Vector2 startGridPosition = CalculateGridPosition(obj.qrData.position);
                // Set the ID directly in the idGrid array
                idGrid[(int)startGridPosition.x, (int)startGridPosition.y] = obj.qrData.id;


                // Optionally, print the entire grid to the console
                KnapsackScript knapsackScript = knapsackSolverGameObject.GetComponent<KnapsackScript>();
                if (knapsackScript != null)
                {
                    knapsackScript.SetInventory(idGrid);
                }
                PrintGrid();
            }
            
        }
    }

    void PrintGrid()
    {
        for (int row = 0; row < numRows; row++)
        {
            string rowString = "";
            for (int col = 0; col < numColumns; col++)
            {
                rowString += idGrid[row, col] + " ";
            }
            Debug.Log("Row " + row + ": " + rowString);
        }
    }

    void InitializeIDGrid()
    {
        idGrid = new int[numRows, numColumns];
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numColumns; col++)
            {
                idGrid[row, col] = 0;
            }
        }
    }

    Vector2 CalculateGridPosition(Vector3 objectPosition)
    {
        // Find the row and column based on the object's position
        float cellWidth = inventoryBounds.size.x / numColumns;
        float cellHeight = inventoryBounds.size.z / numRows;
        int col = Mathf.FloorToInt((objectPosition.x - inventoryBounds.min.x) / cellWidth);
        int row = Mathf.FloorToInt((inventoryBounds.max.z - objectPosition.z) / cellHeight);
        // Clamp the column index to ensure it's within bounds
        //col = Mathf.Clamp(col, 0, numColumns - 1);
        // Adjust the row index to ensure it's within bounds and consider the object's height
        //row = Mathf.Clamp(row, 0, numRows - 1 - Mathf.FloorToInt((GetBounds(testObject).size.z / cellHeight)));
        return new Vector2(row, col);
    }

    void UpdateInventoryBounds()
    {
        if (inventoryObject != null)
        {
            Bounds localBounds = GetBounds(inventoryObject);
            ExtendBounds(ref localBounds, verticalOffset);
            inventoryBounds = localBounds;
        }
    }

    Bounds GetBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        return new Bounds(obj.transform.position, Vector3.one);
    }

    void ExtendBounds(ref Bounds bounds, float offset)
    {
        bounds.center = new Vector3(
            bounds.center.x,
            bounds.center.y + offset / 2,
            bounds.center.z
        );
        bounds.extents = new Vector3(
            bounds.extents.x,
            bounds.extents.y + offset / 2,
            bounds.extents.z
        );
    }
}
