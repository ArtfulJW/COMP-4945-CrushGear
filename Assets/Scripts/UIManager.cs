using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Button startServerButton;

    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private TMP_InputField joinCodeInput;

    [SerializeField]
    private GameObject timerPanel;

    [SerializeField]
    private TextMeshProUGUI playersInGameText;

    [SerializeField]
    private TextMeshProUGUI roomCode;


    private bool hasServerStarted;

    private void Awake()
    {
        Cursor.visible = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        // START SERVER
        startServerButton?.onClick.AddListener(() => {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started...");
                enableRaceScene();
            }
            else
            {
                Debug.Log("Unable to start server...");
            }
        });

        // START HOST
        startHostButton?.onClick.AddListener(async () => {
            // this allows the UnityMultiplayer and UnityMultiplayerRelay scene to work with and without
            // relay features - if the Unity transport is found and is relay protocol then we redirect all the 
            // traffic through the relay, else it just uses a LAN type (UNET) communication.
            if (RelayManager.Instance.IsRelayEnabled)
                await RelayManager.Instance.SetupRelay();

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started...");
                activateRacingUI();
                //enableRaceScene();
            }
            else
            {
                Debug.Log("Unable to start host...");
            }

        });

        // START CLIENT
        startClientButton?.onClick.AddListener(async () => {
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text))
                await RelayManager.Instance.JoinRelay(joinCodeInput.text);

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started...");
                activateRacingUI();
                
            }
            else
            {
                Debug.Log("Unable to start client...");
            }

        });

        // STATUS TYPE CALLBACKS
        NetworkManager.Singleton.OnClientConnectedCallback += (id) => {
            Debug.Log($"{id} just connected...");
            enableRaceScene();
        };

        NetworkManager.Singleton.OnServerStarted += () => {
            hasServerStarted = true;
        };

    }

    // Update is called once per frame
    void Update()
    {
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
        roomCode.text = $"Code: {RelayManager.Instance.GetRoomCode()}";
    }

    void activateRacingUI()
    {
        startHostButton?.gameObject.SetActive(false);
        startClientButton?.gameObject.SetActive(false);
        joinCodeInput?.gameObject.SetActive(false);
        timerPanel?.SetActive(true);
        playersInGameText?.gameObject.SetActive(true);
        roomCode?.gameObject.SetActive(true);
    }

    void enableRaceScene()
    {
        //Switch scenes to game from lobby scene
        GameObject.Find("TrackManager").GetComponent<GenerateRoad>().InitializeTrack();
    }
}
