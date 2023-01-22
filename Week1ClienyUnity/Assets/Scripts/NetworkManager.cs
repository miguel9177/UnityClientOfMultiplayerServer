using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;


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
    static string playerName = "Miguel";

    string ipAdress = "192.168.0.38";

    // Start is called before the first frame update
    void Start()
    {
        ConnectToServer();

        //get the message that is necessary to send the server for a new connection
        byte[] messageToSendForLogin = GetMessageToLoginToServer();
        //start the loop of sending messages to the server
        state._udpClient.BeginSend(messageToSendForLogin, messageToSendForLogin.Length, new AsyncCallback(SendMessageAssyncCallback), state);

        //this starts the infinite loop void that will always be receiving the information from the server
        state._udpClient.BeginReceive(ReceiveMessageAsyncCallback, state);
        
        //this will receive all messages from the server
        void ReceiveMessageAsyncCallback(IAsyncResult result)
        {
            //this gets the packet from the server in bytes
            byte[] receiveBytes = state._udpClient.EndReceive(result, ref state._ipEndPoint);
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

    //this will get the message that we want to send to the server
    private Byte[] GetMessageToSendToServer()
    {
        //store the message to send to the server
        string myMessage2 = "PlayerName: " + playerName + " / Ping: HAVENT MADE PING TIMER YET";
        //make the message a byte array, since we always need to do this
        byte[] array = Encoding.ASCII.GetBytes(myMessage2);
        return array;
    }

    //this will read the message received from the server
    private void ReceivedMessageFromServer(byte[] receiveBytes)
    {
        //this transforms it in a string 
        string receiveString = Encoding.ASCII.GetString(receiveBytes);
        //this writes on the console its message
        Debug.Log("Received " + receiveString + " from " + state._ipEndPoint.ToString());
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


