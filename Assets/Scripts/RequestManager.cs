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
                    RegisterLaptop(requestBody, context);
                }
                else if (url == "/message")
                {
                    ProcessMessage(requestBody, context.Request.UserHostAddress);
                }

                Respond(context, HttpStatusCode.OK, "OK");
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
                else
                {
                    Respond(context, HttpStatusCode.NotFound, "Not Found");
                }
            }
            else
            {
                Respond(context, HttpStatusCode.MethodNotAllowed, "Method Not Allowed");
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

    private void RegisterLaptop(string requestBody, HttpListenerContext context)
    {
        if (!string.IsNullOrEmpty(requestBody))
        {
            bool registered = false;
            for (int i = 0; i < laptops.Length; i++)
            {
                if (string.IsNullOrEmpty(laptops[i]))
                {
                    laptops[i] = requestBody;
                    registered = true;
                    break;
                }
            }

            if (!registered)
            {
                Respond(context, HttpStatusCode.BadRequest, "Maximum number of laptops reached.");
            }
        }
        else
        {
            Respond(context, HttpStatusCode.BadRequest, "Request body is empty.");
        }
    }

    private void ProcessMessage(string requestBody, string senderIpAddress)
    {
        if (!string.IsNullOrEmpty(requestBody))
        {
            MessageData messageData = JsonUtility.FromJson<MessageData>(requestBody);
            string username = messageData.username;
            string message = messageData.message;

            lock (laptops)
            {
                for (int i = 0; i < laptops.Length; i++)
                {
                    if (laptops[i] == senderIpAddress)
                    {
                        Debug.Log($"Received message from {i}: {username} - {message}");

                        lock (messageDataLock)
                        {
                            messageDataList.Add(new MessageData { username = username, message = message });
                        }

                        break;
                    }
                }
            }
        }
    }

    private void addMessage(int idx, string username, string message)
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
        if (context.Request.HttpMethod == "OPTIONS")
        {
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
            context.Response.AddHeader("Access-Control-Max-Age", "1728000");
        }
        context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
    }
}

[Serializable]
public class MessageData
{
    public string username;
    public string message;
}
