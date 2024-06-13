using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createBtn;
    [SerializeField] private Button joinBtn;


    private void Awake() {
        createBtn.onClick.AddListener(() => {
            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);

        });
        joinBtn.onClick.AddListener(() => {
            KitchenGameMultiplayer.Instance.StartClient();
        });
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
