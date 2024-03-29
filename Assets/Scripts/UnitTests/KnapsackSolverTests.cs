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
    public int KnapsackMaxValue_Item_ReturnsExpectedValue(
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

    [Test]
    public void KnapsackPerformanceReportItemRange()
    {
        // Überschrift für den Leistungsbericht ausgeben
        UnityEngine.Debug.Log("Item Count | Elapsed Time (ms)");
        UnityEngine.Debug.Log("-----------------------------");

        // Schleife durch verschiedene Gegenstandanzahlen
        for (int itemCount = 50; itemCount <= 1000; itemCount += 50)
        {
            // Oberen Grenzwert für das Gegenstandgewicht und die Kapazität basierend auf der Gegenstandanzahl berechnen
            int weightUpperBound = itemCount * 2;
            int capacity = itemCount * 5;

            // Zufällige Gegenstände generieren
            var items = GenerateRandomItems(itemCount, weightUpperBound);

            // Gegenstände und Kapazität für den Rucksacklöser festlegen
            knapsackSolver.items = items;
            knapsackSolver.capacity = capacity;

            // Stoppuhr starten, um die verstrichene Zeit zu messen
            var stopwatch = Stopwatch.StartNew();

            // Die zu testende Methode aufrufen
            knapsackSolver.KnapsackMaxValue(out _);

            // Stoppuhr anhalten nach dem Methodenaufruf
            stopwatch.Stop();

            // Anzahl der Gegenstände und verstrichene Zeit für diese Iteration ausgeben
            UnityEngine.Debug.Log($"{itemCount,-10} | {stopwatch.ElapsedMilliseconds,-15}");
        }
    }

    [Test]
    public void KnapsackPerformanceReportRangeIteration()
    {
        // Überschrift für den Leistungsbericht ausgeben
        UnityEngine.Debug.Log("Iteration | Elapsed Time (ms)");
        UnityEngine.Debug.Log("-----------------------------");

        // Konstante für die Anzahl der Gegenstände
        int itemCount = 20;

        // Oberen Grenzwert für das Gegenstandgewicht basierend auf der Gegenstandanzahl berechnen
        int weightUpperBound = itemCount * 2;

        // Schleife für 1000 Mal wiederholen
        for (int i = 1; i <= 500; i++)
        {
            // Oberen Grenzwert für die Kapazität basierend auf der Gegenstandanzahl berechnen
            int capacity = itemCount * 5;

            // Zufällige Gegenstände generieren (festgelegte Anzahl von Gegenständen)
            var items = GenerateRandomItems(itemCount, weightUpperBound);

            // Gegenstände und Kapazität für den Rucksacklöser festlegen
            knapsackSolver.items = items;
            knapsackSolver.capacity = capacity;

            // Stoppuhr starten, um die verstrichene Zeit zu messen
            var stopwatch = Stopwatch.StartNew();

            // Die zu testende Methode aufrufen
            knapsackSolver.KnapsackMaxValue(out _);

            // Stoppuhr anhalten nach dem Methodenaufruf
            stopwatch.Stop();

            // Durchlaufnummer und verstrichene Zeit für diese Iteration ausgeben
            UnityEngine.Debug.Log($"{i,-9} | {stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000.0 * 1000.0)):F2} µs");
        }
    }

    private Dictionary<int, QRData> GenerateRandomItems(int count, int weightUpperBound)
    {
        var items = new Dictionary<int, QRData>();

        for (int i = 1; i <= count; i++)
        {
            int weight = UnityEngine.Random.Range(1, weightUpperBound);
            int value = UnityEngine.Random.Range(1, 100);
            items.Add(i, new QRData { id = i, weight = weight, value = value });
        }

        return items;
    }
}
