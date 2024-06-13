using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button readyMenuBtn;
    [SerializeField] private TextMeshProUGUI lobbyNameTxt;
    [SerializeField] private TextMeshProUGUI lobbyCodeTxt;


    private void Awake() {
        mainMenuBtn.onClick.AddListener(() => {
            KitchenGameLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.GameMenuScene);
        });
        readyMenuBtn.onClick.AddListener(() => {
            CharacterSelectReady.Instance.SetPlayerReady();
        });
    }

    private void Start()
    {
        Lobby lobby = KitchenGameLobby.Instance.GetLobby();

        lobbyNameTxt.text = "Lobby Name: " + lobby.Name;
        lobbyCodeTxt.text = "Lobby Code: " + lobby.LobbyCode;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
