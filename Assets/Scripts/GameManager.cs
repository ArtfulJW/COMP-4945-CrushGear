using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public Player localPlayer;
    List<Player> playerList;

    private static GameManager gameManagerInstance;
    public static GameManager getInstance { get { return gameManagerInstance; } }
    
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

    private void Awake()
    {
        if (gameManagerInstance != null && gameManagerInstance != this)
        {
            Destroy(this);
        }
        else
        {
            gameManagerInstance = this;
        }
    }
}
