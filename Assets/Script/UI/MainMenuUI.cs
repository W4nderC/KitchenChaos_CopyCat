using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button multipLayerPlayButton;
    [SerializeField] private Button singlepLayerPlayButton;
    [SerializeField] private Button quitButton;
    

    private void Awake() {
        multipLayerPlayButton.onClick.AddListener(() => {
            KitchenGameMultiplayer.playMultiplayer = true;
            Loader.Load(Loader.Scene.LobbyScene);
        });
        singlepLayerPlayButton.onClick.AddListener(() => {
            KitchenGameMultiplayer.playMultiplayer = false;
            Loader.Load(Loader.Scene.LobbyScene);
        });
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });

        Time.timeScale = 1f;
    }
}
