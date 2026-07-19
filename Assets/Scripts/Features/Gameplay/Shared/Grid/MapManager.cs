using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
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
    
    private Map _map;
    private BoundsInt _bounds;

    public Map Current
    {
        get
        {
            if (_map == null)
            {
                InitializeMap();
            }

            return _map;
        }
    }
    
    public void InitializeMap()
    {
        Map map = new();

        // INITIALIZE GRID
        tilemapCollider.CompressBounds();
        _bounds = tilemapCollider.cellBounds;

        map.Width = _bounds.xMax - _bounds.xMin;
        map.Height = _bounds.yMax - _bounds.yMin;
        map.Grid = new Node[map.Width * map.Height];
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
                map.Grid[gridX + gridY * map.Width] = new Node(new Vector2Int(gridX, gridY), type);
            }
        }

        // INITIALIZE SPAWNS
        map.SpawnsRed = new List<Node>();
        map.SpawnsBlue = new List<Node>();
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
                    Node node = map.Grid[gridX + gridY * map.Width];
                    map.SpawnsRed.Add(node);
                }
                if (currentTile.Equals(spawnBlueTile))
                {
                    int gridX = x - _bounds.xMin;
                    int gridY = y - _bounds.yMin;
                    Node node = map.Grid[gridX + gridY * map.Width];
                    map.SpawnsBlue.Add(node);
                }
            }
        }

        _map = map;
    }

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        EnsureInitialized();
        return tilemapSpawns.GetCellCenterLocal(GridToCell(gridPosition));
    }
    
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        EnsureInitialized();
        Vector3Int cellPosition = tilemapCollider.WorldToCell(worldPosition);
        return Vector2Int.CeilToInt(new Vector2(cellPosition.x - _bounds.xMin, cellPosition.y - _bounds.yMin));
    }

    public void SetSpawnMarkersVisible(bool visible)
    {
        tilemapSpawns.gameObject.SetActive(visible);
    }
    
    public void SetSelectableCells(params Node[] nodes)
    {
        SetOverlay(tilemapOverlay1, spawnBlueTile, nodes);
    }
    
    public void SetPreviewCells(params Node[] nodes)
    {
        SetOverlay(tilemapOverlay2, spawnRedTile, nodes);
    }
    
    public void SetBlockedCells(params Node[] nodes)
    {
        SetOverlay(tilemapOverlay3, spawnBlueTile, nodes);
    }

    private void SetOverlay(Tilemap tilemap, TileBase tile, params Node[] nodes)
    {
        EnsureInitialized();
        tilemap.ClearAllTiles();
        Vector3Int[] positions = nodes.Select(n => GridToCell(n.GridPosition)).ToArray();
        TileBase[] tiles = nodes.Select(_ => tile).ToArray();
        tilemap.SetTiles(positions, tiles);
    }

    private Vector3Int GridToCell(Vector2Int gridPosition)
    {
        return Vector3Int.CeilToInt(new Vector3(gridPosition.x + _bounds.xMin, gridPosition.y + _bounds.yMin));
    }

    private void EnsureInitialized()
    {
        if (_map == null)
        {
            InitializeMap();
        }
    }
}
