using UnityEngine;

public enum NodeType : byte
{
    Invalid,
    Empty,
    Ground,
    Wall
}

public readonly struct Node
{
    public static readonly Node Invalid = new(Vector2Int.zero, NodeType.Invalid);
    
    public readonly Vector2Int GridPosition;
    public readonly NodeType NodeType;

    public Node(Vector2Int gridPosition, NodeType nodeType)
    {
        GridPosition = gridPosition;
        NodeType = nodeType;
    }
}