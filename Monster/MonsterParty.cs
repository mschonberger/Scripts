using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterParty : MonoBehaviour
{
    [SerializeField] List<Monster> monsters;

    public List<Monster> Monsters
    {
        get
        {
            return monsters;
        }
    }

    private void Start()
    {
        foreach(var monster in monsters)
        {
            monster.Init();
        }
    }

    public Monster GetHealthyMonster()
    {
        return monsters.Where(x => x.HP > 0).FirstOrDefault(); //Linq-Funktion: Where loopt durch die Monster Liste und gibt alle Elemente wieder die die Konditionen erfüllen
    }

    public void AddMonster(Monster newMonster)
    {
        if (monsters.Count < 6)
        {
            monsters.Add(newMonster);
        }
        else
        {
            //to do: After implementation of a storage system
        }
    }
}
