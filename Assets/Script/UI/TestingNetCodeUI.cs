using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetCodeUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;


    private void Awake() {
        startHostButton.onClick.AddListener(() => {
            Debug.Log("Host");
            KitchenGameMultiplayer.Instance.StartHost();
            Hide();
        });
        startClientButton.onClick.AddListener(() => {
            Debug.Log("Client");
            KitchenGameMultiplayer.Instance.StartClient();
            Hide();
        });
    }

    private void Hide () {
        gameObject.SetActive(false);
    }
}
