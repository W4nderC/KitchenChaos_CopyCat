using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public const int MAX_PLAYER_AMOUNT = 4;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    public static KitchenGameMultiplayer Instance { get; private set; }


    
    public static bool playMultiplayer;


    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    [SerializeField] private KitchenObjListSO kitchenObjListSO;
    [SerializeField] private List<Color> playerColorList;


    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;


    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));
        playerDataNetworkList = new NetworkList<PlayerData>(); 
        playerDataNetworkList.OnListChanged += playerDataNetworkList_OnListChanged;
    }

    private void Start() {
        if(!playMultiplayer) {
            //Singleplayer
            StartHost();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    private void playerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetPlayerName(){
        return playerName;
    }

    public void SetPlayerName (string playerName) {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }

    public void StartHost(){
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++) {
            PlayerData playerData = playerDataNetworkList[i];
            if(playerData.clientId == clientId) {
                //Disconnected
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData {
            clientId = clientId,
            colorId = GetFirstUnusedColorId()
        });
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if(SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString()) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }
        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }
        connectionApprovalResponse.Approved = true;

    }

    public void StartClient(){
        OnTryingToJoinGame?.Invoke(this,EventArgs.Empty);
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc (string playerName, ServerRpcParams serverRpcParams = default) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerName = playerName;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc (string playerId, ServerRpcParams serverRpcParams = default) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerId = playerId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnKitchenObject(
        KitchenObjectSO kitchenObjectSO,
        IKitchenObjectParent kitchenObjectParent
    ){
        SpawnKitchenObjServerRpc(
            GetKitchenObjectSOIndex(kitchenObjectSO),
            kitchenObjectParent.GetNetworkObject()
        );
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjServerRpc 
    (
        int kitchenObjSOIndex, 
        NetworkObjectReference kitchenObjParentNetworkObjRef
    ) 
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjSOIndex);

        Transform kitchenObjTransform = Instantiate(kitchenObjectSO.prefab);

        NetworkObject kitchenObjNetworkObj = kitchenObjTransform.GetComponent<NetworkObject>();
        kitchenObjNetworkObj.Spawn(true);
    
        KitchenObject kitchenObject = kitchenObjTransform.GetComponent<KitchenObject>();

        kitchenObjParentNetworkObjRef.TryGet(out NetworkObject kitchenObjParentNetworkObj);
        IKitchenObjectParent kitchenObjectParent = kitchenObjParentNetworkObj.GetComponent<IKitchenObjectParent>();

        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);

    }

    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO){
        return kitchenObjListSO.kitchenObjectSOsList.IndexOf(kitchenObjectSO);
    }

    public KitchenObjectSO GetKitchenObjectSOFromIndex (int kitchenObjSOIndex){
        return kitchenObjListSO.kitchenObjectSOsList[kitchenObjSOIndex];
    }

    public void DestroyKitchenObject (KitchenObject kitchenObject) {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc (
        NetworkObjectReference kitchenObjNetworkObjRef
    ) 
    {
        kitchenObjNetworkObjRef.TryGet(out NetworkObject kitchenObjNetworkObj);
        KitchenObject kitchenObject = kitchenObjNetworkObj.GetComponent<KitchenObject>();

        ClearKitchenObjectOnParentClientRpc(kitchenObjNetworkObjRef);
        
        kitchenObject.DestroySelf();
    }

    [ClientRpc]
    private void ClearKitchenObjectOnParentClientRpc 
    (
        NetworkObjectReference kitchenObjNetworkObjRef
    ) 
    {
        kitchenObjNetworkObjRef.TryGet(out NetworkObject kitchenObjNetworkObj);
        KitchenObject kitchenObject = kitchenObjNetworkObj.GetComponent<KitchenObject>();    
    
        kitchenObject.ClearKitchenObjectOnParent();
    }

    public bool IsPlayerIndexConnected(int playerIndex){
        return playerIndex < playerDataNetworkList.Count;
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId){
        for(int i = 0; i < playerDataNetworkList.Count; i++){
            if(playerDataNetworkList[i].clientId == clientId) {
                return i;
            }
        }
        return -1;
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId){
        foreach(PlayerData playerData in playerDataNetworkList){
            if(playerData.clientId == clientId) {
                return playerData;
            }
        }
        return default;
    }

    public PlayerData GetPlayerData(){
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex){
        return playerDataNetworkList[playerIndex];
    }

    public Color GetPlayerColor(int colorId){
        return playerColorList[colorId];
    }

    public void ChangePlayerColor (int colorId) {
        ChangePlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc (int colorId, ServerRpcParams serverRpcParams = default) {
        if(!IsColorAvailable(colorId)) {
            //Color not available
            return;
        }

        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.colorId = colorId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private bool IsColorAvailable(int colorId){
        foreach (PlayerData playerData in playerDataNetworkList){
            if(playerData.colorId == colorId) {
                //Already in use
                return false;
            }
        }
        return true;
    }

    private int GetFirstUnusedColorId(){
        for(int i = 0; i < playerColorList.Count; i++){
            if(IsColorAvailable(i)) {
                return i;
            }
        }
        return -1;
    }

    public void KickPlayer (ulong clientId) {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }

}
