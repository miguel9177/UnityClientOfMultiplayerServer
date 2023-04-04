using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //this stores our player network gameobject, so that we can know who is our player (this is used to send the shooting message)
    public NetworkGameObject ourPlayerNetworkObject;
    //we have an instance since this is a singleton
    public static GameManager Instance { get; private set; }
    //we store the netmanager tto give access to other gameobjects
    public NetworkManager netManager;

    //does the mandatory singleton code, making sure that theres only one instance of this class
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
}
