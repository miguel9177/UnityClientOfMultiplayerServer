using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions.Vector;

//this has all the player info to send to the network at every possible way, since this info is vital
[Serializable]
public class PlayerInfoClass 
{

    public Vector3Serializable position;

    public Vector3Serializable rotation;

    public PlayerInfoClass()
    {
        position = new Vector3Serializable(Vector3.zero);
        rotation = new Vector3Serializable(Vector3.zero);
    }   
}

//this is the player info monobehaviour and it will track the plkayer transform and store it in a Serializable class (in this case PlayerInfoClass)
public class PlayerInfo : MonoBehaviour
{
    public PlayerInfoClass playerInfo = new PlayerInfoClass();

    public Transform playerTransform;

    public void Update()
    {
        playerInfo.position = new Vector3Serializable(playerTransform.position);
        playerInfo.rotation = new Vector3Serializable(playerTransform.eulerAngles);
    }
}
