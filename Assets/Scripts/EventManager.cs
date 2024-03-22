public static class EventManager
{
    //Level 1
    public static event System.Action<string, string, string> OnMessageReceived;
    public static void ReceiveMsg(string username, string message, string side) => OnMessageReceived?.Invoke(username, message, side);

    public static event System.Action<string, string, string> OnMessageSend;
    public static void SendMsg(string username, string message, string side) => OnMessageSend?.Invoke(username, message, side);

    //Level 2
    public static event System.Action<int[,]> OnGridUpdate;
    public static void GridUpdate(int[,] grid) => OnGridUpdate?.Invoke(grid);
}