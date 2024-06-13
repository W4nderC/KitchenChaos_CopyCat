using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTimerMax = 4f;
    private int plateSpawnAmount;
    private int plateSpawnAmountMax = 4;

    private void Update()
    {
        if(!IsServer) {
            return;
        }
        spawnPlateTimer += Time.deltaTime;
        if(spawnPlateTimer > spawnPlateTimerMax) {
            spawnPlateTimer = 0f;

            if(KitchenGameManager.Instance.IsGamePlaying() && plateSpawnAmount < plateSpawnAmountMax) {
                SpawnPlateServerRpc();
            }
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc () {
        SpawnPlateClientRpc();
    }

    [ClientRpc]
    private void SpawnPlateClientRpc () {
        plateSpawnAmount ++;

        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override void Interact (Player player) {
        if(!player.HasKitchenObject()) {
            //Player is empty handed
            if(plateSpawnAmount > 0) {
                //There's at least plate here
                

                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);

                InteractLogicServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc () {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc () {
        plateSpawnAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
