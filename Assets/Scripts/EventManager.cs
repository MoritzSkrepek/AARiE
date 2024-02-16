public static class EventManager
{
    //Level 1
    public static event System.Action<int, string, string> OnMessageReceived;
    public static void ReceiveMsg(int idx, string username, string message) => OnMessageReceived?.Invoke(idx, username, message);

    public static event System.Action<int, string, string> OnMessageSend;
    public static void SendMsg(int idx, string username, string message) => OnMessageSend?.Invoke(idx, username, message);

    //Level 2
    public static event System.Action<int[,]> OnGridUpdate;
    public static void GridUpdate(int[,] grid) => OnGridUpdate?.Invoke(grid);
}