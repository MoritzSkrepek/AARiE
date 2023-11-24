using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KnapsackScript : MonoBehaviour
{
    [Serializable]
    public class Item
    {
        public int ID;
        public int Value;
        public int Weight;
    }

    public TextMeshPro ownVal;
    public TextMeshPro maxVal;
    private int[,] inventory;
    private int[,] usedItems;

    // inventory for calculation testing
    /*private int[,] inventory = new int[3, 3]
    {
        { 1, 0, 0 },
        { 2, 3, 0 },
        { 0, 0, 0 }
    };*/

    private Dictionary<int, Item> items = new Dictionary<int, Item>
    {
        {1, new Item { ID = 1, Value = 50, Weight = 25}},
        {2, new Item { ID = 2, Value = 20, Weight = 10}},
        {3, new Item { ID = 3, Value = 10, Weight = 5}},
        {4, new Item { ID = 4, Value = 25, Weight = 13}},
        {5, new Item { ID = 5, Value = 40, Weight = 20}},
        {6, new Item { ID = 6, Value = 45, Weight = 22}},
        {7, new Item { ID = 7, Value = 5, Weight = 1}},
        {8, new Item { ID = 8, Value = 100, Weight = 50}},
        {9, new Item { ID = 9, Value = 30, Weight = 6}},
        {10, new Item { ID = 10, Value = 15, Weight = 8}}
    };  

    void Start()
    {
        int capacity = 120;
        int maxValue = KnapsackMaxValue(items, capacity, out usedItems, out int coveredCapacity);
        maxVal.text += " " + maxValue.ToString();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (usedItems[i, j] != 0)
                {
                    Debug.Log(usedItems[i, j] + " ");
                }
                else
                {
                    Debug.Log(". ");
                }
            }
            Debug.Log("");
        }
        int inventoryValue = KnapsackInventoryValue(items, inventory);
        ownVal.text += " " + inventoryValue.ToString();
    }

    int KnapsackMaxValue(Dictionary<int, Item> items, int capacity, out int[,] usedItems, out int coveredCapacity)
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
                else if (items[i].Weight <= w)
                {
                    int newValue = items[i].Value + dp[i - 1, w - items[i].Weight];
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
                group.Add(items[row].ID);
                col -= items[row].Weight;
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

    int KnapsackInventoryValue(Dictionary<int, Item> items, int[,] inventory)
    {
        int totalValue = 0;

        for (int i = 1; i <= items.Count; i++)
        {
            int itemValue = items[i].Value;

            for (int j = 0; j < inventory.GetLength(0); j++)
            {
                for (int k = 0; k < inventory.GetLength(1); k++)
                {
                    if (inventory[j, k] == i)
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
    }
}
