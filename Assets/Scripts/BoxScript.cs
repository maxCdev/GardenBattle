using UnityEngine;
using System.Collections;

public class BoxScript : MonoBehaviour {

	// Use this for initialization
    bool isClicable = false;
    public bool IsClicable { set { isClicable = value; } get {return isClicable; } }
    void OnMouseDown()
    {
        if (transform.childCount > 0&&GameController.instance.canInput==true)
        {
            Transform clickPlayer = transform.GetChild(0);
            if (GameController.instance.currentPlayerMoveName == clickPlayer.tag)
            {
                GameController.instance.ChangePlayerUser(clickPlayer);
            }
        }
        if (isClicable)
        {
            if (transform.childCount == 0)
            {           
               transform.parent.SendMessage("Move", transform);
            }
          
        }

    }
    void ChangeClicable()
    {
        isClicable = isClicable ? false : true;
    }
}
