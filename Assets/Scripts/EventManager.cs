public static class EventManager
{
    //Level 1
    public static event System.Action<string, string> OnMessageReceived;
    public static void ReceiveMsg(string username, string message) => OnMessageReceived?.Invoke(username, message);

    public static event System.Action<string, string> OnMessageSend;
    public static void SendMsg(string username, string message) => OnMessageSend?.Invoke(username, message);

    //Level 2
    public static event System.Action<int[,]> OnGridUpdate;
    public static void GridUpdate(int[,] grid) => OnGridUpdate?.Invoke(grid);
}