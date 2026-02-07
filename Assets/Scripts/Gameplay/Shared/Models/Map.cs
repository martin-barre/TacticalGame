using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class Map
{
    public Node[] Grid;
    public List<Node> SpawnsRed;
    public List<Node> SpawnsBlue;
    public int Width;
    public int Height;
    
    public Node GetNode(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= Width || gridPosition.y < 0 || gridPosition.y >= Height) return Node.Invalid;
        return Grid[gridPosition.x + gridPosition.y * Width];
    }
    
    public Node GetNode(int cellId)
    {
        if (cellId < 0 || cellId >= Grid.Length) return Node.Invalid;
        return Grid[cellId];
    }
    
    public Node GetNode(Vector3 worldPosition)
    {
        Vector2Int gridPosition = MapManager.Instance.WorlPositionToGridPosition(worldPosition);
        return GetNode(gridPosition);
    }
    
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        return MapManager.Instance.WorlPositionToGridPosition(worldPosition);
    }
    
    public bool IsWalkable(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= Width || gridPosition.y < 0 || gridPosition.y >= Height) return false;
        Node node = Grid[gridPosition.x + gridPosition.y * Width];
        return node is { NodeType: NodeType.Ground };
    }
    
    public bool IsWalkable(int cellId)
    {
        if (cellId < 0 || cellId >= Grid.Length) return false;
        return Grid[cellId] is { NodeType: NodeType.Ground };
    }
}
