using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ServerPlusUDP : MonoBehaviour
{
    Socket serverSocket;
    Dictionary<EndPoint, string> connectedUsers = new Dictionary<EndPoint, string>();

    List<(TextMeshProUGUI, string)> messageQueue = new List<(TextMeshProUGUI, string)>();
    bool queueEmpty = true;

    [Header("Console variables")]
    [SerializeField] TextMeshProUGUI consoleText;
    [SerializeField] TMP_InputField servernameInputField;
    [SerializeField] TMP_InputField UsernameInputField;

    [Header("Chat variables")]
    [SerializeField] TMP_InputField chatInputField;
    [SerializeField] TextMeshProUGUI chatText;

    void Start()
    {
        //Add function when submitting text to he chat input field.
        chatInputField.onSubmit.AddListener((string inputMsg) =>
        {
            if (inputMsg == string.Empty) return;
            BroadCastMessage(new MessageData(MessageType.Message, $"{UsernameInputField.text}: {inputMsg}"));
            chatInputField.text = null;
        });
    }

    void Update()
    {
        //Add messages from the main thread which are received from the other threads to a texbox.
        if (queueEmpty) { return; }

        foreach ((TextMeshProUGUI, string) message in messageQueue)
        {
            ChatUtilities.AddMessageToTextBox(message.Item1, message.Item2);
        }

        if (messageQueue.Count > 0) { messageQueue.Clear(); queueEmpty = true; }
    }

    public void startServer()
    {
        ChatUtilities.AddMessageToTextBox(consoleText, $"Starting the \"{servernameInputField.text}\" Server...");

        //Create and bind socket for the server.
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

        serverSocket.Bind(ipep);

        //Start a new thread to receive messages.
        Thread mainThread = new Thread(Receive);
        mainThread.Start();
    }
 
    void Receive()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        byte[] data = new byte[1024];
        int recv;

        //Read the message type and call the respective functions.
        while (true)
        {
            recv = serverSocket.ReceiveFrom(data, ref Remote);

            if (recv == 0) { continue; }

            MessageData myMessage = ChatUtilities.DecodeData(data);

            switch (myMessage._type)
            {
                case MessageType.Connect:
                    CheckNewConnection(Remote, myMessage._message);
                    break;
                case MessageType.Disconnect:
                    DisconnectUser(Remote);
                    break;
                case MessageType.Message:
                    BroadCastMessage(new MessageData(MessageType.Message, $"{connectedUsers[Remote]}: {myMessage._message}"));
                    break;
                default:
                    ChatUtilities.AddMessageToQueue(messageQueue, ref queueEmpty, consoleText, "[Server]: There was an error reading the incoming message type.");
                    break;
            }
        }
    }

    bool CheckNewConnection(EndPoint ep, string username)
    {
        if (connectedUsers.ContainsKey(ep)) { return false; }

        connectedUsers.Add(ep, username);

        BroadCastMessage(new MessageData(MessageType.Connect, $"{connectedUsers[ep]} has joined {servernameInputField.text}."));

        return true;
    }

    void DisconnectUser(EndPoint ep)
    {
        BroadCastMessage(new MessageData(MessageType.Disconnect, $"{connectedUsers[ep]} left the server."));

        connectedUsers.Remove(ep);
    }

    void BroadCastMessage(MessageData message)
    {
        //Add received messages to the message queue, so they can be printed from the main thread.
        if (message._type == MessageType.Message) { ChatUtilities.AddMessageToQueue(messageQueue, ref queueEmpty, chatText, message._message); }
        else { ChatUtilities.AddMessageToQueue(messageQueue, ref queueEmpty, consoleText, message._message); }
        
        byte[] data = ChatUtilities.EncodeData(message);

        //Send the received message to all of the connected clients.
        foreach (var connectedUser in connectedUsers)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(GetIP(connectedUser.Key)), GetPort(connectedUser.Key));
            serverSocket.SendTo(data, data.Length, SocketFlags.None, ipep);
        }
    }

    string GetIP(EndPoint ipep)
    {
        string[] subStrings = ipep.ToString().Split(':');
        return subStrings[0];
    }
    int GetPort(EndPoint ipep)
    {
        string[] subStrings = ipep.ToString().Split(':');
        return int.Parse(subStrings[1]);
    }
}
