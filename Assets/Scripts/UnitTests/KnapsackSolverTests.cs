using NUnit.Framework;
using static QRTracking.QRItem;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;

[TestFixture]
public class KnapsackSolverTests
{
    private KnapsackSolver knapsackSolver;

    [SetUp]
    public void SetUp()
    {
        GameObject solverObject = new GameObject();
        knapsackSolver = solverObject.AddComponent<KnapsackSolver>();
    }

    [Test]
    public void KnapsackMaxValue_NoItems_ReturnsZero()
    {
        // Arrange
        knapsackSolver.items = new Dictionary<int, QRData>();
        knapsackSolver.capacity = 100;

        // Act
        int maxValue = knapsackSolver.KnapsackMaxValue(out int[,] usedItems);

        // Assert
        Assert.AreEqual(0, maxValue, "The maximum value should be zero when there are no items.");
    }

    [TestCase(20, 50, 30, ExpectedResult = 50,
        TestName = "Single item within capacity returns expected value.")]
    [TestCase(30, 50, 20, ExpectedResult = 0,
        TestName = "Single item exceeding capacity returns zero value.")]
    [TestCase(30, 50, 30, ExpectedResult = 50,
        TestName = "Single item exact capacity returns expected value.")]
    public int KnapsackMaxValue_SingleItem_ReturnsExpectedValue(
        int weight, int value, int capacity)
    {
        // Arrange
        var item = new QRData { id = 1, weight = weight, value = value };
        knapsackSolver.items = new Dictionary<int, QRData> { { 1, item } };
        knapsackSolver.capacity = capacity;

        // Act
        return knapsackSolver.KnapsackMaxValue(out _);
    }

    [TestCase(10, ExpectedResult = 0,
        TestName = "All items exceeding capacity return zero value.")]
    [TestCase(90, ExpectedResult = 220,
        TestName = "All items within capacity return expected value.")]
    [TestCase(70, ExpectedResult = 170,
        TestName = "Multiple items within capacity, return optimal selection.")]
    public int KnapsackMaxValue_MultipleItems_ReturnsExpectedValue(
        int capacity)
    {
        // Arrange
        var items = new Dictionary<int, QRData>
        {
            { 1, new QRData { id = 1, weight = 20, value = 50 } },
            { 2, new QRData { id = 2, weight = 30, value = 70 } },
            { 3, new QRData { id = 3, weight = 40, value = 100 } }
        };
        knapsackSolver.items = items;
        knapsackSolver.capacity = capacity;

        // Act
        return knapsackSolver.KnapsackMaxValue(out _);
    }

    [TestCase(10, 20, 100, TestName = "Performance test with small data set.")]
    [TestCase(100, 100, 1000, TestName = "Performance test with moderate data set.")]
    [TestCase(1000, 2000, 10000, TestName = "Performance test with large data set.")]
    public void KnapsackMaxValue_PerformanceTest(int itemCount, int weightUpperBound, int capacity)
    {
        // Arrange
        var random = new System.Random();
        var items = GenerateRandomItems(itemCount, weightUpperBound);

        knapsackSolver.items = items;
        knapsackSolver.capacity = capacity;

        // Act
        var stopwatch = Stopwatch.StartNew();
        knapsackSolver.KnapsackMaxValue(out _);
        stopwatch.Stop();

        // Assert
        Assert.Less(stopwatch.ElapsedMilliseconds, 100, $"Time taken for KnapsackMaxValue with {itemCount} items should be less than 100 ms");
    }

    private Dictionary<int, QRData> GenerateRandomItems(int count, int weightUpperBound)
    {
        var items = new Dictionary<int, QRData>();
        var random = new System.Random();

        for (int i = 1; i <= count; i++)
        {
            int weight = random.Next(1, weightUpperBound + 1);
            int value = random.Next(1, 101);
            items.Add(i, new QRData { id = i, weight = weight, value = value });
        }

        return items;
    }
}
