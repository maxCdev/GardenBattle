using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

    public GameObject levelMenu;
    public void OnePlayer()
    {
        levelMenu.SetActive(true);
        
    }
    public void TwoPlayers()
    {
        GameManager.instance.Start2Players();
    }

    public void EasyAi()
    {
        GameManager.instance.Level = AiLevel.Easy;
        GameManager.instance.Start1Player();
    }
    public void MiddleAi()
    {
        GameManager.instance.Level = AiLevel.Easy;
        GameManager.instance.Start1Player();
    }
    public void HardAi()
    {
        GameManager.instance.Level = AiLevel.Hard;
        GameManager.instance.Start1Player();
    }
    public void Exit()
    {
        Application.Quit();
    }
}
