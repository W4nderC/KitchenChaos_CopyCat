using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{

    public static event EventHandler OnAnyCut;

    new public static void ResetStaticData(){ 
        OnAnyCut = null; 
    }

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cuttingProgress;

    public override void Interact(Player player)
    {
        if(!HasKitchenObject()) {
            //There is no KitchenObject here
            if(player.HasKitchenObject()) {
                //Player carrying something
                if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
                    //Player carrying something that can be cut
                    KitchenObject kitchenObject = player.GetKitchenObject();                  
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjOnCounterServerRpc();
                }
                
            } else {
                //Player not carrying anything
            }
        } else {
            //There is a KitchenObject here
            if(player.HasKitchenObject()) {
                //Player is carrying something
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    //Player is holding plate
                    if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }         
                }
            } else {
                // Player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjOnCounterServerRpc () {
        InteractLogicPlaceObjOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPlaceObjOnCounterClientRpc () {
        cuttingProgress = 0;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs{
            progressNormalized = 0f
        });        
    }    

    public override void InteractAlternate(Player player)
    {
        if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {
            //There is a KitchenObject here AND it can be cut
            CutObjServerRpc ();
            TestCuttingProgressDoneServerRpc ();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjServerRpc () {
        CutObjClientRpc(); 
    }

    [ClientRpc]
    private void CutObjClientRpc () {
        cuttingProgress++;

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);

        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs{
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });

    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc () {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        if(cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
            KitchenObjectSO ouputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

            KitchenObject.DestroyKitchenObject(GetKitchenObject());

            KitchenObject.SpawnKitchenObject(ouputKitchenObjectSO, this);
        }       
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO){
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;

    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO){
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if(cuttingRecipeSO != null) {
            return cuttingRecipeSO.output;
        } else {
            return null;
        }
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray){
            if(cuttingRecipeSO.input == inputKitchenObjectSO) {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}
