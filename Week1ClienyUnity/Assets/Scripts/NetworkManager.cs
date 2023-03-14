using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Serializer;
using System.Collections.Generic;
using System.Collections;

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
    string receiveString = "";
    [SerializeField] GameObject networkAvatar;

    public List<NetworkGameObject> worldState;
    //public List<NetworkGameObject> myNetObjects;

    string ipAdress = "10.1.113.216";

    // Start is called before the first frame update
    void Start()
    {
        ConnectToServer();
        RequestUIDs();

        //get the message that is necessary to send the server for a new connection
        byte[] messageToSendForLogin = GetMessageToLoginToServer();
        //start the loop of sending messages to the server
        state._udpClient.Send(messageToSendForLogin, messageToSendForLogin.Length);
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
    
        StartCoroutine(SendNetworkUpdates(state._udpClient));
        StartCoroutine(UpdateWorldState(state._udpClient));
    }

    private void RequestUIDs()
    {
        worldState = new List<NetworkGameObject>();
        worldState.AddRange(GameObject.FindObjectsOfType<NetworkGameObject>());


        //myNetObjects = new List<NetworkGameObject>();
        //myNetObjects.AddRange(GameObject.FindObjectsOfType<NetworkGameObject>());
        foreach (NetworkGameObject netObject in worldState)
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
    private Byte[] GetMessageToLoginToServer()
    {
        //send the first message
        byte[] array = Encoding.ASCII.GetBytes("FirstEntrance");
        return array;
    }

    IEnumerator SendNetworkUpdates(UdpClient client)
    {
        while (true)
        {
            worldState = new List<NetworkGameObject>();
            worldState.AddRange(GameObject.FindObjectsOfType<NetworkGameObject>());

            foreach (NetworkGameObject netObject in worldState)
            {
                if (netObject.isLocallyOwned && netObject.uniqueNetworkID != 0)
                {
                    string A = Encoding.ASCII.GetString(netObject.GetPosAndRotToPacket());
                    client.Send(netObject.GetPosAndRotToPacket(), netObject.GetPosAndRotToPacket().Length);
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }


    IEnumerator UpdateWorldState(UdpClient client)
    {
        while (true)
        {

            //read in the current world state as all network game objects in the scene
            worldState = new List<NetworkGameObject>();
            worldState.AddRange(GameObject.FindObjectsOfType<NetworkGameObject>());

            //cache the recieved packet string - we'll use that later to suspend the couroutine until it changes
            string previousRecieveString = receiveString;

            //if it's an object update, process it, otherwise skip
            if (previousRecieveString.Contains("Object data;"))
            {
                //we'll want to know if an object with this global id is already in the game world
                bool objectIsAlreadyInWorld = false;

                //we'll also want to exclude any invalid packets with a bad global id
                if (GetGlobalIDFromPacket(previousRecieveString) != 0)
                {
                    //for every networked gameobject in the world
                    for (int i = 0; i < worldState.Count; i++)
                    {
                        //if it's unique ID matches the packet, update it's position from the packet
                        if (worldState[i].uniqueNetworkID == GetGlobalIDFromPacket(previousRecieveString) || worldState[i].uniqueNetworkID == 0)
                        {
                            //only update it if we don't own it - you might want to try disabling and seeing the effect
                            if (!worldState[i].isLocallyOwned)
                            {
                                worldState[i].UpdatePosAndRotFromPacket(previousRecieveString);

                            }
                            //if we have any uniqueID matches, our object is in the world
                            objectIsAlreadyInWorld = true;
                        }

                    }

                    //if it's not in the world, we need to spawn it
                    if (!objectIsAlreadyInWorld)
                    {
                        int idReceived = GetGlobalIDFromPacket(previousRecieveString);
                        //for every networked gameobject in the world
                        for (int i = 0; i < worldState.Count; i++) 
                        {
                            //if it's unique ID matches the packet, update it's position from the packet
                            if (worldState[i].uniqueNetworkID == idReceived)
                            {
                                //if we have any uniqueID matches, our object is in the world
                                objectIsAlreadyInWorld = true;
                            }

                        }

                        if(!objectIsAlreadyInWorld)
                        {
                            GameObject otherPlayerAvatar = Instantiate(networkAvatar);
                            //update its component properties from the packet
                            otherPlayerAvatar.GetComponent<NetworkGameObject>().uniqueNetworkID = GetGlobalIDFromPacket(previousRecieveString);
                            otherPlayerAvatar.GetComponent<NetworkGameObject>().UpdatePosAndRotFromPacket(previousRecieveString);
                        }
                    }
                }

            }

            //wait until the incoming string with packet data changes then iterate again

            yield return new WaitUntil(() => !receiveString.Equals(previousRecieveString));
            //yield return new WaitForEndOfFrame();
        }
    }

    int GetGlobalIDFromPacket(String packet)
    {
        return Int32.Parse(packet.Split(';')[1]);
    }

    //this will read the message received from the server
    private void ReceivedMessageFromServer(byte[] receiveBytes)
    {
        //this transforms it in a string 
        receiveString = Encoding.ASCII.GetString(receiveBytes);
        //this writes on the console its message
        Debug.Log("Received " + receiveString + " from " + state._ipEndPoint.ToString());
    }

    //this checks if we received a uid message, if so assign it
    private void IfReceivedMessageIsUIDAssignThem(byte[] receiveBytes)
    {
        //this transforms it in a string 
        receiveString = Encoding.ASCII.GetString(receiveBytes);
        if (receiveString.Contains("Assigned UID:"))
        {

            int parseFrom = receiveString.IndexOf(':');
            int parseTo = receiveString.LastIndexOf(';');

            //we need to parse the string from the server back into ints to work with
            int localID = Int32.Parse(BetweenStrings(receiveString, ":", ";"));
            int globalID = Int32.Parse(receiveString.Substring(receiveString.IndexOf(";") + 1));

            Debug.Log("Got assignment: " + localID + " local to: " + globalID + " global");

            foreach (NetworkGameObject netObject in worldState)
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


