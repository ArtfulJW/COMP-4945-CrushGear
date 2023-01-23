using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class Preloader : MonoBehaviour
{
    
    public string[] acquireStats()
    {
        string[] stats = new string[2];
        // Receive IP and port
        Console.WriteLine("Enter IP:\n> ");
        //TODO REMOVE
        stats[0] = "127.0.0.1"; 
        //stats[0] = Console.ReadLine();
        Console.WriteLine("Enter Port:\n> ");
        //TODO REMOVE
        stats[1] = "25565";
        //stats[1] = Console.ReadLine();
        return stats;
    }

    /* Awake(): Monobehavior Event Function
     * Executes when script instance is being loaded. Can be re-run by re-enabling 
     * 
     * We overload Awake()
     */
    void Awake()
    {
        /*
         * Pull Command Line Arguments to execute desired functionality.
         */

        string[] args = System.Environment.GetCommandLineArgs();
        string[] stats = acquireStats();

        Server server = null;
        Client client = null;
        foreach (string str in args)
        {
            
            switch (str){
                case "-server":
                    try {
                        // Load Server with IP and port
                        server = GetComponent<Server>();
                        server.initServer(stats[0], Convert.ToInt32(stats[1]));
                    } catch (Exception e) {
                        UnityEngine.Debug.Log(e.ToString());
                    }
                    
                    break;
                case "-client-host":
                    try {
                        // Load Server with IP and port
                        server = GetComponent<Server>();
                        server.initServer(stats[0], Convert.ToInt32(stats[1]));
                    } catch (Exception e) {
                        UnityEngine.Debug.Log(e.ToString());
                    }
                    try {
                        // Load client with IP and port
                        client = GetComponent<Client>();
                        client.initClient(stats[0], Convert.ToInt32(stats[1]));
                    } catch (Exception e) {
                        UnityEngine.Debug.Log(e.ToString());
                    }
                    
                    break;
                default:
                    try {
                        // Load client with IP and port
                        client = GetComponent<Client>();
                        client.initClient(stats[0], Convert.ToInt32(stats[1]));
                    } catch (Exception e) {
                        UnityEngine.Debug.Log(e.ToString());
                    }
                    break;
            }
        }

    }


    
    // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // Update is called once per frame
    // void Update()
    // {
        
    // }
}
