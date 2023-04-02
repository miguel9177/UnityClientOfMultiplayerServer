using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    public NetworkGameObject myNetworkObject;
    public TextMeshProUGUI txtHp;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        txtHp.text = "Hp: " + myNetworkObject.hp;
    }
}
