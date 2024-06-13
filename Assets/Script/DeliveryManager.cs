using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;
    private int successfulRecipesAmount;

    private void Awake () {
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update(){
        if(!IsServer){
            return;
        }

        spawnRecipeTimer -= Time.deltaTime;
        if(spawnRecipeTimer <= 0f){
            spawnRecipeTimer = spawnRecipeTimerMax;

            if(KitchenGameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipeMax) {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
                
                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);       
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc (int waitingRecipeSOIndex) {
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];

        waitingRecipeSOList.Add(waitingRecipeSO);    

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);  
    }

    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject) {
        for (int i = 0; i < waitingRecipeSOList.Count; i++){
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if(waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count) {
                //Has the same number of ingredients
                bool plateContentsMatchesRecipe = true;
                foreach(KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList){
                    //Cycling through all ingredients in Recipe
                    bool ingredientFound = false;
                    foreach(KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()){
                        //Cycling through all ingredients in the Plate
                        if(plateKitchenObjectSO == recipeKitchenObjectSO) {
                            //Ingredient matches!
                            ingredientFound = true;
                            break;
                        }
                    }
                    if(!ingredientFound) {
                        //This Recipe ingredient was not found on the Plate
                        plateContentsMatchesRecipe = false;
                    }
                }
                if(plateContentsMatchesRecipe) {
                    //Player delivered the correct recipe!
                    DeliveryCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }
        //No matches found
        //Player did not deliver the correct recipe!
        DeliveryIncorrectRecipeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliveryIncorrectRecipeServerRpc () {
        DeliveryIncorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliveryIncorrectRecipeClientRpc () {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliveryCorrectRecipeServerRpc (int waitingRecipeSOListIndex) {
        DeliveryCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }

    [ClientRpc]
    private void DeliveryCorrectRecipeClientRpc (int waitingRecipeSOListIndex) {
        successfulRecipesAmount ++;

        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);        
    }

    public List<RecipeSO> GetWaitingRecipeSOList(){
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipeAmount(){
        return  successfulRecipesAmount;
    }
}
