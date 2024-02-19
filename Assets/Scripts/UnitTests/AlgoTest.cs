using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using QRTracking;
using UnityEngine;
using UnityEngine.TestTools;
using static QRTracking.QRItem;

public class AlgoTest
{
    GameObject testObject;
    KnapsackSolver knapsackScript;
    int[,] usedItems;
    int capacity = 120;

    [SetUp]
    public void Setup()
    {

        // Create an instance of KnapsackSolver before each test
        testObject = new GameObject();
        knapsackScript = testObject.AddComponent<KnapsackSolver>();
    }

    [TearDown]
    public void Teardown()
    {
        // Destroy the test GameObject after each test
        GameObject.DestroyImmediate(testObject);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void KnapsackMaxValue_ReturnsCorrectValue()
    {

        Dictionary<int, QRData> items = new Dictionary<int, QRData>() {
            {1, new QRData { id = 1, weight = 50, value = 100 }},
            {2, new QRData { id = 2, weight = 25, value = 50 }},
            {3, new QRData { id = 3, weight = 20, value = 30 }},
            {4, new QRData { id = 4, weight = 10, value = 15 }},
            {5, new QRData { id = 5, weight = 10, value = 5 }},
            {6, new QRData { id = 6, weight = 25, value = 50 }},
            {7, new QRData { id = 7, weight = 10, value = 10 }},
            {8, new QRData { id = 8, weight = 5, value = 50 }},
            {9, new QRData { id = 9, weight = 5, value = 15 }},
            {10, new QRData { id = 10, weight = 20, value = 15 }},
        };

        System.Random random = new System.Random();
        for (int i = 11; i <= 100; i++)
        {
            int randomWeight = random.Next(1, 51);
            int randomValue = random.Next(1, 101);

            items.Add(i, new QRData { id = i, weight = randomWeight, value = randomValue });
        }

        knapsackScript.capacity = capacity;
        knapsackScript.maxItems = items.Count;
        knapsackScript.items = items;

        System.Diagnostics.Stopwatch knapsackMaxValueStopwatch = System.Diagnostics.Stopwatch.StartNew();
        int maxValue = knapsackScript.KnapsackMaxValue(out usedItems);
        knapsackMaxValueStopwatch.Stop();
        Debug.Log($"Time taken by KnapsackMaxValue: {knapsackMaxValueStopwatch.ElapsedMilliseconds} ms");

        System.Diagnostics.Stopwatch recursiveStopwatch = System.Diagnostics.Stopwatch.StartNew();
        int maxValueComp = KnapsackMaxValueRecursive(items.Count, capacity, items, new Dictionary<(int, int), int>());
        recursiveStopwatch.Stop();
        Debug.Log($"Time taken by KnapsackMaxValueRecursive: {recursiveStopwatch.ElapsedMilliseconds} ms");

        Assert.AreEqual(maxValueComp, maxValue);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode, you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator AlgoTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }

    public int KnapsackMaxValueRecursive(int n, int remainingCapacity, Dictionary<int, QRData> items, Dictionary<(int, int), int> memo)
    {
        if (n == 0 || remainingCapacity == 0)
            return 0;

        if (memo.TryGetValue((n, remainingCapacity), out int memoizedValue))
            return memoizedValue;

        if (items[n].weight > remainingCapacity)
        {
            memo[(n, remainingCapacity)] = KnapsackMaxValueRecursive(n - 1, remainingCapacity, items, memo);
            return memo[(n, remainingCapacity)];
        }

        int includedValue = items[n].value + KnapsackMaxValueRecursive(n - 1, remainingCapacity - items[n].weight, items, memo);
        int excludedValue = KnapsackMaxValueRecursive(n - 1, remainingCapacity, items, memo);

        int result = Math.Max(includedValue, excludedValue);

        memo[(n, remainingCapacity)] = result;

        return result;
    }
}
