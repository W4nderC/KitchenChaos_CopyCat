using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button quickJoinBtn;  
    [SerializeField] private Button joinCodeBtn;  
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private LobbyCreateUI lobbyCreateUI;  
    [SerializeField] private Transform lobbyContainer; 
    [SerializeField] private Transform lobbyTemplate; 


    private void Awake() {
        mainMenuBtn.onClick.AddListener(() => {
            KitchenGameLobby.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.GameMenuScene);
        });
        createLobbyBtn.onClick.AddListener(() => {
            lobbyCreateUI.Show();
        });
        quickJoinBtn.onClick.AddListener(() => {
            KitchenGameLobby.Instance.QuickJoin();
        });
        joinCodeBtn.onClick.AddListener(() => {
            KitchenGameLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        playerNameInputField.text = KitchenGameMultiplayer.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string newText) => {
            KitchenGameMultiplayer.Instance.SetPlayerName(newText);
        });

        KitchenGameLobby.Instance.OnLobbyListChanged += KitchenGameLobby_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void KitchenGameLobby_OnLobbyListChanged(object sender, KitchenGameLobby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList (List<Lobby> lobbyList) {
        foreach(Transform child in lobbyContainer){
            if(child == lobbyTemplate) continue; 
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in lobbyList){
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void OnDestroy(){
        KitchenGameLobby.Instance.OnLobbyListChanged -= KitchenGameLobby_OnLobbyListChanged;

    }
}
