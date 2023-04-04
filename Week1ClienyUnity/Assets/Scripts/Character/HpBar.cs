using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    //this stores the network gamobject that this hp bar is associatted to
    public NetworkGameObject myNetworkObject;
    //this stores the hp text ui to use to display the hp
    public TextMeshProUGUI txtHp;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //this edits the hp text to match the network object associated with this text 
        txtHp.text = "Hp: " + myNetworkObject.hp;
    }
}
