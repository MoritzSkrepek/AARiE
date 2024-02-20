using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RequestManager : MonoBehaviour
{
    private HttpListener listener;
    private string[] laptops = new string[2];
    private int port = 9090;
    private List<MessageData> messageDataList = new List<MessageData>();
    private readonly object messageDataLock = new object();

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
        EventManager.OnMessageSend += addMessage;
        while (true)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error handling request: {e.Message}");
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        string requestMethod = context.Request.HttpMethod;
        string url = context.Request.Url.AbsolutePath;

        if (requestMethod == "OPTIONS")
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            context.Response.Close();
            return;
        }

        if (requestMethod == "POST")
        {
            if (url == "/message")
            {
                string requestBody;
                using (StreamReader reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = reader.ReadToEnd();
                }

                ProcessMessage(requestBody);
                Respond(context, HttpStatusCode.OK, "OK");
            }
            else
            {
                Respond(context, HttpStatusCode.NotFound, "Not Found");
            }
        }
        else if (requestMethod == "GET")
        {
            if (url == "/get-messages")
            {
                lock (messageDataLock)
                {
                    string jsonResponse = JsonUtility.ToJson(messageDataList);
                    Respond(context, HttpStatusCode.OK, jsonResponse);
                }
            }
            else if (url == "/connect")
            {
                Respond(context, HttpStatusCode.OK, "OK");
            }
            else
            {
                Respond(context, HttpStatusCode.NotFound, "Not Found");
            }
        }
        else
        {
            Respond(context, HttpStatusCode.MethodNotAllowed, "Method Not Allowed");
        }

        context.Response.Close();
    }

    private void ProcessMessage(string requestBody)
    {
        if (!string.IsNullOrEmpty(requestBody))
        {
            MessageData messageData = JsonUtility.FromJson<MessageData>(requestBody);

            EventManager.ReceiveMsg(messageData.username, messageData.message);
            Debug.Log($"Received message: {messageData.username} - {messageData.message}");
        }
    }

    private void addMessage(string username, string message)
    {
        lock (messageDataLock)
        {
            messageDataList.Add(new MessageData { username = username, message = message });
        }
    }

    private void Respond(HttpListenerContext context, HttpStatusCode statusCode, string responseMessage)
    {
        byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseMessage);
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "text/plain";
        context.Response.ContentLength64 = responseBytes.Length;
        context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);


        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        context.Response.Close();
    }

}

[Serializable]
public class MessageData
{
    public string username;
    public string message;
}
