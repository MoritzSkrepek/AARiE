using QRTracking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static QRTracking.QRItem;

public class KnapsackScript : MonoBehaviour
{
    public GameObject QRCodeManager;

    private ConcurrentDictionary<int, QRItem> activeQRObjects;
    private List<QRItem> activeItems = new List<QRItem>();

    private Dictionary<int, QRData> items = new QRItem().items;

    public int capacity = 120;
    private int maxItems = 9;

    public TextMeshPro ownVal;
    public TextMeshPro maxVal;
    //private int[,] inventory;
    private int[,] usedItems;

    private int[,] inventory = new int[3, 3]
    {
    {0, 0, 3},
    {2, 0, 0},
    {0, 0, 8}
    };


    void Start()
    {
        activeQRObjects = QRCodeManager.GetComponent<QRCodesVisualizer>().activeQRObjects;

        activeItems.Clear();
        lock (activeQRObjects)
        {
            var enumerator = activeQRObjects.GetEnumerator();

            while (enumerator.MoveNext())
            {
                activeItems.Add(enumerator.Current.Value);
            }
        }

        int maxValue = KnapsackMaxValue(out usedItems, out int coveredCapacity);
        int inventoryValue = KnapsackInventoryValue(inventory);

        maxVal.text = "Max Value: " + maxValue.ToString();
        ownVal.text = "Own Value: " + inventoryValue.ToString();
        Debug.Log(maxValue.ToString());
        Debug.Log(inventoryValue.ToString());   
    }

    int KnapsackMaxValue(out int[,] usedItems, out int coveredCapacity)
    {
        int n = items.Count;
        int[,] dp = new int[n + 1, capacity + 1];
        bool[,] selected = new bool[n + 1, capacity + 1];

        for (int i = 0; i <= n; i++)
        {
            for (int w = 0; w <= capacity; w++)
            {
                if (i == 0 || w == 0)
                    dp[i, w] = 0;
                else if (items[i].weight <= w && i <= maxItems)
                {
                    int newValue = items[i].value + dp[i - 1, w - items[i].weight];
                    if (newValue > dp[i - 1, w])
                    {
                        dp[i, w] = newValue;
                        selected[i, w] = true;
                    }
                    else
                    {
                        dp[i, w] = dp[i - 1, w];
                        selected[i, w] = false;
                    }
                }
                else
                {
                    dp[i, w] = dp[i - 1, w];
                    selected[i, w] = false;
                }
            }
        }

        // Backtrack to find selected items
        List<List<int>> tempUsedItems = new List<List<int>>();
        int row = n;
        int col = capacity;
        while (row > 0 && col > 0)
        {
            List<int> group = new List<int>();
            while (row > 0 && col > 0 && selected[row, col])
            {
                group.Add(items[row].id);
                col -= items[row].weight;
                row--;
            }

            if (group.Count > 0)
            {
                tempUsedItems.Add(group);
            }

            if (row > 0 && col > 0)
            {
                row--;
            }
        }

        // Find the maximum count manually
        int maxGroupSize = 0;
        foreach (var group in tempUsedItems)
        {
            if (group.Count > maxGroupSize)
                maxGroupSize = group.Count;
        }

        // Convert List<List<int>> to int[,]
        usedItems = new int[tempUsedItems.Count, maxGroupSize];
        for (int i = 0; i < tempUsedItems.Count; i++)
        {
            for (int j = 0; j < tempUsedItems[i].Count; j++)
            {
                usedItems[i, j] = tempUsedItems[i][j];
            }
        }

        coveredCapacity = capacity - col; // Calculate covered capacity

        return dp[n, capacity];
    }


    int KnapsackInventoryValue(int[,] inventory)
    {
        int totalValue = 0;

        foreach (var item in activeItems)
        {
            int itemId = item.qrData.id;
            int itemValue = item.qrData.value;

            for (int j = 0; j < inventory.GetLength(0); j++)
            {
                for (int k = 0; k < inventory.GetLength(1); k++)
                {
                    if (inventory[j, k] == itemId)
                    {
                        totalValue += itemValue;
                    }
                }
            }
        }

        return totalValue;
    }

    public void SetInventory(int[,] newInventory)
    {
        //inventory = newInventory;
    }
}
