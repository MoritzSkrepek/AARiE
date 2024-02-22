using QRTracking;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject QRCodeManager;
    public GameObject knapsackSolverGameObject;
    public TextMeshPro userInfo;

    private SortedDictionary<System.Guid, GameObject> activeQRObjects;

    private GameObject inventoryObject;
    public float verticalOffset = 0.1f;
    private Bounds inventoryBounds;
    private int numRows = 3;
    private int numColumns = 3;
    private int[,] idGrid;
    private KnapsackSolver knapsackSolver;
    private int cap;
    private int currWeight = 0;
    private HashSet<int> processedItems;

    void Start()
    {
        userInfo.text = "Platzieren Sie nun Gegenstände im Inventar";
        activeQRObjects = QRCodeManager.GetComponent<QRCodesVisualizer>().qrCodesObjectsList;
        knapsackSolver = knapsackSolverGameObject.GetComponent<KnapsackSolver>();
        cap = knapsackSolver.capacity;
        processedItems = new HashSet<int>();
        UpdateInventoryBounds();
        InitializeIDGrid();
    }

    void Update()
    {
        UpdateGrid();
    }

    void UpdateGrid()
    {
        lock (activeQRObjects)
        {
            foreach (var item in activeQRObjects.Values)
            {
                QRCode qRCode = item.GetComponent<QRCode>();
                Vector3 worldPosition = item.transform.TransformPoint(qRCode.item.qrData.position);

                if (item != null && inventoryBounds.Contains(worldPosition))
                {
                    int itemId = qRCode.item.qrData.id;

                    if (!processedItems.Contains(itemId))
                    {
                        if (currWeight + qRCode.item.qrData.weight <= cap)
                        {
                            userInfo.text = "";
                            processedItems.Add(itemId);
                            Vector2 startGridPosition = CalculateGridPosition(worldPosition);
                            idGrid[(int)startGridPosition.x, (int)startGridPosition.y] = itemId;
                            knapsackSolver?.UpdateInfoMesh("", Color.white);
                            currWeight += qRCode.item.qrData.weight;
                            EventManager.GridUpdate(idGrid);
                        }
                        else
                        {
                            knapsackSolver?.UpdateInfoMesh("Item hat zu viel Gewicht!", Color.red);
                        }
                    }
                }
                else if (!inventoryBounds.Contains(worldPosition) && processedItems.Contains(qRCode.item.qrData.id) && ContainsId(qRCode.item.qrData.id))
                {
                    int itemId = qRCode.item.qrData.id;
                    processedItems.Remove(itemId);
                    RemoveItem(itemId);
                    currWeight -= qRCode.item.qrData.weight;
                    EventManager.GridUpdate(idGrid);
                }
            }
            PrintGrid();
        }
    }


    private void PrintGrid()
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

    private void InitializeIDGrid()
    {
        idGrid = new int[numRows, numColumns];
    }

    private Vector2 CalculateGridPosition(Vector3 objectPosition)
    {
        float cellWidth = inventoryBounds.size.x / numColumns;
        float cellHeight = inventoryBounds.size.z / numRows;
        int col = Mathf.FloorToInt((objectPosition.x - inventoryBounds.min.x) / cellWidth);
        int row = Mathf.FloorToInt((inventoryBounds.max.z - objectPosition.z) / cellHeight);
        return new Vector2(row, col);
    }

    private void UpdateInventoryBounds()
    {
        if (inventoryObject != null)
        {
            Bounds localBounds = GetBounds(inventoryObject);
            ExtendBounds(ref localBounds, verticalOffset);
            inventoryBounds = localBounds;
        }
    }

    private Bounds GetBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        return renderer != null ? renderer.bounds : new Bounds(obj.transform.position, Vector3.one);
    }

    private void ExtendBounds(ref Bounds bounds, float offset)
    {
        bounds.center = new Vector3(bounds.center.x, bounds.center.y + offset / 2, bounds.center.z);
        bounds.extents = new Vector3(bounds.extents.x, bounds.extents.y + offset / 2, bounds.extents.z);
    }

    public void SetInventoryObject(GameObject obj)
    {
        inventoryObject = obj;
    }
    private void RemoveItem(int id)
    {
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numColumns; j++)
            {
                if (idGrid[i, j] == id)
                {
                    idGrid[i, j] = 0;
                    return;
                }
            }
        }
    }

    private bool ContainsId(int id)
    {
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numColumns; j++)
            {
                if (idGrid[i, j] == id)
                {
                    return true;
                }
            }
        }
        return false;
    }
}