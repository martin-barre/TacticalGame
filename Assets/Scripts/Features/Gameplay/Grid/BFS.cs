using System;
using System.Collections.Generic;
using UnityEngine;

public static class BFS
{
    public static List<Node> GetPath(Vector2Int startPos, Vector2Int endPos, GameState gameState, Map map, bool adjacent = false)
    {
        Queue<int> queue = new();
        int[] cameFrom = new int[map.Width * map.Height];
        bool[] visited = new bool[map.Width * map.Height];
        int startPositionIndex = startPos.x + startPos.y * map.Width;
        int endPositionIndex = endPos.x + endPos.y * map.Width;
        int[] directions = { -1, 1, -map.Width, map.Width };
        
        queue.Enqueue(startPositionIndex);
        visited[startPositionIndex] = true;
        
        while (queue.Count > 0)
        {
            int position = queue.Dequeue();

            if (adjacent)
            {
                if (position + 1 == endPositionIndex && position % map.Width != map.Width - 1) endPositionIndex -= 1;
                else if (position - 1 == endPositionIndex && position % map.Width != 0) endPositionIndex += 1;
                else if (position + map.Width == endPositionIndex) endPositionIndex -= map.Width;
                else if (position - map.Width == endPositionIndex) endPositionIndex += map.Width;
            }
            
            if (position == endPositionIndex)
            {
                List<Node> path = new();
                int current = endPositionIndex;

                while (current != startPositionIndex)
                {
                    path.Add(map.GetNode(current));
                    current = cameFrom[current];
                }

                path.Reverse();
                return path;
            }

            for (int i = 0; i < directions.Length; i++)
            {
                int dir = directions[i];
                
                if((dir == -1 && position % map.Width == 0) // Border left
                   || (dir == 1 && position % map.Width == map.Width - 1)) continue; // // Border right
                
                int newPosition = position + dir;

                if (!map.IsWalkable(newPosition)) continue;
                if (gameState.GetEntityByGridPosition(new Vector2Int(newPosition % map.Width, newPosition / map.Width)) != null) continue;
                if (visited[newPosition]) continue;
                
                queue.Enqueue(newPosition);
                visited[newPosition] = true;
                cameFrom[newPosition] = position;
            }
        }
        return null;
    }

    /// <summary>
    /// Calculates a Distance Map (Dijkstra Map) from all target positions.
    /// Values represent the walking distance (in cells) to the NEAREST target.
    /// Ignores dynamic entities (walks through them), considers only Map walls.
    /// Returns an array where index = y * width + x.
    /// Unreachable cells have value int.MaxValue.
    /// </summary>
    public static int[] GetDistanceMap(List<Vector2Int> targetPositions, Map map)
    {
        int width = map.Width;
        int height = map.Height;
        int length = width * height;
        int[] distances = new int[length];
        
        // Use Span for faster initialization if possible, or just loop/Array.Fill
        Array.Fill(distances, int.MaxValue);

        Queue<int> queue = new Queue<int>(targetPositions.Count * 2);
        
        foreach (Vector2Int pos in targetPositions)
        {
            if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
            {
                int index = pos.x + pos.y * width;
                distances[index] = 0;
                queue.Enqueue(index);
            }
        }
        
        int[] directions = { -1, 1, -width, width };

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            int currentDist = distances[current];
            int nextDist = currentDist + 1;

            for (int i = 0; i < directions.Length; i++)
            {
                int dir = directions[i];

                // Border checks
                if ((dir == -1 && current % width == 0) || 
                    (dir == 1 && current % width == width - 1)) 
                    continue;

                int neighbor = current + dir;

                if (neighbor < 0 || neighbor >= length) continue;
                if (distances[neighbor] <= nextDist) continue; // Already found a shorter or equal path
                if (!map.IsWalkable(neighbor)) continue;

                distances[neighbor] = nextDist;
                queue.Enqueue(neighbor);
            }
        }

        return distances;
    }

    public struct ReachableNode
    {
        public Vector2Int Position;
        public int Cost;
    }

    /// <summary>
    /// Finds all reachable cells within maxPm range, respecting obstacles (Entities).
    /// </summary>
    public static List<ReachableNode> GetReachableCells(Vector2Int startPos, int maxPm, GameState gameState, Map map)
    {
        int width = map.Width;
        int height = map.Height;
        int length = width * height;
        
        List<ReachableNode> results = new List<ReachableNode>();
        
        // We use an array to store min cost to reach a cell. -1 means unvisited.
        int[] costToReach = new int[length];
        Array.Fill(costToReach, -1);

        int startIdx = startPos.x + startPos.y * width;
        costToReach[startIdx] = 0;
        
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startIdx);
        
        // Add start position (0 movement)
        results.Add(new ReachableNode { Position = startPos, Cost = 0 });

        int[] directions = { -1, 1, -width, width };

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            int currentCost = costToReach[current];
            
            if (currentCost >= maxPm) continue;
            
            int nextCost = currentCost + 1;

            for (int i = 0; i < directions.Length; i++)
            {
                int dir = directions[i];

                if ((dir == -1 && current % width == 0) || 
                    (dir == 1 && current % width == width - 1)) 
                    continue;

                int neighbor = current + dir;

                if (neighbor < 0 || neighbor >= length) continue;
                if (costToReach[neighbor] != -1 && costToReach[neighbor] <= nextCost) continue;
                
                if (!map.IsWalkable(neighbor)) continue;
                
                // Check for Entity at neighbor position
                Vector2Int neighborPos = new Vector2Int(neighbor % width, neighbor / width);
                if (gameState.GetEntityByGridPosition(neighborPos) != null) continue;

                costToReach[neighbor] = nextCost;
                queue.Enqueue(neighbor);
                results.Add(new ReachableNode { Position = neighborPos, Cost = nextCost });
            }
        }

        return results;
    }
}
