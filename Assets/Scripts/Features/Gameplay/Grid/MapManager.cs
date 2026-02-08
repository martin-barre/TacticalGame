using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoSingleton<MapManager>
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap tilemapCollider;
    [SerializeField] private Tilemap tilemapSpawns;
    [SerializeField] private Tilemap tilemapOverlay1;
    [SerializeField] private Tilemap tilemapOverlay2;
    [SerializeField] private Tilemap tilemapOverlay3;

    [Header("Tiles")]
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase spawnRedTile;
    [SerializeField] private TileBase spawnBlueTile;
    
    private readonly Map _map = new();
    private BoundsInt _bounds;
    
    public void InitializeMap()
    {
        // INITIALIZE GRID
        tilemapCollider.CompressBounds();
        _bounds = tilemapCollider.cellBounds;

        _map.Width = _bounds.xMax - _bounds.xMin;
        _map.Height = _bounds.yMax - _bounds.yMin;
        _map.Grid = new Node[_map.Width * _map.Height];
        for (int y = _bounds.yMin; y < _bounds.yMax; y++)
        {
            for (int x = _bounds.xMin; x < _bounds.xMax; x++)
            {
                TileBase currentTile = tilemapCollider.GetTile(Vector3Int.CeilToInt(new Vector3(x, y)));

                NodeType type = NodeType.Empty;
                if (currentTile == null) type = NodeType.Empty;
                else if (currentTile.Equals(groundTile)) type = NodeType.Ground;
                else if (currentTile.Equals(wallTile)) type = NodeType.Wall;

                int gridX = x - _bounds.xMin;
                int gridY = y - _bounds.yMin;
                _map.Grid[gridX + gridY * _map.Width] = new Node(new Vector2Int(gridX, gridY), type);
            }
        }

        // INITIALIZE SPAWNS
        _map.SpawnsRed = new List<Node>();
        _map.SpawnsBlue = new List<Node>();
        for (int y = _bounds.yMin; y < _bounds.yMax; y++)
        {
            for (int x = _bounds.xMin; x < _bounds.xMax; x++)
            {
                TileBase currentTile = tilemapSpawns.GetTile(Vector3Int.CeilToInt(new Vector2(x, y)));
                if (currentTile == null) continue;
                if (currentTile.Equals(spawnRedTile))
                {
                    int gridX = x - _bounds.xMin;
                    int gridY = y - _bounds.yMin;
                    Node node = _map.Grid[gridX + gridY * _map.Width];
                    _map.SpawnsRed.Add(node);
                }
                if (currentTile.Equals(spawnBlueTile))
                {
                    int gridX = x - _bounds.xMin;
                    int gridY = y - _bounds.yMin;
                    Node node = _map.Grid[gridX + gridY * _map.Width];
                    _map.SpawnsBlue.Add(node);
                }
            }
        }
    }

    public Map GetMap()
    {
        return new Map
        {
            Grid = _map.Grid,
            SpawnsRed = _map.SpawnsRed,
            SpawnsBlue = _map.SpawnsBlue,
            Width = _map.Width,
            Height = _map.Height
        };
    }

    public Vector3 GridPositionToWorlPosition(Vector2Int gridPosition)
    {
        return tilemapSpawns.GetCellCenterLocal(Vector3Int.CeilToInt(new Vector3(gridPosition.x + _bounds.xMin, gridPosition.y + _bounds.yMin)));
    }
    
    public Vector2Int WorlPositionToGridPosition(Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemapCollider.WorldToCell(worldPosition);
        return Vector2Int.CeilToInt(new Vector2(cellPosition.x - _bounds.xMin, cellPosition.y - _bounds.yMin));
    }

    public void ActiveTilemapSpawns(bool active)
    {
        tilemapSpawns.gameObject.SetActive(active);
    }
    
    public void SetOverlay1(params Node[] nodes)
    {
        tilemapOverlay1.ClearAllTiles();
        Vector3Int[] positions = nodes.Select(n => new Vector3Int(n.GridPosition.x + _bounds.xMin, n.GridPosition.y + _bounds.yMin, 0)).ToArray();
        TileBase[] tiles = nodes.Select(n => spawnBlueTile).ToArray();
        tilemapOverlay1.SetTiles(positions, tiles);
    }
    
    public void SetOverlay2(params Node[] nodes)
    {
        tilemapOverlay2.ClearAllTiles();
        Vector3Int[] positions = nodes.Select(n => new Vector3Int(n.GridPosition.x + _bounds.xMin, n.GridPosition.y + _bounds.yMin, 0)).ToArray();
        TileBase[] tiles = nodes.Select(n => spawnRedTile).ToArray();
        tilemapOverlay2.SetTiles(positions, tiles);
    }
    
    public void SetOverlay3(params Node[] nodes)
    {
        tilemapOverlay3.ClearAllTiles();
        Vector3Int[] positions = nodes.Select(n => new Vector3Int(n.GridPosition.x + _bounds.xMin, n.GridPosition.y + _bounds.yMin, 0)).ToArray();
        TileBase[] tiles = nodes.Select(n => spawnBlueTile).ToArray();
        tilemapOverlay3.SetTiles(positions, tiles);
    }
}
