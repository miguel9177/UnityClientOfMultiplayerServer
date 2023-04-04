using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    //this stores the udp state
    public struct UdpState
    {
        public UdpClient _udpClient;
        public IPEndPoint _ipEndPoint;
    }

    //this stores the state of the udp
    static UdpState state = new UdpState();
    //this stores the player name
    public string playerName;
    //this stores the received message from the server
    string receiveString = "";
    //this stores if the player is connected to the serrver or not, to fix infinte loop on disconnect from server
    bool connectedToServer = true;
    //this stores the prefab for new players
    [SerializeField] GameObject networkAvatar;

    //this stores all the gameobjects from the worldd
    public List<NetworkGameObject> worldState;

    //this stores the ipadress of the server
    string ipAdress = "127.0.0.1";

    // Start is called before the first frame update
    void Start()
    {
        //this connects to the server
        ConnectToServer();

        //get the message that is necessary to send the server for a new connection
        byte[] messageToSendForLogin = GetMessageToLoginToServer();
        //start the loop of sending messages to the server
        state._udpClient.Send(messageToSendForLogin, messageToSendForLogin.Length);
        //this starts the infinite loop void that will always be receiving the information from the server
        state._udpClient.BeginReceive(ReceiveMessageAsyncCallback, state);

        //this requests Uids to the server
        RequestUIDs();

        //this will receive all messages from the server, being called by itself
        void ReceiveMessageAsyncCallback(IAsyncResult result)
        {
            //trys to receive the message
            try
            {
                //this gets the packet from the server in bytes
                byte[] receiveBytes = state._udpClient.EndReceive(result, ref state._ipEndPoint);

                //if the message we received was a Uid assignement we assign the uid to tthe multiplayer game object
                IfReceivedMessageIsUIDAssignThem(receiveBytes);

                //send the received message to the function that handles the received messages
                ReceivedMessageFromServer(receiveBytes);

                //this recalls this function to make it loop infinitely
                state._udpClient.BeginReceive(ReceiveMessageAsyncCallback, state); //self-callback, meaning this loops infinitely
            }
            //if theres an error receiving the message we disconnect the player from the server
            catch(Exception e)
            {
                connectedToServer = false;
            }
        }
    
        //we start the send network updates coroutine
        StartCoroutine(SendNetworkUpdates(state._udpClient));
        //we start the update world state coroutine
        StartCoroutine(UpdateWorldState(state._udpClient));
    }

    //this function requests the uids from the server
    private void RequestUIDs()
    {
        //we repopulate the world state list, by populating it with all network game objeccts
        worldState = new List<NetworkGameObject>();
        worldState.AddRange(GameObject.FindObjectsOfType<NetworkGameObject>());

        //we loop through all network gameobjects in the world
        foreach (NetworkGameObject netObject in worldState)
        {
            //if the current netobject is locally onwed and the unique network id is 0, it means the object is ours and hasnt got the uid setted up
            if (netObject.isLocallyOwned && netObject.uniqueNetworkID == 0)
            {
                //we create a new message string asking for a new uid
                string myMessage = "I need a UID for local object:" + netObject.localID;
                //this converts the message created above to an array of bytes
                byte[] array = Encoding.ASCII.GetBytes(myMessage);
                //this sends the message to the server
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

    //this will get the message that we want to send to login to the server
    private Byte[] GetMessageToLoginToServer()
    {
        //send the first message
        byte[] array = Encoding.ASCII.GetBytes("FirstEntrance");
        return array;
    }

    //this coroutine sends the network updates to the server
    IEnumerator SendNetworkUpdates(UdpClient client)
    {
        //infinite loop
        while (true)
        {
            //repopulates the world state with all network gameobjects
            worldState = new List<NetworkGameObject>();
            worldState.AddRange(GameObject.FindObjectsOfType<NetworkGameObject>());

            //this foreach loops through all gameobjects
            foreach (NetworkGameObject netObject in worldState)
            {
                //if the object is locally owned, and the unique network id is diferent then 0
                if (netObject.isLocallyOwned && netObject.uniqueNetworkID != 0)
                {
                    //this string stores the object position and rotation
                    string A = Encoding.ASCII.GetString(netObject.GetPosAndRotToPacket());
                    //this sends the posiiton and rotation to the server
                    client.Send(netObject.GetPosAndRotToPacket(), netObject.GetPosAndRotToPacket().Length);
                }
            }

            //we wait 0.5 seconds to not flud the server
            yield return new WaitForSeconds(0.5f);
            //yield return new WaitForEndOfFrame();
        }
    }

    //this updates the world state
    IEnumerator UpdateWorldState(UdpClient client)
    {
        //while we are connected to the server
        while (connectedToServer)
        {
            //repopulates the world state with all network gameobjects
            worldState = new List<NetworkGameObject>();
            worldState.AddRange(GameObject.FindObjectsOfType<NetworkGameObject>());

            //we store the receive string
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
                            //we update the pos and rot if we dont own the gameobject
                            if (!worldState[i].isLocallyOwned)
                            {
                                worldState[i].UpdatePosAndRotFromPacket(previousRecieveString);
                            }
                            //we update the hp if we ownn the player
                            else
                            {
                                worldState[i].UpdateHpOfPlayerFromPacket(previousRecieveString);
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
                        //if object is not in the worldd we spawn the network object
                        if (!objectIsAlreadyInWorld)
                        {
                            //we spawn the new player object 
                            GameObject otherPlayerAvatar = Instantiate(networkAvatar);
                            //update its component properties from the packet
                            otherPlayerAvatar.GetComponent<NetworkGameObject>().uniqueNetworkID = GetGlobalIDFromPacket(previousRecieveString);
                            otherPlayerAvatar.GetComponent<NetworkGameObject>().UpdatePosAndRotFromPacket(previousRecieveString);
                        }
                    }
                }

            }

            //if theres no messages left from the server we wait a frame, we use this so that unity reads all data and only stops when theres no data left, this fixed
            //the problem of unity being much slower then unreal at updating its information
            if (NoMessagesLeft())
            {
                yield return new WaitForEndOfFrame();
            }

        }
    }

    //this will return true if theres no messages left on the udp client
    private bool NoMessagesLeft()
    {
        return state._udpClient.Client.Available == 0;
    }

    //this gets the global id from the packet
    int GetGlobalIDFromPacket(String packet)
    {
        return Int32.Parse(packet.Split(';')[1]);
    }

    //this will read the message received from the server
    private void ReceivedMessageFromServer(byte[] receiveBytes)
    {
        //this transforms it in a string 
        receiveString = Encoding.ASCII.GetString(receiveBytes);
    }

    //this checks if we received a uid message, if so assign it
    private void IfReceivedMessageIsUIDAssignThem(byte[] receiveBytes)
    {
        //this transforms it in a string 
        receiveString = Encoding.ASCII.GetString(receiveBytes);
        //if the message received is a Assigned Uid message
        if (receiveString.Contains("Assigned UID:"))
        {
            //we get the local id from the message received
            int localID = Int32.Parse(BetweenStrings(receiveString, ":", ";"));
            //we get the global id from the message received
            int globalID = Int32.Parse(receiveString.Substring(receiveString.IndexOf(";") + 1));

            //this loops through all objects in the world state
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

    //this gets the value between the start and end
    public static String BetweenStrings(String text, String start, String end)
    {
        //this finds the position of the start string and we add its length
        int p1 = text.IndexOf(start) + start.Length;
        //this finds the position of the end string starting from the p1 position
        int p2 = text.IndexOf(end, p1);

        //if end is null we return the substring of p1
        if (end == "") return (text.Substring(p1));
        //else we return the substring of p1 and p2
        else return text.Substring(p1, p2 - p1);
    }

    //this is called when an enemy player was shot by us
    public void AnEnemyPlayerWasShotByUs(NetworkGameObject characterWeWit, string nameOfWeapon)
    {
        //we get the message to send to the server
        string enemyPlayerHitMessage = "GameplayEvent: Player shot another player: Player with id;" + GameManager.Instance.ourPlayerNetworkObject.uniqueNetworkID + "; shot player with id; " + characterWeWit.uniqueNetworkID + "; with weapon;" + nameOfWeapon;
        //we send the message to the server
        SendStringMessage(enemyPlayerHitMessage);
    }

    //this closes the socket when we leave the program
    private void CloseSocket()
    {
        if (state._udpClient != null)
        {
            state._udpClient.Close();
        }
    }

    //if we close unity we close the socket
    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    #region Helper functions

    //this sends a message to the server
    private void SendStringMessage(string message)
    {
        //creates an byte array of the message received
        byte[] array = Encoding.ASCII.GetBytes(message);
        //sends the message to the server
        state._udpClient.Send(array, array.Length);
    }

    #endregion
}


