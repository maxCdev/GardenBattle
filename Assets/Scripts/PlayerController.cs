using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class PlayerController : MonoBehaviour {
    GameController controller;
	void Start () {
        controller = GameController.instance;
        if (controller.GetComponent<GameController>().AI&&tag=="Weed")
        {
            Destroy(GetComponent<PlayerController>());
        }
	}
	void Update () {
	
	}
    void OnMouseDown()
    {
        if (GameController.instance.currentPlayerMoveName == gameObject.tag&&GameController.instance.canInput==true)
        {      
            controller.SendMessage("ChangePlayerUser", transform);
        }
    }
}
