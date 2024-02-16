using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

public class RequestManager : MonoBehaviour
{
    private HttpListener listener;
    private string[] laptops = new string[2];
    private int port = 9090;

    private void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add($"http://*:{port}/");
        listener.Start();

        Thread listenerThread = new Thread(Listen);
        listenerThread.Start();
    }

    private void Listen()
    {
        while (true)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                Task.Run(() => HandleRequest(context));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error handling request: {e.Message}");
            }
        }
    }

    private async void HandleRequest(HttpListenerContext context)
    {
        try
        {
            string requestMethod = context.Request.HttpMethod;
            string url = context.Request.Url.AbsolutePath;

            if (requestMethod == "POST")
            {
                string requestBody;
                using (StreamReader reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                if (url == "/register")
                {
                    RegisterLaptop(requestBody);
                }
                else if (url == "/message")
                {
                    ProcessMessage(requestBody, context.Request.UserHostAddress);
                }

                Respond(context, HttpStatusCode.OK, "OK");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error handling request: {e.Message}");
            Respond(context, HttpStatusCode.InternalServerError, "Internal Server Error");
        }
        finally
        {
            context.Response.Close();
        }
    }

    private void RegisterLaptop(string requestBody)
    {
        if (!string.IsNullOrEmpty(requestBody))
        {
            for (int i = 0; i < laptops.Length; i++)
            {
                if (string.IsNullOrEmpty(laptops[i]))
                {
                    laptops[i] = requestBody;
                    break;
                }
            }
        }
    }

    private void ProcessMessage(string requestBody, string senderIpAddress)
    {
        if (!string.IsNullOrEmpty(requestBody))
        {
            MessageData messageData = JsonUtility.FromJson<MessageData>(requestBody);
            string username = messageData.username;
            string message = messageData.message;

            for (int i = 0; i < laptops.Length; i++)
            {
                if (laptops[i] == senderIpAddress)
                {
                    Debug.Log($"Received message from {i}: {username} - {message}");

                    for (int j = 0; j < laptops.Length; j++)
                    {
                        if (laptops[j] != senderIpAddress && !string.IsNullOrEmpty(laptops[j]))
                        {
                            SendMsg(j, username, message);
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }

    private void Respond(HttpListenerContext context, HttpStatusCode statusCode, string responseMessage)
    {
        byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseMessage);
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "text/plain";
        context.Response.ContentLength64 = responseBytes.Length;
        context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        if (context.Request.HttpMethod == "OPTIONS")
        {
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
            context.Response.AddHeader("Access-Control-Max-Age", "1728000");
        }
        context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
    }

    public void SendMsg(int idx, string username, string message)
    {
        StartCoroutine(SendMessage(idx, username, message));
    }

    private IEnumerator SendMessage(int idx, string username, string message)
    {
        string jsonPayload = JsonUtility.ToJson(new MessageData { username = username, message = message });

        string ipAddress = laptops[idx];
        string url = $"http://{ipAddress}:9090/message";

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, jsonPayload))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error sending message: {request.error}");
            }
        }
    }


    private string GetIp()
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

    public void StopListener()
    {
        listener.Stop();
    }
}

[Serializable]
public class MessageData
{
    public string username;
    public string message;
}
