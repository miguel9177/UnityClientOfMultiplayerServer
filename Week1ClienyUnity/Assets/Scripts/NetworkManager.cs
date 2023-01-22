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

    //this stores the ping timer, so that i can know my ping
    System.Diagnostics.Stopwatch pingTimer = new System.Diagnostics.Stopwatch();
    //this stores the state of the udp
    static UdpState state;
    //this stores the Timer
    TimeSpan timer = new TimeSpan();
    //this stores the ui text that stores the ping
    public TextMeshProUGUI txtPing;

    //this stores the player name
    static string playerName = "Miguel";

    string ipAdress = "192.168.0.38";

    // Start is called before the first frame update
    void Start()
    {
        //this creates a new client
        UdpClient client = new UdpClient();
        //this creates a new ipEndPoint using the ip adress and the port 9050
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAdress), 9050); 
        //we connect the client
        client.Connect(ep);

        //variable with the first entrance message so that the server knows that this is the first connection made
        string myMessage = "FirstEntrance";
        //this transforms the first entrance message into an array
        byte[] array = Encoding.ASCII.GetBytes(myMessage);
        //we send the message to the server
        client.Send(array, array.Length);

        //this starts the infinite loop void that will always be receiving the information from the server
        client.BeginReceive(ReceiveAsyncCallback, state);

        //this will receive all messages from the server
        void ReceiveAsyncCallback(IAsyncResult result)
        {
            //this gets the packet from the server in bytes
            byte[] receiveBytes = client.EndReceive(result, ref ep); 
            //this transforms it in a string 
            string receiveString = Encoding.ASCII.GetString(receiveBytes); 
            //this writes on the console its message
            Debug.Log("Received " + receiveString + " from " + ep.ToString()); 
            //this recalls this function to make it loop infinitely
            client.BeginReceive(ReceiveAsyncCallback, state); //self-callback, meaning this loops infinitely
            //this stops the ping timer
            pingTimer.Stop();
            //this gets the time elapsed
            timer = pingTimer.Elapsed;
            //store the message to send to the server
            string myMessage2 = "PlayerName: " + playerName + " / Ping: " + timer.Milliseconds;
            //make the message a byte array, since we always need to do this
            byte[] array2 = Encoding.ASCII.GetBytes(myMessage2);
            //we send the information
            client.Send(array2, array2.Length);
            //we restart the ping timer and start it again
            pingTimer.Restart();
            pingTimer.Start();
        }
    }


    // Update is called once per frame
    void Update()
    {
        txtPing.text = "Ping: " + timer.Milliseconds;
    }

}


