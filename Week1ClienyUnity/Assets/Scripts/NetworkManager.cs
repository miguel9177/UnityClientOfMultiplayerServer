using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Serializer;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public struct UdpState
    {
        public UdpClient _udpClient;
        public IPEndPoint _ipEndPoint;
    }

    //this stores the state of the udp
    static UdpState state = new UdpState();
    //this stores the player name
    public string playerName;
    //this stores the player transform
    //public PlayerInfo player;
    public List<NetworkGameObject> netObjects;

    string ipAdress = "10.1.17.235";

    // Start is called before the first frame update
    void Start()
    {
        ConnectToServer();
        RequestUIDs();

        //i need this to start the loop of sending messages to the server, this is necessary since i cant put the SendMessageAssyncCallback outside the start function or it wont work for some reason
        byte[] startMessageAssyncCallBack = Encoding.ASCII.GetBytes("");
        //start the loop of sending messages to the server
        state._udpClient.BeginSend(startMessageAssyncCallBack, startMessageAssyncCallBack.Length, new AsyncCallback(SendMessageAssyncCallback), state);

        //this starts the infinite loop void that will always be receiving the information from the server
        state._udpClient.BeginReceive(ReceiveMessageAsyncCallback, state);
        
        //this will receive all messages from the server
        void ReceiveMessageAsyncCallback(IAsyncResult result)
        {
            //this gets the packet from the server in bytes
            byte[] receiveBytes = state._udpClient.EndReceive(result, ref state._ipEndPoint);

            IfReceivedMessageIsUIDAssignThem(receiveBytes);

            //send the received message to the function that handles the received messages
            ReceivedMessageFromServer(receiveBytes);

            //this recalls this function to make it loop infinitely
            state._udpClient.BeginReceive(ReceiveMessageAsyncCallback, state); //self-callback, meaning this loops infinitely
        }

        //this will send messages to the server
        void SendMessageAssyncCallback(IAsyncResult result)
        {
            try
            {
                state._udpClient.EndSend(result);

                byte[] messageToSend = GetMessageToSendToServer();
                state._udpClient.BeginSend(messageToSend, messageToSend.Length, new AsyncCallback(SendMessageAssyncCallback), state);

                Debug.Log("Message sent successfully!");
            }
            catch (SocketException ex)
            {
                Debug.Log("Message Error " + ex);
            }
        }

    }

    private void RequestUIDs()
    {
        netObjects = new List<NetworkGameObject>();
        netObjects.AddRange(GameObject.FindObjectsOfType<NetworkGameObject>());
        foreach (NetworkGameObject netObject in netObjects)
        {
            if (netObject.isLocallyOwned && netObject.uniqueNetworkID == 0)
            {
                string myMessage = "I need a UID for local object:" + netObject.localID;
                byte[] array = Encoding.ASCII.GetBytes(myMessage);
                state._udpClient.Send(array, array.Length);
            }
        }

    }

    //this connects to the server
    private void ConnectToServer()
    {
        //this creates a new client
        state._udpClient = new UdpClient();
        //this creates a new ipEndPoint using the ip adress and the port 9050
        state._ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAdress), 9050);
        //we connect the client
        state._udpClient.Connect(state._ipEndPoint);
    }

    //this will get the message that we want to send to the server
    private Byte[] GetMessageToSendToServer()
    {

        PlayerInfoClass playerInfoClass = new PlayerInfoClass();

        // Fill myClass with data
        //playerInfoClass.position = player.playerInfo.position;
        //playerInfoClass.rotation = player.playerInfo.rotation;

        //// Serialize MyClass to a byte array
        //byte[] serializedClass = ObjectsSerializer.Serialize(playerInfoClass);

        //PlayerInfoClass playerInfoClass2 = ObjectsSerializer.Deserialize<PlayerInfoClass>(serializedClass);

        //send the first message
        byte[] array = Encoding.ASCII.GetBytes("UpdateMessage");
        return array;

        //return serializedClass;
    }

    //this will read the message received from the server
    private void ReceivedMessageFromServer(byte[] receiveBytes)
    {
        //this transforms it in a string 
        string receiveString = Encoding.ASCII.GetString(receiveBytes);
        //this writes on the console its message
        Debug.Log("Received " + receiveString + " from " + state._ipEndPoint.ToString());
    }

    //this checks if we received a uid message, if so assign it
    private void IfReceivedMessageIsUIDAssignThem(byte[] receiveBytes)
    {
        //this transforms it in a string 
        string receiveString = Encoding.ASCII.GetString(receiveBytes);
        if (receiveString.Contains("Assigned UID:"))
        {

            int parseFrom = receiveString.IndexOf(':');
            int parseTo = receiveString.LastIndexOf(';');

            //we need to parse the string from the server back into ints to work with
            int localID = Int32.Parse(BetweenStrings(receiveString, ":", ";"));
            int globalID = Int32.Parse(receiveString.Substring(receiveString.IndexOf(";") + 1));

            Debug.Log("Got assignment: " + localID + " local to: " + globalID + " global");

            foreach (NetworkGameObject netObject in netObjects)
            {
                //if the local ID sent by the server matches this game object
                if (netObject.localID == localID)
                {
                    Debug.Log(localID + " : " + globalID);
                    //the global ID becomes the server-provided value
                    netObject.uniqueNetworkID = globalID;
                }
            }
        }

    }

    public static String BetweenStrings(String text, String start, String end)
    {
        int p1 = text.IndexOf(start) + start.Length;
        int p2 = text.IndexOf(end, p1);

        if (end == "") return (text.Substring(p1));
        else return text.Substring(p1, p2 - p1);
    }


    //this closes the socket when we leave the progran
    private void CloseSocket()
    {
        if (state._udpClient != null)
        {
            state._udpClient.Close();
        }
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }
}


