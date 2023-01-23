using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions.Vector;
using System.Runtime.Serialization;

//this has all the player info to send to the network at every possible way, since this info is vital
[DataContract]
public class PlayerInfoClass 
{
    [DataMember]
    public Vector3Serializable position;
    [DataMember]
    public Vector3Serializable rotation;

    public PlayerInfoClass()
    {
        position = new Vector3Serializable(0,0,0);
        rotation = new Vector3Serializable(0,0,0);
    }   
}

//this is the player info monobehaviour and it will track the plkayer transform and store it in a Serializable class (in this case PlayerInfoClass)
public class PlayerInfo : MonoBehaviour
{
    public PlayerInfoClass playerInfo = new PlayerInfoClass();

    public Transform playerTransform;

    public void Update()
    {
        playerInfo.position = new Vector3Serializable(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z);
        playerInfo.rotation = new Vector3Serializable(playerTransform.eulerAngles.x, playerTransform.eulerAngles.y, playerTransform.eulerAngles.z);
    }
}
