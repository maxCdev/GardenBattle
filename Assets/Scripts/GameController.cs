using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameController : MonoBehaviour {
    [SerializeField] int fieldLength = 8;
    public bool AI = false;
    Transform[] prevTileSelect=null;
    Transform[] prevClickTiles = null;
    Material wallMath;
    Material matSelect;
    string winner = null;
    public GameObject endGamePanel;
    public Text winnerText;
    public Text flowersCount;
    public Text weedsCount;
    public bool canInput = true;
    public string currentPlayerMoveName;//static 19.12.2015
    Transform level;
    public Transform selectedPlayer;
    List<Transform> tiles=new List<Transform>();
    public static GameController instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Error: Game Conreoller not one!");
        }
    }
	void Start () {
        AI = GameManager.instance.AI;
        matSelect = Resources.Load<Material>("materials/SelectWall");
        wallMath = Resources.Load<Material>("materials/wall");
        BuildLevel();
        currentPlayerMoveName = "Flower";
        level = transform;
        string end = (fieldLength - 1).ToString();
        for (int i = 0; i < level.childCount; i++)
        {
            Transform tile = level.GetChild(i);
            if (tile.name.StartsWith("0:0") || tile.name.StartsWith("0:"+end))
            {
                CreatePlayer(tile, "Flower");
            }
            if (tile.name.StartsWith(end+":0") || tile.name.StartsWith(end+":"+end))
            {
                CreatePlayer(tile, "Weed");
            }
            tiles.Add(tile);
        }
        UpdateStatistic();
        if (AI)
        {
            GetComponent<AI>().enabled = true;
        }
	}
    void BuildLevel()
    {
        GameObject prefub = null;
        for (int i = 0; i < fieldLength; i++)
            for (int j = 0; j < fieldLength; j++)
            {
                prefub = Resources.Load<GameObject>("prefubs/box");
                prefub.name = i + ":" + j;
                prefub = Instantiate(prefub, new Vector3(i, 0, j), new Quaternion()) as GameObject;
                prefub.transform.parent = transform;
            }
    }
    void Select()
    {
        if (selectedPlayer != null)
        {
            selectedPlayer.GetComponentInChildren<Animator>().Play("Idle");
        }
        if (prevClickTiles != null)
        {
            for (int i = 0; i < prevClickTiles.Length; i++)
            {
                prevClickTiles[i].SendMessage("ChangeClicable");
            }
            prevClickTiles = null;
        }
        if (prevTileSelect != null)
        {
            for (int i = 0; i < prevTileSelect.Length; i++)
            {
                prevTileSelect[i].GetComponent<Renderer>().material = wallMath;
            }
            prevTileSelect = null;
        }
    }
    void Move(Transform tile)
    {
        Select();
        canInput = false;
        if (selectedPlayer!=null)
        {
            if (Vector3.Distance(tile.position, selectedPlayer.parent.position) < 1.5f)
            {             
                StartCoroutine(CreatePlayerCourutine(tile, selectedPlayer.name.Replace("(Clone)", "")));
                

            }
            else
            {
                    Animator anim = (selectedPlayer.GetComponentInChildren(typeof(Animator)) as Animator);
                    StartCoroutine(PlayerDown(anim, tile));
             
            }
        }
    }
    IEnumerator PlayerDown(Animator anim, Transform tile)
    {
        anim.Play("Down");
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        selectedPlayer.position = tile.position + Vector3.up;
        selectedPlayer.parent = tile;
        StartCoroutine(PlayerUp(tile, anim));
    }
    IEnumerator PlayerUp(Transform tile, Animator anim)
    {
        anim.Play("Up");
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Asimilation(tile.position);
        UpdateStatistic();
        MoveComplite();
    }
    IEnumerator CreatePlayerCourutine(Transform tile, string prefubName)
    {
        GameObject prefub = Resources.Load<GameObject>("prefubs/" + prefubName);
        prefub = Instantiate(prefub, tile.position + Vector3.up, new Quaternion()) as GameObject;
        prefub.transform.parent = tile;
        Animator anim = (prefub.GetComponentInChildren(typeof(Animator)) as Animator);
        anim.Play("Up");
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Asimilation(tile.position);
        UpdateStatistic();
        MoveComplite();

    }
    void CreatePlayer(Transform tile,string prefubName,bool asim=false)
    {
        GameObject prefub = Resources.Load<GameObject>("prefubs/"+prefubName);
        prefub = Instantiate(prefub, tile.position + Vector3.up, new Quaternion()) as GameObject;
        prefub.transform.parent = tile;
        if (asim)
        {
            (prefub.GetComponentInChildren(typeof(Animator)) as Animator).Play("Asim");
        }
        else
        {
            (prefub.GetComponentInChildren(typeof(Animator)) as Animator).Play("Up");
        }
    }
    //AI
    void ChangePlayer(Transform newSelectedPlayer)
    {
        selectedPlayer = newSelectedPlayer;
    }
    //user
     public void ChangePlayerUser(Transform newSelectedPlayer)
    {
        if (newSelectedPlayer == selectedPlayer)
        {
            return;
        }
        Select();
        Transform[] select = tiles.Where(a => Vector3.Distance(newSelectedPlayer.parent.position, a.transform.position) < 3f).ToArray();
        for (int i = 0; i < select.Length; i++)
        {
            select[i].GetComponent<Renderer>().material = matSelect;
        }
        newSelectedPlayer.parent.GetComponent<Renderer>().material = wallMath;
        prevTileSelect = select;
        newSelectedPlayer.GetComponentInChildren<Animator>().Play("Select");
        selectedPlayer = newSelectedPlayer;
        
        Transform[] clicableTiles = select.Where(a => a.childCount == 0).ToArray();
        if (clicableTiles != null)
        {
            for (int i = 0; i < clicableTiles.Length; i++)
            {
                clicableTiles[i].SendMessage("ChangeClicable");
            }
        }
        prevClickTiles = clicableTiles;
    }
    void MoveComplite()
    {
        currentPlayerMoveName = currentPlayerMoveName == "Flower" ? "Weed" : "Flower";
        if (currentPlayerMoveName == "Weed"&&AI==true&&winner==null)
        {
            transform.SendMessage("AIMove");
        }
        canInput = true;
    }
    void Asimilation(Vector3 newPosition)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(currentPlayerMoveName == "Flower" ? "Weed" : "Flower")
            .Where(a => Vector3.Distance(a.transform.parent.position, newPosition) < 1.5f).ToArray();
        Transform tile;
               for (int i=0;i<enemies.Length;i++)
               {
                   tile = enemies[i].transform.parent;
                   enemies[i].SetActive(false);
                   Destroy(enemies[i]);
                   CreatePlayer(tile,currentPlayerMoveName,true);
                     
               }
    }
    void UpdateStatistic()
    {
        GameObject[] flowers = GameObject.FindGameObjectsWithTag("Flower");
        GameObject[] weeds = GameObject.FindGameObjectsWithTag("Weed");
        flowersCount.text = "Flowers: " + flowers.Length;
        weedsCount.text = "Weeds: " + weeds.Length;
        if (flowers.Length==0||weeds.Length==0)
        {
            winner = flowers.Length==0?"Weeds":"Flowers";
        }
        if (flowers.Length + weeds.Length == fieldLength * fieldLength)
        {
            if (flowers.Length == weeds.Length)
            {
                winner = "drawn game";
            }
            else
            {
                winner = flowers.Length < weeds.Length ? "Weeds" : "Flowers";
            }
        }
        if (winner != null)
        {
            EndGame();
        }
    }
    public void GiveUp()
    {
        if (AI && currentPlayerMoveName=="Flowers")
        {
            winner = "Weeds";
        }
        else 
        {
            winner = currentPlayerMoveName == "Weed" ? "Flowers" : "Weeds";
            EndGame();
        }
    }
    void EndGame()
    {
        canInput = false;
        winnerText.text = winner + " win!!!";
        endGamePanel.SetActive(true);
    }
    public void ReloedGame()
    {
        SceneManager.LoadScene("Game");
    }
    public void BackMenu()
    {
        Debug.Log("fuck your self");
        SceneManager.LoadScene("Menu");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
