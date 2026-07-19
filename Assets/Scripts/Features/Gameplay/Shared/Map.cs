using System.Collections.Generic;
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
    
    public bool IsOpaque(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= Width || gridPosition.y < 0 || gridPosition.y >= Height) return false;
        Node node = Grid[gridPosition.x + gridPosition.y * Width];
        return node is { NodeType: NodeType.Wall };
    }
    
    public bool IsOpaque(int cellId)
    {
        if (cellId < 0 || cellId >= Grid.Length) return false;
        return Grid[cellId] is { NodeType: NodeType.Wall };
    }
}
