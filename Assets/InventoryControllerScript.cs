using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject testObject;
    public GameObject knapsackSolverGameObject;

    // Inventory Object parameters
    private GameObject inventoryObject;
    private float verticalOffset = 0.05f; // Adjust this offset as needed
    private Bounds inventoryBounds;
    private bool isTestObjectInsideBounds = false;

    // Grid parameters for a 7x7 grid
    private int numRows = 7; // Adjust as needed
    private int numColumns = 7; // Adjust as needed
    private int[,] idGrid; // 2D array to store IDs

    // QR-Code parameters
    private int qrID = 1; //Replace with function to get id from qr-code

    void Start()
    {
        // Initialize inventory bounds and grid once at the start
        UpdateInventoryBounds();
        InitializeIDGrid();
    }

    public void SetInventoryObject(GameObject obj)
    {
        inventoryObject = obj;
    }

    void Update()
    {
        CheckObjectInBounds();
        // If the object is inside bounds and can fit within the grid, calculate its grid position
        if (isTestObjectInsideBounds)
        {
            // get QR ID from QR-Code
            // Calculate the starting grid position
            Vector2 startGridPosition = CalculateGridPosition(testObject.transform.position);
            // Set the ID directly in the idGrid array
            idGrid[(int)startGridPosition.x, (int)startGridPosition.y] = qrID;
            // This part will be deleted
            testObject.GetComponent<Renderer>().material.color = Color.green;
            // This part will be deleted
            // Optionally, print the entire grid to the console
            KnapsackScript knapsackScript = knapsackSolverGameObject.GetComponent<KnapsackScript>();
            if (knapsackScript != null)
            {
                knapsackScript.SetInventory(idGrid);
            }
            PrintGrid();
        }
        else
        {
            // Object doesn't fit within bounds, handle accordingly (e.g., display a message)
            Debug.Log("Object doesn't fit within bounds.");
            // This part will be deleted
            testObject.GetComponent<Renderer>().material.color = Color.red;
            // This part will be deleted
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
        col = Mathf.Clamp(col, 0, numColumns - 1);
        // Adjust the row index to ensure it's within bounds and consider the object's height
        row = Mathf.Clamp(row, 0, numRows - 1 - Mathf.FloorToInt((GetBounds(testObject).size.z / cellHeight)));
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

    void CheckObjectInBounds()
    {
        if (testObject != null && inventoryObject != null)
        {
            // Check if the object is inside the inventory bounds
            // replace testObject.transform.position with qr code positioin
            isTestObjectInsideBounds = inventoryBounds.Contains(testObject.transform.position);
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
