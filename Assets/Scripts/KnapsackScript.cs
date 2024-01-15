using QRTracking;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static QRTracking.QRItem;

public class KnapsackScript : MonoBehaviour
{
    public GameObject QRCodeManager;
    public TextMeshPro ownMesh;
    public TextMeshPro maxMesh;
    public TextMeshPro infoMesh;

    public int[,] usedItems;
    public int capacity = 120;
    
    private Dictionary<int, QRData> items;
    private int maxItems = 9;
    private int[,] inventory;

    void Start()
    {
        items = new QRItem(0).items;
        EventManager.OnGridUpdate += SetInventory;
    }

    void CalculateKnapsack()
    {
        int maxValue = KnapsackMaxValue(out usedItems);
        int inventoryValue = -1;
        maxMesh.text = "Maximal erreichbarer Wert: " + maxValue.ToString();
        try
        {
            inventoryValue = KnapsackInventoryValue(inventory);
            if (maxValue == inventoryValue)
            {
                infoMesh.color = Color.green;
                infoMesh.text = "Maximale Punktzahl erreicht";
            }
            else
            {
                infoMesh.text = "";
            }
            ownMesh.text = "Erreichter Wert: " + inventoryValue.ToString();

        } 
        catch (Exception e)
        {
            Debug.LogError("Error calculating inventory value: " + e.Message);
        }
    }

    //New Version!!
    int KnapsackMaxValue(out int[,] usedItems)
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
                else if (i <= maxItems && items[i].weight <= w)
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
        int[,] tempUsedItems = new int[3, 3];
        int row = n;
        int col = capacity;
        int rowIndex = 0;
        int colIndex = 0;

        while (row > 0 && col > 0 && rowIndex < 3 && colIndex < 3)
        {
            if (selected[row, col] && colIndex < maxItems)
            {
                tempUsedItems[rowIndex, colIndex] = items[row].id;
                col -= items[row].weight;
                row--;

                colIndex++;
                if (colIndex >= 3)
                {
                    colIndex = 0;
                    rowIndex++;
                }
            }
            else
            {
                row--;
            }
        }

        usedItems = tempUsedItems;

        return dp[n, capacity];
    }



    /* Old Version!!
    int KnapsackMaxValue(out int[,] usedItems)
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

        return dp[n, capacity];
    }
    */

    int KnapsackInventoryValue(int[,] inventory)
    {
        if(inventory == null)
        {
            throw new System.Exception("Inventory is null");
        }

        int totalValue = 0;

        foreach (var item in items.Values)
        {
            int itemId = item.id;
            int itemValue = item.value;

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
        inventory = newInventory;
        CalculateKnapsack();
    }

    public void UpdateInfoMesh(string input)
    {
        infoMesh.color = Color.red;
        infoMesh.text = input;
    }
}
