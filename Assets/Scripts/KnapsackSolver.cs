using QRTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static QRTracking.QRItem;

public class KnapsackSolver : MonoBehaviour
{
    public GameObject QRCodeManager;
    public TextMeshPro ownMesh;
    public TextMeshPro maxMesh;
    public TextMeshPro infoMesh;

    public int[,] usedItems;
    public int capacity = 120;
    public Dictionary<int, QRData> items;
    public int maxItems = 9;
    
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
                UpdateInfoMesh("Optimale Lösung gefunden", Color.green);
            }
            else
            {
                infoMesh.text = "";
            }
            ownMesh.text = inventoryValue.ToString();

        }
        catch (Exception e)
        {
            Debug.LogError("Error calculating inventory value: " + e.Message);
        }
    }

    public int KnapsackMaxValue(out int[,] usedItems)
    {
        int n = items.Count;
        int[,] dp = new int[n + 1, capacity + 1];
        bool[,] selected = new bool[n + 1, capacity + 1];
        var itemsList = new List<QRData>(items.Values);
        for (int i = 0; i <= n; i++)
        {
            for (int w = 0; w <= capacity; w++)
            {
                if (i == 0 || w == 0)
                    dp[i, w] = 0;
                else if (i <= n && itemsList[i - 1].weight <= w)
                {
                    int newValue = itemsList[i - 1].value + dp[i - 1, w - itemsList[i - 1].weight];
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
        // Backtracking
        List<int> tempUsedItems = new List<int>();
        int row = n;
        int col = capacity;
        while (row > 0 && col > 0)
        {
            if (selected[row, col])
            {
                tempUsedItems.Add(itemsList[row - 1].id);
                col -= itemsList[row - 1].weight;
            }
            row--;
        }
        //If solution contains more than maxItems remove the ones with the worst value/weight ratio
        if (tempUsedItems.Count > maxItems)
        {
            // Sort items by weight/value ratio and remove the worst ones
            tempUsedItems = tempUsedItems
                .Select(id => items[id])
                .OrderBy(item => (double)item.weight / item.value)
                .Take(maxItems)
                .Select(item => item.id)
                .ToList();
        }
        usedItems = new int[3, 3];
        int index = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (index < tempUsedItems.Count)
                {
                    usedItems[i, j] = tempUsedItems[index];
                    index++;
                }
                else
                {
                    usedItems[i, j] = 0;
                }
            }
        }
        return dp[n, capacity];
    }

    public int KnapsackInventoryValue(int[,] inventory)
    {
        if (inventory == null)
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

    public void UpdateInfoMesh(string input, Color color)
    {
        infoMesh.color = color;
        infoMesh.text = input;
    }
}
