﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public Player localPlayer = new Player();
    public List<Player> playerList = new List<Player>();

    private static GameManager gameManagerInstance;
    public static GameManager getInstance { get { return gameManagerInstance; } }

    public string newConnectionID = "";
    
    /// <summary>
    /// Whenever localPlayer makes a change to the current gameState, notify all subscribers (Observers).
    /// </summary>
    public void notifyAll()
    {
        foreach(Player player in playerList)
        {
            // TODO: Send out packet.
        }
    }

    void Awake()
    {
        // Testing
        playerList.Add(new Player());
        Player p = new Player();
        p.id = 222;
        p.xcoord = 999F;
        p.ycoord = 777F;
        playerList.Add(p);

        if (gameManagerInstance != null && gameManagerInstance != this)
        {
            Destroy(this);
        }
        else
        {
            gameManagerInstance = this;
        }
    }

    /// <summary>
    /// Process new connection ID
    /// </summary>
    /// <param name="id">ID of new connection</param>
    public void processNewClientConnection(string id)
    {
        newConnectionID = id;
        //  generate new player
        //  send packet
        byte[] msg = new PacketBuilder().buildPacket(PacketBuilder.ContentTypeEnum.PlayerIdAssignment);
        Server.Send(msg, id);
        newConnectionID = "";
    }
    public override string ToString() {
        return "GAMESTATE";
    }

}
