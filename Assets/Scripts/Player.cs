using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private TrackInfo trackInfo;
    [SerializeField]
    private int triggerAmount;
    [SerializeField]
    private int triggerCounter = 0;
    [SerializeField]
    private int lapCounter = 0;

    [SerializeField]
    private NetworkVariable<int> networkTriggerCounter = new NetworkVariable<int>();

    [SerializeField]
    private NetworkVariable<int> networkLapCounter = new NetworkVariable<int>();

    private void Start()
    {
        // Init track info
        trackInfo = GameObject.Find("Track").GetComponent<TrackInfo>();
        triggerAmount = trackInfo.triggers.Count;
    }

    /// <summary>
    /// Proccess Trigger events
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (IsClient && IsOwner)
        {
            Debug.Log("Enter Trigger");
            if (other.tag == "PathPointTrigger")
            {
                other.gameObject.SetActive(false);
                triggerCounter++;
            }
            else if (other.tag == "Goal")
            {
                other.gameObject.SetActive(false);
                lapCounter++;
                triggerCounter = 0;
            }
            UpdateClientServerRpc(triggerCounter, lapCounter);
            processTrigger();
        }
        
    }

    [ServerRpc]
    public void UpdateClientServerRpc(int triggerCount, int lapCount)
    {
        networkTriggerCounter.Value = triggerCount;
        networkLapCounter.Value = lapCount;
    }

    /// <summary>
    /// Activates next trigger point in lap.
    /// </summary>
    void processTrigger()
    {
        Debug.Log("Processing Triggers");
        if (networkTriggerCounter.Value < triggerAmount)
        {
            trackInfo.triggers[networkTriggerCounter.Value].gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("Activate Goal");
            trackInfo.goal.transform.Find("GoalTrigger").gameObject.SetActive(true);
        }
            
    }
}
