using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity
{
    public int Id;
    public Team Team;
    public Race Race;
    public Vector2Int GridPosition;
    public int Hp;
    public int Pa;
    public int Pm;
    public bool IsPlayer;
    public Entity Summoner;
    public List<ActiveBuff> Buffs = new();

    public Entity Clone()
    {
        return new Entity
        {
            Id = Id,
            Team = Team,
            Race = Race,
            GridPosition = GridPosition,
            Hp = Hp,
            Pa = Pa,
            Pm = Pm,
            IsPlayer = IsPlayer,
            Summoner = Summoner?.Clone(),
            Buffs = Buffs.ToList()
        };
    }
}
