using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class Enemy
{
    public enum Type
    {
        NORMAL,
        ELITE,
        BOSS
    }
    public GameObject prefab;
    public Type type;
    public int goldValue;
}

public class EnemyManager : MonoBehaviour
{
    public List<Enemy> enemyList = new();

    public List<Enemy> GetEnemyByRarity(Enemy.Type type)
    {
        return enemyList.Where(enemy => enemy.type == type).ToList();
    }
}
