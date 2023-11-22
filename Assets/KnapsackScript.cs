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
        public int Width;
        public int Height;
    }

    //private int[,] inventory;
    private int[,] inventory = new int[7, 7]
    { { 1, 0, 0, 0, 0, 0, 0 },
      { 2, 3, 0, 0, 0, 0, 0 },
      { 0, 0, 0, 0, 0, 0, 0 },
      { 0, 0, 0, 0, 0, 0, 0 },
      { 0, 0, 0, 8, 0, 0, 0 },
      { 0, 0, 0, 0, 0, 0, 0 },
      { 0, 0, 9, 0, 0, 0, 0 } };

    public TextMeshPro ownVal;
    public TextMeshPro maxVal;

    //Just for test purposes right now
    public Dictionary<int, Item> items = new Dictionary<int, Item>
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

    public void Start()
    {
        Debug.Log("Shalom");
        int capacity = 120;
        int maxValue = KnapsackMaxValue(items, capacity);
        Debug.Log("Maximaler Wert des Knapsacks: " + maxValue);
        maxVal.text += " " + maxValue.ToString();

        int inventoryValue = KnapsackInventoryValue(items, inventory);
        Debug.Log("Wert des Inventars: " + inventoryValue);
        ownVal.text += " " + inventoryValue.ToString();

    }

    int KnapsackMaxValue(Dictionary<int, Item> items, int capacity)
    {
        int n = items.Count;
        int[,] dp = new int[n + 1, capacity + 1];

        for (int i = 0; i <= n; i++)
        {
            for (int w = 0; w <= capacity; w++)
            {
                if (i == 0 || w == 0)
                    dp[i, w] = 0;
                else if (items[i].Weight <= w)
                    dp[i, w] = Mathf.Max(items[i].Value + dp[i - 1, w - items[i].Weight], dp[i - 1, w]);
                else
                    dp[i, w] = dp[i - 1, w];
            }
        }

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
