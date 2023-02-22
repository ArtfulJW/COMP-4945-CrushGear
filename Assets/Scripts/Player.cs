using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{

    [SerializeField]

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter Trigger");
    }
}
