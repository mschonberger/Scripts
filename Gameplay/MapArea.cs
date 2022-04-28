using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Monster> wildMonsters;

    public Monster GetRandomWildMonster()
    {
        var wildMonster =  wildMonsters[Random.Range(0, wildMonsters.Count)];
        wildMonster.Init();
        return wildMonster;
    }

    /*[Header("Monster Settings")]
    [SerializeField]
    [Tooltip("The level field can be ignored!")]
    List<Monster> specificMonsters;

    [SerializeField] List<MonsterType> allowedMonsterTypes;

    [Header("Level Settings")]
    [Tooltip("Default level range for this map")]
    [SerializeField] Vector2Int levelRange;

    [Tooltip("Allow the wild Monster to level up like the player does")]
    [SerializeField] bool wildMonsterDoLevelUp = true;

    [Tooltip("Divisor to calculate the min level of the wild Monster")]
    [SerializeField] int playerLevelDivisor = 2;

    public List<Monster> WildMonsters { get; set; }

    int oldPlayerMonsterMaxLevel = 1;
    int activePlayerMonsterMaxLevel = 0;

    public Monster GetRandomWildMonster()
    {
        Monster wildMonster = null;

        // check if we have a list of pokemon types
        if (allowedMonsterTypes.Count > 0)
        {
            // get a random pokemon from our types list
            wildMonster = WildMonsters[Random.Range(0, WildMonsters.Count)];
            wildMonster.Init();
        }
        else if (specificMonsters.Count > 0)
        {
            // get a pokemon from our specific pokemon list
            wildMonster = specificMonsters[Random.Range(0, specificMonsters.Count)];
            // get a random level in the given range
            wildMonster.Level = GetWildMonsterLevel();
            wildMonster.Init();
        }

        return wildMonster;
    }

    public List<Monster> BuildMonsterList()
    {
        // check if the level of the highest pokemon has changed
        if (oldPlayerMonsterMaxLevel != activePlayerMonsterMaxLevel)
        {
            // get the pokemon party of the player
            MonsterParty party = MonsterParty.GetPlayerParty();

            if (party.Monsters.Count > 0)
            {
                // find the pokemon with the highest level
                activePlayerMonsterMaxLevel = party.Monsters.Max(p => p.Level);
                // set the old level to the new one so that we don't need 
                // to get this again later
                oldPlayerMonsterMaxLevel = activePlayerMonsterMaxLevel;
            }
        }

        // get pokemon db
        var pokemonDb = MonsterDB.GetObjectDB();

        // run through the pokemon db and get all pokemons
        // which are of the allowed allowedPokemonTypes
        var availablePokemon = pokemonDb.Where(
            (p) =>
                allowedMonsterTypes.Contains(p.Value.Type1) ||
                allowedMonsterTypes.Contains(p.Value.Type2)
        );

        // create a new list of wild pokemons
        List<Monster> listOfWildPokemons = new List<Monster>();
        foreach (var keyValuePair in availablePokemon)
        {
            // get a random level in the given range
            int wildLevel = GetWildMonsterLevel();

            // add the new pokemon to the list of wild pokemon for
            // this map
            listOfWildPokemons.Add(
                new Monster(keyValuePair.Value, wildLevel)
            );
        }

        return listOfWildPokemons;
    }

    int GetWildMonsterLevel()
    {
        int wildLevel;

        // check if we allow wild pokemon to level up and if a pokemon of the
        // player has a higher level as the max default level of the map
        if (wildMonsterDoLevelUp && activePlayerMonsterMaxLevel > levelRange.y)
        {
            wildLevel = Random.Range(
                // use as default the half of the max level as the new min level
                Mathf.FloorToInt(activePlayerMonsterMaxLevel / playerLevelDivisor),
                // the max level of the highest pokemon will be the new max level
                activePlayerMonsterMaxLevel
            );
        }
        else
        {
            wildLevel = Random.Range(levelRange.x, levelRange.y);
        }

        return wildLevel;
    }*/
}
