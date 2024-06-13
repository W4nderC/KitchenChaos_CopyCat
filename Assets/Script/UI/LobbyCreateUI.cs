using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button createPublicBtn;
    [SerializeField] private Button createPrivateBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TMP_InputField lobbyNameInputField;


    private void Awake() {
        createPublicBtn.onClick.AddListener(() => {
            KitchenGameLobby.Instance.CreateLobby(lobbyNameInputField.text, false);
        });
        createPrivateBtn.onClick.AddListener(() => {
            KitchenGameLobby.Instance.CreateLobby(lobbyNameInputField.text, true);
        });
        closeBtn.onClick.AddListener(() => {
            Hide();
        });
    }

    private void Start() {
        Hide();
    }

    public void Show () {
        gameObject.SetActive(true);
    }

    private void Hide () {
        gameObject.SetActive(false);
    }  
}
