using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>以随机游走生成房间中心，再用矩形房间和 L 形走廊组成可达地牢。</summary>
public sealed class DungeonGenerator : MonoBehaviour
{
    private readonly HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    private readonly List<Vector2> enemySpawns = new List<Vector2>();
    private System.Random random;

    public Vector2 PlayerSpawn { get; private set; }
    public IReadOnlyList<Vector2> EnemySpawns => enemySpawns;

    /// <summary>在当前地牢的可行走网格上使用 A* 计算四方向最短路径。</summary>
    public bool FindPath(Vector2 worldStart, Vector2 worldGoal, List<Vector2> result)
    {
        result.Clear();
        Vector2Int start = FindNearestFloor(Vector2Int.RoundToInt(worldStart));
        Vector2Int goal = FindNearestFloor(Vector2Int.RoundToInt(worldGoal));
        if (!floorTiles.Contains(start) || !floorTiles.Contains(goal)) return false;

        var open = new List<Vector2Int> { start };
        var closed = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
        Vector2Int[] directions = { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };

        while (open.Count > 0)
        {
            int bestIndex = 0;
            int bestScore = int.MaxValue;
            for (int i = 0; i < open.Count; i++)
            {
                int score = gScore[open[i]] + Manhattan(open[i], goal);
                if (score < bestScore) { bestScore = score; bestIndex = i; }
            }

            Vector2Int current = open[bestIndex];
            open.RemoveAt(bestIndex);
            if (current == goal)
            {
                ReconstructPath(start, goal, cameFrom, result);
                return true;
            }

            closed.Add(current);
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = current + direction;
                if (!floorTiles.Contains(neighbor) || closed.Contains(neighbor)) continue;
                int tentative = gScore[current] + 1;
                int known;
                if (!gScore.TryGetValue(neighbor, out known) || tentative < known)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative;
                    if (!open.Contains(neighbor)) open.Add(neighbor);
                }
            }
        }
        return false;
    }

    private Vector2Int FindNearestFloor(Vector2Int origin)
    {
        if (floorTiles.Contains(origin)) return origin;
        for (int radius = 1; radius <= 3; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int candidate = origin + new Vector2Int(x, y);
                    if (floorTiles.Contains(candidate)) return candidate;
                }
            }
        }
        return origin;
    }

    private static int Manhattan(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static void ReconstructPath(Vector2Int start, Vector2Int goal,
        Dictionary<Vector2Int, Vector2Int> cameFrom, List<Vector2> result)
    {
        var reversed = new List<Vector2Int>();
        Vector2Int current = goal;
        reversed.Add(current);
        while (current != start && cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            reversed.Add(current);
        }
        for (int i = reversed.Count - 2; i >= 0; i--) result.Add(reversed[i]);
    }

    public void Generate(int seed)
    {
        random = new System.Random(seed);
        floorTiles.Clear();
        enemySpawns.Clear();

        List<Vector2Int> centers = CreateRoomCenters();
        for (int i = 0; i < centers.Count; i++)
        {
            int width = random.Next(9, 14);
            int height = random.Next(7, 11);
            AddRoom(centers[i], width, height);
            if (i > 0)
            {
                AddCorridor(centers[i - 1], centers[i]);
            }
        }

        PlayerSpawn = centers[0];
        for (int roomIndex = 1; roomIndex < centers.Count; roomIndex++)
        {
            int count = 2 + roomIndex / 2;
            for (int i = 0; i < count; i++)
            {
                enemySpawns.Add(centers[roomIndex] + new Vector2(
                    (float)(random.NextDouble() * 5.0 - 2.5),
                    (float)(random.NextDouble() * 3.0 - 1.5)));
            }
        }

        BuildVisuals();
    }

    private List<Vector2Int> CreateRoomCenters()
    {
        var centers = new List<Vector2Int> { Vector2Int.zero };
        var occupied = new HashSet<Vector2Int> { Vector2Int.zero };
        Vector2Int cursor = Vector2Int.zero;
        Vector2Int[] directions = { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };

        while (centers.Count < 8)
        {
            Vector2Int next = cursor + directions[random.Next(directions.Length)];
            if (occupied.Add(next))
            {
                centers.Add(next * 15);
            }
            cursor = next;
        }

        return centers;
    }

    private void AddRoom(Vector2Int center, int width, int height)
    {
        int halfWidth = width / 2;
        int halfHeight = height / 2;
        for (int x = -halfWidth; x <= halfWidth; x++)
        {
            for (int y = -halfHeight; y <= halfHeight; y++)
            {
                floorTiles.Add(center + new Vector2Int(x, y));
            }
        }
    }

    private void AddCorridor(Vector2Int from, Vector2Int to)
    {
        bool horizontalFirst = random.NextDouble() > 0.5;
        Vector2Int corner = horizontalFirst ? new Vector2Int(to.x, from.y) : new Vector2Int(from.x, to.y);
        AddWideLine(from, corner);
        AddWideLine(corner, to);
    }

    private void AddWideLine(Vector2Int from, Vector2Int to)
    {
        Vector2Int cursor = from;
        Vector2Int direction = new Vector2Int(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y));
        while (cursor != to)
        {
            AddCorridorWidth(cursor);
            cursor += direction;
        }
        AddCorridorWidth(to);
    }

    private void AddCorridorWidth(Vector2Int point)
    {
        floorTiles.Add(point);
        floorTiles.Add(point + Vector2Int.right);
        floorTiles.Add(point + Vector2Int.up);
        floorTiles.Add(point + Vector2Int.right + Vector2Int.up);
    }

    private void BuildVisuals()
    {
        Transform floorRoot = new GameObject("Floor Tiles").transform;
        Transform wallRoot = new GameObject("Wall Tiles").transform;
        floorRoot.SetParent(transform);
        wallRoot.SetParent(transform);

        foreach (Vector2Int tile in floorTiles)
        {
            GameObject floor = new GameObject($"Floor {tile.x},{tile.y}");
            floor.transform.SetParent(floorRoot);
            floor.transform.position = new Vector3(tile.x, tile.y, 0f);
            Color color = ((tile.x + tile.y) & 1) == 0
                ? new Color(0.12f, 0.15f, 0.22f)
                : new Color(0.14f, 0.17f, 0.24f);
            RuntimeSpriteFactory.AddRenderer(floor, color, -10, Vector2.one * 1.02f);
        }

        var walls = new HashSet<Vector2Int>();
        foreach (Vector2Int tile in floorTiles)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int candidate = tile + new Vector2Int(x, y);
                    if (!floorTiles.Contains(candidate))
                    {
                        walls.Add(candidate);
                    }
                }
            }
        }

        foreach (Vector2Int tile in walls)
        {
            GameObject wall = new GameObject($"Wall {tile.x},{tile.y}");
            wall.transform.SetParent(wallRoot);
            wall.transform.position = new Vector3(tile.x, tile.y, 0f);
            RuntimeSpriteFactory.AddRenderer(wall, new Color(0.28f, 0.32f, 0.45f), -5, Vector2.one);
            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }
    }
}
