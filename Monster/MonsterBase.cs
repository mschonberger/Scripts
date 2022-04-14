using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new Monster")]

public class MonsterBase : ScriptableObject
{

#pragma warning disable CS0108 // Element blendet vererbte Element aus; fehlendes 'new'-Schlüsselwort
    [SerializeField] string name;
#pragma warning restore CS0108 // Element blendet vererbte Element aus; fehlendes 'new'-Schlüsselwort

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite sprite;
    [SerializeField] Sprite faceSprite;

    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    
    [SerializeField] int xpGain;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate; //between 0 and 255. higher = easier

    [SerializeField] List<LearnableMove> learnableMoves;
    
    public int GetXPForLevel (int level) 
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level ) / 5;
        } else if (growthRate == GrowthRate.MediumFast) 
        {
            return level * level * level;
        } else if (growthRate == GrowthRate.MediumSlow)
        {
            return 6 * (level * level * level) / 5 - 15 * (level * level) + 100 * level - 140;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return 5 * (level * level * level) / 4;
        }
        else if (growthRate == GrowthRate.Fluctuating)
        {
            return GetFluctuating(level);
        }
        return -1;
    }
    
    public int GetFluctuating(int level)
    {
        if (level < 15)
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor((level + 1) / 3) + 24) / 50));
        }
        else if (level >= 15 && level < 36)
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level + 14) / 50));
        }
        else
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor(level / 2) + 32) / 50));
        }
    }

    public string Name {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite Sprite
    {
        get { return sprite; }
    }

    public Sprite FaceSprite
    {
        get { return faceSprite; }
    }

    public MonsterType Type1
    {
        get { return type1; }
    }

    public MonsterType Type2
    {
        get { return type2; }
    }

    public int MaxHP
    {
        get { return maxHP; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
    
    //Shorter way of writing
    public int CatchRate => catchRate;
    
    public int XPGain => xpGain;
    public GrowthRate GrowthRate => growthRate;
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

public enum MonsterType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

public enum GrowthRate 
{
    Fast,
    MediumFast,
    MediumSlow,
    Slow
    Fluctuating
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    //Diese beiden sind keine Stats im eigentlichen Sinne
    Accuracy,
    Evasion
}

public class TypeChart
{
    static float[][] chart = {
    //                        Nor   Fir   Wat   Ele   Gra   Ice   Fig   Poi   Gro   Fly   Psy   Bug   Roc   Gho   Dra    Dar    Ste    Fai
    /*Normal*/  new float[] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f,   0f,   1f,   1f,  0.5f,   1f },
    /*Fire*/    new float[] { 1f, 0.5f, 0.5f,   1f,   2f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f,   1f, 0.5f,   1f,    2f,   1f },
    /*Water*/   new float[] { 1f,   2f, 0.5f,   1f, 0.5f,   1f,   1f,   1f,   2f,   1f,   1f,   1f,     2f,   1f, 0.5f,   1f,    1f,   1f },
    /*Electric*/new float[] { 1f,   1f,   2f, 0.5f, 0.5f,   1f,   1f,   1f,   0f,   2f,   1f,   1f,     1f,   1f, 0.5f,   1f,    1f,   1f },
    /*Grass*/   new float[] { 1f, 0.5f,   2f,   1f, 0.5f,   1f,   1f, 0.5f,   2f, 0.5f,   1f, 0.5f,     2f,   1f, 0.5f,   1f,  0.5f,   1f },
    /*Ice*/     new float[] { 1f, 0.5f, 0.5f,   1f,   2f, 0.5f,   1f,   1f,   2f,   2f,   1f,   1f,     1f,   1f,   2f,   1f,  0.5f,   1f },
    /*Fighting*/new float[] { 2f,   1f,   1f,   1f,   1f,   2f,   1f, 0.5f,   1f, 0.5f, 0.5f, 0.5f,     2f,   0f,   1f,   2f,    2f, 0.5f },
    /*Poison*/  new float[] { 1f,   1f,   1f,   1f,   2f,   1f,   1f, 0.5f, 0.5f,   1f,   1f,   1f,   0.5f, 0.5f,   1f,   1f,    0f,   2f },
    /*Ground*/  new float[] { 1f,   2f,   1f,   2f, 0.5f,   1f,   1f,   2f,   1f,   0f,   1f, 0.5f,     2f,   1f,   1f,   1f,    2f,   1f },
    /*Flying*/  new float[] { 1f,   1f,   1f, 0.5f,   2f,   1f,   2f,   1f,   1f,   1f,   1f,   2f,   0.5f,   1f,   1f,   1f,  0.5f,   1f },
    /*Psychic*/ new float[] { 1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   1f,   1f, 0.5f,   1f,     1f,   1f,   1f,   0f,  0.5f,   1f },
    /*Bug*/     new float[] { 1f, 0.5f,   1f,   1f,   2f,   1f, 0.5f, 0.5f,   1f, 0.5f,   2f,   1f,     1f, 0.5f,   1f,   2f,  0.5f, 0.5f },
    /*Rock*/    new float[] { 1f,   2f,   1f,   1f,   1f,   2f, 0.5f,   1f, 0.5f,   2f,   1f,   2f,     1f,   1f,   1f,   1f,  0.5f,   1f },
    /*Ghost*/   new float[] { 0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f, 0.5f,   1f,     1f,   2f,   1f, 0.5f,    1f,   1f },
    /*Dragon*/  new float[] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,     1f,   1f,   2f,   1f,  0.5f,   0f },
    /*Dark*/    new float[] { 1f,   1f,   1f,   1f,   1f,   1f, 0.5f,   1f,   1f,   1f,   2f,   1f,     1f,   2f,   1f, 0.5f,    1f, 0.5f },
    /*Steel*/   new float[] { 1f, 0.5f, 0.5f, 0.5f,   1f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f,   1f,   1f,   1f,  0.5f,   2f },
    /*Fairy*/   new float[] { 1f, 0.5f,   1f,   1f,   1f,   1f,   2f, 0.5f,   1f,   1f,   1f,   1f,     1f,   1f,   2f,   2f,  0.5f,   1f }
    };

public static float GetEffectiveness(MonsterType attackType, MonsterType defenseType)
    {
        if (attackType == MonsterType.None || defenseType == MonsterType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
