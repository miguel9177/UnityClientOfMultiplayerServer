using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class NetworkGameObject : MonoBehaviour
{
    //if this is true it means we are the owners of this object
    [SerializeField] public bool isLocallyOwned;
    //this stores the unique id network of the object
    [SerializeField] public int uniqueNetworkID;
    //this stores the local id of the object
    [SerializeField] public int localID;
    //this stores the hp of the object
    public float hp;
    //this stores the last assigned local id, its static to be able to always add a new number to the current gameobject
    static int lastAssignedLocalID = 0;

    private void Awake()
    {
        //if we are locally owned, we set the local id
        if (isLocallyOwned) localID = lastAssignedLocalID++;
    }

    //this rotates around the y, its necessary to fix the rotation convertion of the unreal and unity
    public Quaternion AddRotationAroundY(Quaternion original, float degrees)
    {
        //this adds the requested degrees to an empty quaternion
        Quaternion rotationY = Quaternion.Euler(0, degrees, 0);
        //we multiply the original quaternion with the y rotated quaternion
        return original * rotationY;
    }

    //this convert the relevant info on the gameobject to a packet
    public byte[] GetPosAndRotToPacket() 
    {
        //this gets the object quaternion with an 90 rotation on the y axis, this is necessary to match the unity and unreal rotation
        Quaternion rotOfPlayerWithMinus90OnY = AddRotationAroundY(transform.rotation, 90);
        
        //we create the string with the full object data
        string returnVal = "Object data;" + uniqueNetworkID + ";" +
                            transform.position.x * -100 + ";" +
                            transform.position.z * 100 + ";" +
                            transform.position.y * 100 + ";" +
                            rotOfPlayerWithMinus90OnY.x + ";" +
                            rotOfPlayerWithMinus90OnY.z + ";" +
                            rotOfPlayerWithMinus90OnY.y + ";" +
                            rotOfPlayerWithMinus90OnY.w + ";"
                            ;

        //we convert the string to the bytes
        return Encoding.ASCII.GetBytes(returnVal);
    }

    //convert a packet to the relevant data and apply it to the gameobject properties
    public void UpdatePosAndRotFromPacket(string data) 
    {
        //we split the data to a array of strings divided by the ;
        string[] values = data.Split(';');

        //we set the cultural info since my pc is portuguese
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        //we get the positions from the packet
        float posX = float.Parse(values[2], NumberStyles.Any, ci);
        float posZ = float.Parse(values[3], NumberStyles.Any, ci);
        float posY = float.Parse(values[4], NumberStyles.Any, ci);

        //we get the rotation from the packet
        float rotX = float.Parse(values[5], NumberStyles.Any, ci);
        float rotZ = float.Parse(values[6], NumberStyles.Any, ci);
        float rotY = float.Parse(values[7], NumberStyles.Any, ci);
        float rotW = float.Parse(values[8], NumberStyles.Any, ci);

        //we change the object position to the one received
        transform.position = new Vector3(posX / -100, posY / 100, posZ / 100);
        //we change the object rotation to the one received
        transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
        //we change the object hp to the one received
        hp = float.Parse(values[9], NumberStyles.Any, ci);
    }

    //this updates the hp of the player
    public void UpdateHpOfPlayerFromPacket(string data)
    {
        //we split the data to a array of strings divided by the ;
        string[] values = data.Split(';');

        //we set the cultural info since my pc is portuguese
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
        //this edits the object hp
        hp = float.Parse(values[9], NumberStyles.Any, ci);
    }
}
