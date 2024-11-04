using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClientPlusUDP : MonoBehaviour
{
    Socket clientSocket;
    IPEndPoint ipep;

    List<(TextMeshProUGUI, string)> messageQueue = new List<(TextMeshProUGUI, string)>();
    bool queueEmpty = true;

    [Header("Console variables")]
    [SerializeField] TextMeshProUGUI consoleText;
    [SerializeField] TMP_InputField IPInputField;
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
            ChatUtilities.SendMessageUDP(clientSocket, ipep, MessageType.Message, inputMsg); 
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

    public void StartClient()
    {
        Thread mainThread = new Thread(() => Connect(IPInputField.text, UsernameInputField.text));
        mainThread.Start();
    }

    void Connect(string targetAdress, string userName)
    {
        //Set the client socket and start a thread to receive messages through it.
        ipep = new IPEndPoint(IPAddress.Parse(targetAdress), 9050);
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        ChatUtilities.SendMessageUDP(clientSocket, ipep, MessageType.Connect, userName);
        
        Thread receive = new Thread(() => Receive(targetAdress));
        receive.Start();
    }

    void Receive(string broadcasterIP)
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Parse(broadcasterIP), 0);
        EndPoint Remote = (EndPoint)(sender);

        byte[] data = new byte[1024];
        int recv;

        while (true)
        {
            recv = clientSocket.ReceiveFrom(data, ref Remote);

            if (recv == 0) { continue; }

            MessageData receivedMessage = ChatUtilities.DecodeData(data);

            //Read the message type and call the respective functions.
            switch (receivedMessage._type)
            {
                case MessageType.Connect:
                case MessageType.Disconnect:
                    ChatUtilities.AddMessageToQueue(messageQueue, ref queueEmpty, consoleText, receivedMessage._message);
                    break;
                case MessageType.Message:
                    ChatUtilities.AddMessageToQueue(messageQueue, ref queueEmpty, chatText, receivedMessage._message);
                    break;
                default:
                    ChatUtilities.AddMessageToQueue(messageQueue, ref queueEmpty, consoleText, "[Client]: There was an error reading the incoming message type.");
                    break;
            }
        }    
    }

    private void OnDestroy()
    {
        if (ipep == null) { return; }
        ChatUtilities.SendMessageUDP(clientSocket, ipep, MessageType.Disconnect);
    }
}

