using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class NetworkGameObject : MonoBehaviour
{
    [SerializeField] public bool isLocallyOwned;
    [SerializeField] public int uniqueNetworkID;
    [SerializeField] public int localID;
    static int lastAssignedLocalID = 0;
    private void Awake()
    {
        if (isLocallyOwned) localID = lastAssignedLocalID++;
    }

    public byte[] GetPosAndRotToPacket() //convert the relevant info on the gameobject to a packet
    {
        //create a delimited string with the required data
        //note if we put strings in this we might want to check they don’t have a semicolon or use a different delimiter like |
        string returnVal = "Object data;" + uniqueNetworkID + ";" +
                            transform.position.x * 100 + ";" +
                            transform.position.z * -100 + ";" +
                            transform.position.y * 100 + ";" +
                            transform.rotation.x + ";" +
                            transform.rotation.y + ";" +
                            transform.rotation.z + ";" +
                            transform.rotation.w + ";"
                            ;


        return Encoding.ASCII.GetBytes(returnVal);
    }

    public void UpdatePosAndRotFromPacket(string data) //convert a packet to the relevant data and apply it to the gameobject properties
    {
        string[] values = data.Split(';');

        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        float posX = float.Parse(values[2], NumberStyles.Any, ci);
        float posZ = float.Parse(values[3], NumberStyles.Any, ci);
        float posY = float.Parse(values[4], NumberStyles.Any, ci);

        float rotX = float.Parse(values[5], NumberStyles.Any, ci);
        float rotY = float.Parse(values[6], NumberStyles.Any, ci);
        float rotZ = float.Parse(values[7], NumberStyles.Any, ci);
        float rotW = float.Parse(values[8], NumberStyles.Any, ci);

        transform.position = new Vector3(posX / 100, posY / 100, posZ / -100);
        transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
    }
}
