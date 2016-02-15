using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public bool AI;
    public AiLevel Level;
	public static GameManager instance;
	void Start () {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Level = AiLevel.Hard;
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
	}
	
	// Update is called once per frame
    public void Start1Player()
    {
        AI = true;
        SceneManager.LoadScene("Game");
    }
    public void Start2Players()
    {
        AI = false;
        SceneManager.LoadScene("Game");
    }
}
