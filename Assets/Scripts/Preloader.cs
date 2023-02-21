using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class Preloader : MonoBehaviour
{
    /* Start(): Monobehavior Event Function
     * Executes when script instance is being loaded. Can be re-run by re-enabling 
     * 
     * We overload Start()
     */
    void Start()
    {
        if (Application.isEditor)
            return;

        var args = GetCommandlineArgs();

        if (args.TryGetValue("-mode", out string mode))
        {
            switch (mode)
            {
                case "server":
                    NetworkManager.Singleton.StartServer();
                    break;
                case "host":
                    NetworkManager.Singleton.StartHost();
                    break;
                case "client":
                    NetworkManager.Singleton.StartClient();
                    break;
            }
        }

    }

    private Dictionary<string, string> GetCommandlineArgs()
    {
        Dictionary<string, string> argDictionary = new Dictionary<string, string>();

        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; ++i)
        {
            var arg = args[i].ToLower();
            if (arg.StartsWith("-"))
            {
                var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;

                argDictionary.Add(arg, value);
            }
        }
        return argDictionary;
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
