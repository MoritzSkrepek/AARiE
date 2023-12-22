using QRTracking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject QRCodeManager;
    public GameObject knapsackSolverGameObject;

    private SortedDictionary<System.Guid, GameObject> activeQRObjects;

    private GameObject inventoryObject;
    public float verticalOffset = 0.1f;
    private Bounds inventoryBounds;
    private int numRows = 3;
    private int numColumns = 3;
    private int[,] idGrid;
    private int cap = 120;
    private int currWeight = 0;

    void Start()
    {
        activeQRObjects = QRCodeManager.GetComponent<QRCodesVisualizer>().qrCodesObjectsList;
        
        UpdateInventoryBounds();
        InitializeIDGrid();
    }

    public void SetInventoryObject(GameObject obj)
    {
        inventoryObject = obj;
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
                    Debug.Log("Item " + qRCode.item.qrData.id + " is in the inventory.");
                    Vector2 startGridPosition = CalculateGridPosition(worldPosition);
                    idGrid[(int)startGridPosition.x, (int)startGridPosition.y] = qRCode.item.qrData.id;

                    EventManager.GridUpdate(idGrid);
                }
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
    }

    Vector2 CalculateGridPosition(Vector3 objectPosition)
    {
        float cellWidth = inventoryBounds.size.x / numColumns;
        float cellHeight = inventoryBounds.size.z / numRows;
        int col = Mathf.FloorToInt((objectPosition.x - inventoryBounds.min.x) / cellWidth);
        int row = Mathf.FloorToInt((inventoryBounds.max.z - objectPosition.z) / cellHeight);

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
        return renderer != null ? renderer.bounds : new Bounds(obj.transform.position, Vector3.one);
    }

    void ExtendBounds(ref Bounds bounds, float offset)
    {
        bounds.center = new Vector3(bounds.center.x, bounds.center.y + offset / 2, bounds.center.z);
        bounds.extents = new Vector3(bounds.extents.x, bounds.extents.y + offset / 2, bounds.extents.z);
    }
}
