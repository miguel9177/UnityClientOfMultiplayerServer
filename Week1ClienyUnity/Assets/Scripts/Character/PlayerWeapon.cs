using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public LayerMask charactersLayerMask;
    public Camera playerCamera;
    public string nameOfWeapon = "Raycast Weapon";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            CharacterShotWeapon();
    }

    private void CharacterShotWeapon()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, charactersLayerMask))
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            if(hit.transform.TryGetComponent(out NetworkGameObject enemyPlayerThatWasShot))
            {
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
