using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    //this stores the player layer mask
    public LayerMask charactersLayerMask;
    //this stores the player camera
    public Camera playerCamera;
    //this stores the name of the weapon
    public string nameOfWeapon = "Raycast Weapon";
    //this stores the max range of the weapon
    public float maxRange;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if the user clicks the mouse button we call the function character shot weapon
        if (Input.GetMouseButtonDown(0))
            CharacterShotWeapon();
    }

    //this is called everytime the user clicks the mouse left key
    private void CharacterShotWeapon()
    {
        //we do raycast
        RaycastHit hit;
        //if the ray collided with a player
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, maxRange, charactersLayerMask))
        {
            //if the hit has the network gameobject component
            if(hit.transform.TryGetComponent(out NetworkGameObject enemyPlayerThatWasShot))
            {
                //if the enemy that we shot is null we leave
                if (enemyPlayerThatWasShot == null)
                    return;

                //this tells the server that we shot a player
                GameManager.Instance.netManager.AnEnemyPlayerWasShotByUs(enemyPlayerThatWasShot, nameOfWeapon);
            }
        }
        else
        {
            Debug.Log("No Hit");
        }
    }
}
