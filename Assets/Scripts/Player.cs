using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;

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

    //TIMER
    private bool isRunning;
    [SerializeField]
    private float elapsedSeconds;
    private float bestTimeSeconds = float.MaxValue;

    [SerializeField]
    private NetworkVariable<int> networkTriggerCounter = new NetworkVariable<int>();

    [SerializeField]
    private NetworkVariable<int> networkLapCounter = new NetworkVariable<int>();

    [SerializeField]
    private NetworkVariable<NetworkString> playerNetworkName = new NetworkVariable<NetworkString>();


    private void Start()
    {
        // Init track info
        trackInfo = GameObject.Find("TrackManager").GetComponent<TrackInfo>();
        triggerAmount = trackInfo.triggers.Count;
        setOverlay();
    }

    private void Update()
    {
        if (!isRunning) return;
        elapsedSeconds += Time.deltaTime;
        UIManager.Instance.SetElapsedTime(elapsedSeconds);
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
                UpdateClientServerRpc(triggerCounter, lapCounter);
                processTrigger();
            }
            else if (other.tag == "Goal")
            {
                other.gameObject.SetActive(false);
                lapCounter++;
                triggerCounter = 0;
                UpdateClientServerRpc(triggerCounter, lapCounter);
                processTrigger();
                resetTimer();
            }

        }

    }

    public void SetSpawnPosition()
    {

        //Transform[] t = GetComponentsInChildren<Transform>();
        UnityEngine.Debug.Log(GameObject.Find("Goal").transform.position);
        UnityEngine.Debug.Log("HELL: " + transform.parent.position + " " + transform.parent.name);

        //This for loop works.
        foreach (Transform t in transform)
        {
            t.position += GameObject.Find("Goal").transform.position;
        }

        // This one doesnt work
        //transform.GetComponent<Rigidbody>().position = GameObject.Find("TrackManager").GetComponent<GenerateRoad>().convexHull[0];
        //for (int x = 0; x < t.Length; x++)
        //{
        //    t[x].position += GameObject.Find("TrackManager").GetComponent<GenerateRoad>().convexHull[0];
        //}
        //for (int x = 0; x < t.Length; x++)
        //{
        //    t[x].position += new Vector3(0,3,0);
        //}
    }

    [ServerRpc]
    public void UpdateClientServerRpc(int triggerCount, int lapCount)
    {
        networkTriggerCounter.Value = triggerCount;
        networkLapCounter.Value = lapCount;
    }

    void resetTimer()
    {
        if (!isRunning)
        {
            isRunning = true;
            return;
        }
        if (bestTimeSeconds > elapsedSeconds)
        {
            bestTimeSeconds = elapsedSeconds;
            UIManager.Instance.SetBestTime(bestTimeSeconds);
        }
        elapsedSeconds = 0;
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

    void setOverlay()
    {
        Debug.Log("Setting Player name");
        var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        localPlayerOverlay.text = $"{playerNetworkName.Value}";
    }

    public void SetPlayerID(ulong clientId)
    {
        playerNetworkName.Value = $"Player {clientId}";
    }
}
