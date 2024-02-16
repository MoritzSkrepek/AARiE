using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RequestManager : MonoBehaviour
{
    private HttpListener listener;
    private string[] laptops = new string[2];
    private int port = 9090;

    void Start()
    {
        Debug.Log("Starting listener");
        listener = new HttpListener();
        listener.Prefixes.Add($"http://*:{this.port}/");

        EventManager.OnMessageSend += sendMsg;

        listener.Start();

        Thread listenerThread = new Thread(listen);
        listenerThread.Start();
    }

    void Update()
    {

    }

    private void listen()
    {
        Debug.Log("Listening...");
        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            Task.Run(() => HandleRequest(context));
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        Debug.Log("Handling request");
        try
        {
            string requestMethod = context.Request.HttpMethod;
            string url = context.Request.Url.AbsolutePath;

            if (requestMethod == "POST")
            {
                string requestBody;
                using (StreamReader reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = reader.ReadToEnd();
                }

                if (url == "/register")
                {
                    if (!laptops.Contains(requestBody))
                    {
                        if (laptops[0] == null)
                        {
                            laptops[0] = requestBody;
                        }
                        else if (laptops[1] == null)
                        {
                            laptops[1] = requestBody;
                        }
                        else
                        {
                            throw new Exception("Too many IP addresses registered");
                        }
                    }

                }
                else if (url == "/message")
                {
                    MessageData requestData = JsonUtility.FromJson<MessageData>(requestBody);
                    string username = requestData.username;
                    string message = requestData.message;
                    if (laptops[0] == context.Request.UserHostAddress)
                    {
                        EventManager.ReceiveMsg(0, username, message);
                        Debug.Log("Received message from 0: " + context.Request.UserHostAddress);
                    }
                    else if (laptops[1] == context.Request.UserHostAddress)
                    {
                        EventManager.ReceiveMsg(1, username, message);
                        Debug.Log("Received message from 1: " + context.Request.UserHostAddress);
                    }
                }

                string responseString = "200 OK";
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentType = "text/plain";
                context.Response.ContentLength64 = responseBytes.Length;
                context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            context.Response.Close();
        }
    }


    public void sendMsg(int idx, string username, string message)
    {
        StartCoroutine(sendMessage(idx, username, message));
    }

    private IEnumerator sendMessage(int idx, string username, string message)
    {
        string jsonPayload = "{\"username\": \"" + username + "\", \"message\": \"" + message + "\"}";

        string ipAddress = laptops[idx];
        string url = "http://" + ipAddress + ":9090/message";

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, jsonPayload))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

            request.uploadHandler = new UploadHandlerRaw(jsonBytes);

            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error sending message: " + request.error);
            }
        }
    }

    private string getIp()
    {
        string ipv4Address = "";
        string hostName = Dns.GetHostName();
        IPAddress[] addresses = Dns.GetHostAddresses(hostName);

        foreach (IPAddress address in addresses)
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ipv4Address = address.ToString();
                break;
            }
        }

        return ipv4Address;
    }

    public void stopListener()
    {
        listener.Stop();
    }
}


[Serializable]
public class MessageData
{
    public string username;
    public string message;

    public MessageData(string username, string message)
    {
        this.username = username;
        this.message = message;
    }
}