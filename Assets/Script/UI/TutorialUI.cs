using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyMoveUpTxt;
    [SerializeField] private TextMeshProUGUI keyMoveDownTxt;
    [SerializeField] private TextMeshProUGUI keyMoveLeftTxt;
    [SerializeField] private TextMeshProUGUI keyMoveRightTxt;
    [SerializeField] private TextMeshProUGUI keyInteractTxt;
    [SerializeField] private TextMeshProUGUI keyInteractAltTxt;
    [SerializeField] private TextMeshProUGUI keyPauseTxt;
    [SerializeField] private TextMeshProUGUI keyGamepadInteractTxt;
    [SerializeField] private TextMeshProUGUI keyGamepadInteractAltTxt;
    [SerializeField] private TextMeshProUGUI keyGamepadPauseTxt;


    private void Start() {
        GameInput.Instance.OnBindingRebind += GameInput_OnBindingRebind;
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += KitchenGameManager_OnLocalPlayerReadyChanged;
        UpdateVisual();
        Show();
    }

    private void KitchenGameManager_OnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        if(KitchenGameManager.Instance.IsLocalPlayerReady()) {
            Hide();
        }
    }

    private void GameInput_OnBindingRebind (object sender, EventArgs e) {
        UpdateVisual();
    }

    private void UpdateVisual () {
        keyMoveUpTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
        keyMoveDownTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        keyMoveLeftTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        keyMoveRightTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
        keyInteractTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        keyInteractAltTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact_Alt);
        keyPauseTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);
        keyGamepadInteractTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Interact);
        keyGamepadInteractAltTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_InteractAlt);
        keyGamepadPauseTxt.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Pause);
    }

    private void Show () {
        gameObject.SetActive(true);
    }

    private void Hide () {
        gameObject.SetActive(false);
    }
}
