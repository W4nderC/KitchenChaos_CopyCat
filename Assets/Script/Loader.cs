using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene {
        GameMenuScene,
        GameScene,
        LoadingScene,
        LobbyScene,
        CharacterSelectScene
    }


    public static Scene targetScene;

    public static void Load(Scene targetSceneName){
        Loader.targetScene = targetSceneName;
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadNetwork(Scene targetSceneName){

        NetworkManager.Singleton.SceneManager.LoadScene(targetSceneName.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback () {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
