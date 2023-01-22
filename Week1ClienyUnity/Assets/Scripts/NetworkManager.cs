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
        public UdpClient u;
        public IPEndPoint e;
    }

    System.Diagnostics.Stopwatch pingTimer = new System.Diagnostics.Stopwatch();
    static UdpClient client;
    static IPEndPoint ep;
    static UdpState state;
    TimeSpan timer = new TimeSpan();
    public TextMeshProUGUI txt;


    static string playerName = "Miniking";

    //string ipAdress = "127.0.0.1";
    //string ipAdress = "10.1.42.129";
    string ipAdress = "192.168.0.38";
    // Start is called before the first frame update
    void Start()
    {
        UdpClient client = new UdpClient();
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAdress), 9050); // endpoint where server is listening (testing localy)
        client.Connect(ep);

        string myMessage = "FirstEntrance";
        byte[] array = Encoding.ASCII.GetBytes(myMessage);
        client.Send(array, array.Length);

        client.BeginReceive(ReceiveAsyncCallback, state);

        void ReceiveAsyncCallback(IAsyncResult result)
        {

            byte[] receiveBytes = client.EndReceive(result, ref ep); //get the packet
            string receiveString = Encoding.ASCII.GetString(receiveBytes); //decode the packet
            Debug.Log("Received " + receiveString + " from " + ep.ToString()); //display the packet
            client.BeginReceive(ReceiveAsyncCallback, state); //self-callback, meaning this loops infinitely
            pingTimer.Stop();
            timer = pingTimer.Elapsed;
            string myMessage2 = "PlayerName: " + playerName + " / Ping: " + timer.Milliseconds;
            byte[] array2 = Encoding.ASCII.GetBytes(myMessage2);
            client.Send(array2, array2.Length);
            pingTimer.Restart();
            pingTimer.Start();
        }
    }


    // Update is called once per frame
    void Update()
    {
        txt.text = "Ping: " + timer.Milliseconds;
    }

}


