using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance { get; private set; }


    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;


    private enum State {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    [SerializeField] private Transform playerPrefab;


    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private bool isLocalPlayerRready;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 90f;
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private bool autoTestGamePausedState;


    private void Awake() {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    private void Start() {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn(){
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if(IsServer) {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        autoTestGamePausedState = true;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if(isGamePaused.Value) {
            Time.timeScale = 0f;

            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        } else {
            Time.timeScale = 1f;

            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if(state.Value == State.WaitingToStart) {
            isLocalPlayerRready = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc ();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc (ServerRpcParams serverRpcParams = default) {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            if(!playerReadyDictionary.ContainsKey(clientId) 
            || !playerReadyDictionary[clientId]) {
                //This player is not ready
                allClientsReady = false;
                break;
            }
        }

        if(allClientsReady) {
            state.Value = State.CountdownToStart;
        }
    }

    private void GameInput_OnPauseAction (object sender, EventArgs e) {
        TogglePauseGame();
    }

    // Update is called once per frame
    private void Update()
    {
        if(!IsServer) {
            return;
        }

        switch (state.Value) {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if(countdownToStartTimer.Value < 0f) {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;                   
                }
                break;
            
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if(gamePlayingTimer.Value < 0f) {
                    state.Value = State.GameOver;
                }
                break;
            
            case State.GameOver:
                break;
        }
    }

    private void LateUpdate() {
        if(autoTestGamePausedState == true) {
            autoTestGamePausedState = false;
            TestGamePausedState();
        }
    }

    public bool IsGamePlaying(){
        return state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive() {
        return state.Value == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer() {
        return countdownToStartTimer.Value;
    }

    public bool IsGameOver(){
        return state.Value == State.GameOver;
    }

    public bool IsWaitingToStart(){
        return state.Value == State.WaitingToStart;
    }

    public bool IsLocalPlayerReady(){
        return isLocalPlayerRready;
    }

    public float GetGamePlayingTimerNormalized(){
        return 1- (gamePlayingTimer.Value/gamePlayingTimerMax);
    }

    public void TogglePauseGame(){
        isLocalGamePaused = !isLocalGamePaused;
        if(isLocalGamePaused) {
            PauseGamerServerRpc();
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        } else {
            UnpauseGamerServerRpc();
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        } 
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGamerServerRpc (ServerRpcParams serverRpcParams = default) {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGamerServerRpc (ServerRpcParams serverRpcParams = default) {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;
    }

    private void TestGamePausedState () {
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            if(playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId]) {
                // This player is paused
                isGamePaused.Value = true;
                return;
            }
        }
        //All player are unpaused
        isGamePaused.Value = false;
    }
}
