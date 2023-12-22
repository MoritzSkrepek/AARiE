using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public static EventManager instance;
    public static event System.Action<int[,]> OnGridUpdate;
    public static void GridUpdate(int[,] grid) => OnGridUpdate?.Invoke(grid);
}