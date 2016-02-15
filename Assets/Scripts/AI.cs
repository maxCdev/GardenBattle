using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
public enum AiLevel {Easy,Middle,Hard}
[RequireComponent(typeof(GameController))]
public class AI : MonoBehaviour {

    List<Transform> tiles=new List<Transform>();
    List<GameObject> myUnits = null;
    List<GameObject> enemies = null;
    public AiLevel level;
	void Start () {
        level = GameManager.instance.Level;
        for (int i = 0; i < transform.childCount; i++)
        {
           tiles.Add(transform.GetChild(i));
        }
	}
    void AIMove()
    {
        GetMyUnits();
        GetEnemies();
        ArrayList attackMove=DetectTileMove();
        if (attackMove == null)
        {
            SendMessage("GiveUp");
        }
        Transform start;
        Transform goal;

        start = (attackMove[0] as Transform);
        goal = (attackMove[1] as Transform);


        transform.SendMessage("ChangePlayer", start.GetChild(0));
        transform.SendMessage("Move",goal);
    }
    int GetAsimilationCount(Vector3 newPosition)
    {
        GameObject[] enemiesAsim = enemies.Where(a => Vector3.Distance(a.transform.parent.position, newPosition) < 1.6f).ToArray();
        return enemiesAsim.Length;
    }
    void TakeMoves(ref List<Transform> moves,bool checkFoolMove=false)
    {
        float maxAsim=0;
        foreach (var unit in myUnits)
        {
            List<Transform> unitMoves = GetSelectTiles(unit.transform.parent);
            if (unitMoves == null || unitMoves.Count == 0)
            {
                continue;
            }
            maxAsim = unitMoves.Max(a => GetAsimilationCount(a.position));

            foreach (var bestTile in unitMoves.Where(a => GetAsimilationCount(a.position) == maxAsim).ToArray())
            {
                if (checkFoolMove)//проверять или не проверять на дурацкий ход
                {
                    if (!FoolMove(bestTile, maxAsim))
                    {
                        moves.Add(bestTile);
                    }
                }
                else
                {
                    moves.Add(bestTile);
                }
            }
        }
    }
    ArrayList DetectTileMove()
    {
#region Detect Best Moves
        List<Transform> moves = new List<Transform>();
        TakeMoves(ref moves, true);
        if (moves.Count == 0)  
        {
            TakeMoves(ref moves);
            if (moves.Count == 0)
            {
                return null;
            }
        }
        moves = moves.Distinct().ToList();
        float maxAsim =  moves.Max(a => GetAsimilationCount(a.position));
        moves = moves.Where(a => GetAsimilationCount(a.position) == maxAsim).ToList();
#endregion
#region Create Dictionary Move/Unit
        Dictionary<Transform, Transform> targetTiles = new Dictionary<Transform, Transform>();
        float nearUnitCost;
        foreach (var move in moves)
        {
            nearUnitCost = myUnits.Min(a =>Vector3.Distance(move.position, a.transform.parent.position));
            Transform bestUnit=myUnits.First(a=>Vector3.Distance(a.transform.parent.position,move.position)==nearUnitCost).transform.parent;
            targetTiles.Add(move, bestUnit);
        }
#endregion
#region Best Unit/Best Move
        Transform resumeTarget=null;
        switch (level)//принятие решения соответственно уровню
        {
            case AiLevel.Easy:{
                resumeTarget = targetTiles.FirstOrDefault(a=>Vector3.Distance(a.Key.position, a.Value.position)<1.6f).Key;
                if (resumeTarget == null)
                {
                    resumeTarget = targetTiles.First().Key;
                }
                }break;

            case AiLevel.Middle: {
                nearUnitCost = targetTiles.Min(a => Vector3.Distance(a.Key.position, a.Value.position));
                resumeTarget = targetTiles.First(a => Vector3.Distance(a.Key.position, a.Value.position) == nearUnitCost).Key;
            } break;

            case AiLevel.Hard:{                    
                nearUnitCost = targetTiles.Min(a => Vector3.Distance(a.Key.position, a.Value.position));
                int minAroundUnit = targetTiles.Where(a => Vector3.Distance(a.Key.position, a.Value.position) == nearUnitCost)
                    .Min(a => AroundUnitCount(a.Value));
                resumeTarget = targetTiles.First(a => Vector3.Distance(a.Key.position, a.Value.position) == nearUnitCost 
                    && AroundUnitCount(a.Value) == minAroundUnit).Key;
            } break;
        }           
        return new ArrayList(){targetTiles[resumeTarget], resumeTarget};
#endregion
    }
    bool FoolMove(Transform resumeTarget, float maxAsim)
    {
        if (resumeTarget != null && maxAsim >0&& maxAsim < 3&&enemies.Count+myUnits.Count<15)
        {
            List<GameObject> skipEnemie = enemies.Where(a => Vector3.Distance(a.transform.parent.position, resumeTarget.position) < 1.6f).Select(a=>a.gameObject).ToList();
            List<Transform> otherEnemie = enemies.Except(skipEnemie).Select(a => a.transform).ToList();

            for (int i = 0; i < otherEnemie.Count; i++)
			{
			    if (GetSelectTiles(otherEnemie[i].parent).FirstOrDefault(a=>
                {
                    if (maxAsim == 1)
                    {
                        return GetAroundTiles(skipEnemie[0].transform.parent.transform).Contains(a) &&
                            GetAroundTiles(resumeTarget).Contains(a);
                    }
                    else
                    {
                        return GetAroundTiles(skipEnemie[0].transform.parent.transform).Contains(a) &&
                            GetAroundTiles(skipEnemie[1].transform.parent.transform).Contains(a)&&
                            GetAroundTiles(resumeTarget).Contains(a);
                    }
                }
                )!=null)
                {
                    return true;
                }
			}

            
        }
        return false;
    }
    List<Transform> GetAroundTiles(Transform tile)
    {
        return tiles.Where(a => Vector3.Distance(a.transform.position, tile.position) < 1.6f).ToList();
    }
    int AroundUnitCount(Transform unit)
    {
        GameObject[] around=myUnits.Where(a => Vector3.Distance(a.transform.parent.position, unit.position) < 1.6f).ToArray();
        return around.Length;
    }
    List<Transform> GetSelectTiles(Transform tile)
    {
        return tiles.Where(a => Vector3.Distance(tile.position, a.position) < 3f&&a.childCount==0).ToList();
    }
    void GetMyUnits()
    {
        myUnits = GameObject.FindGameObjectsWithTag("Weed").ToList();
    }
    void GetEnemies()
    {
        enemies = GameObject.FindGameObjectsWithTag("Flower").ToList();
    }
}
