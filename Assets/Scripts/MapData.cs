using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using String = System.String;

public class MapData
{
    // MaxRow, MaxColumn are not used
    public const int MaxRow = 18, MaxColumn = 32;
    public string LevelTag { get; private set; }
    public Vector2Int Size { get; private set; }
    public Vector2Int Player { get; set; }
    public int PlayerDepth { get; set; }
    public int FallDistanceSum { get; private set; }

    // depth -1 means it is a wall
    // walls cannot be inside the map
    public int[,] Depth;
    public List<Vector2Int> Rocks;
    public List<Target.Data> Targets;
    public List<Edge> Edges;

    public Queue<MicroHistory> Histories { get; private set; }
    public bool[] MovedEdges;

    public enum Direction { Up, Right, Down, Left }
    private static Vector2Int[] directionDictionary;

    static MapData()
    {
        directionDictionary = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    }

    public void Initialize(string tag)
    {
        LevelTag = tag;

        Edges = new List<Edge>();
        Edge tempEdge;
        switch (tag)
        {
            case "1":
                Size = new Vector2Int(8, 6);
                Player = new Vector2Int(3, 1);
                Depth = new int[Size.x, Size.y];
                Rocks = new List<Vector2Int>();
                Targets = new List<Target.Data>();
                for (int i = 4; i < 7; i++)
                {
                    for (int j = 1; j < 3; j++)
                    {
                        Targets.Add(new Target.Data(new Vector2Int(i, j), 1));
                    }
                }
                for (int i = 1; i < 4; i++)
                {
                    tempEdge = new Edge(new Vector2Int(i, 5), true);
                    Edges.Add(tempEdge);
                    tempEdge = new Edge(new Vector2Int(i + 2, 2), true);
                    Edges.Add(tempEdge);
                }
                for (int i = 2; i < 4; i++)
                {
                    tempEdge = new Edge(new Vector2Int(1, i + 1), false);
                    Edges.Add(tempEdge);
                    tempEdge = new Edge(new Vector2Int(6, i), false);
                    Edges.Add(tempEdge);
                }
                break;

            case "2":
                Size = new Vector2Int(8, 6);
                Player = new Vector2Int(1, 4);
                Depth = new int[Size.x, Size.y];
                Rocks = new List<Vector2Int>();
                Targets = new List<Target.Data>();
                Targets.Add(new Target.Data(new Vector2Int(2, 3), 1));
                Targets.Add(new Target.Data(new Vector2Int(3, 3), 1));
                Targets.Add(new Target.Data(new Vector2Int(4, 2), 1));
                Targets.Add(new Target.Data(new Vector2Int(5, 2), 1));
                for (int i = 1; i < 3; i++)
                {
                    Edges.Add(new Edge(new Vector2Int(i, 1), true));
                    Edges.Add(new Edge(new Vector2Int(i + 1, 3), true));
                    Edges.Add(new Edge(new Vector2Int(i + 3, 3), true));
                    Edges.Add(new Edge(new Vector2Int(i + 4, 5), true));
                }
                Edges.Add(new Edge(new Vector2Int(1, 1), false));
                Edges.Add(new Edge(new Vector2Int(4, 2), false));
                Edges.Add(new Edge(new Vector2Int(4, 3), false));
                Edges.Add(new Edge(new Vector2Int(7, 4), false));
                break;

            case "3":
                Size = new Vector2Int(8, 5);
                Player = new Vector2Int(5, 3);
                Depth = new int[Size.x, Size.y];
                Rocks = new List<Vector2Int>();
                Targets = new List<Target.Data>();
                Targets.Add(new Target.Data(new Vector2Int(4, 1), 1));
                Targets.Add(new Target.Data(new Vector2Int(4, 2), 1));
                Targets.Add(new Target.Data(new Vector2Int(6, 1), 1));
                Targets.Add(new Target.Data(new Vector2Int(6, 2), 1));
                for (int i = 1; i < 4; i++)
                {
                    Edges.Add(new Edge(new Vector2Int(i, 3), true));
                    Edges.Add(new Edge(new Vector2Int(i + 2, 1), true));
                }
                Edges.Add(new Edge(new Vector2Int(1, 2), false));
                Edges.Add(new Edge(new Vector2Int(4, 2), false));
                Edges.Add(new Edge(new Vector2Int(3, 1), false));
                Edges.Add(new Edge(new Vector2Int(6, 1), false));
                Rocks.Add(new Vector2Int(2, 1));
                Rocks.Add(new Vector2Int(2, 2));
                Rocks.Add(new Vector2Int(3, 2));
                Rocks.Add(new Vector2Int(5, 4));
                Rocks.Add(new Vector2Int(6, 4));
                Rocks.Add(new Vector2Int(6, 0));
                Rocks.Add(new Vector2Int(7, 0));
                Rocks.Add(new Vector2Int(7, 1));
                break;

            case "4":
                Size = new Vector2Int(8, 6);
                Player = new Vector2Int(1, 4);
                Depth = new int[Size.x, Size.y];
                Rocks = new List<Vector2Int>();
                Targets = new List<Target.Data>();
                Targets.Add(new Target.Data(new Vector2Int(4, 3), 2));
                Targets.Add(new Target.Data(new Vector2Int(5, 3), 2));
                for (int i = 1; i < 3; i++)
                {
                    Edges.Add(new Edge(new Vector2Int(i, 1), true));
                    Edges.Add(new Edge(new Vector2Int(i + 1, 3), true));
                    Edges.Add(new Edge(new Vector2Int(i + 3, 3), true));
                    Edges.Add(new Edge(new Vector2Int(i + 4, 5), true));
                }
                Edges.Add(new Edge(new Vector2Int(1, 1), false));
                Edges.Add(new Edge(new Vector2Int(4, 2), false));
                Edges.Add(new Edge(new Vector2Int(4, 3), false));
                Edges.Add(new Edge(new Vector2Int(7, 4), false));
                break;

            case "7":
                Size = new Vector2Int(11, 7);
                Player = new Vector2Int(1, 0);
                Depth = new int[Size.x, Size.y];
                Rocks = new List<Vector2Int>();
                Targets = new List<Target.Data>();
                Targets.Add(new Target.Data(new Vector2Int(4, 2), 1));
                Targets.Add(new Target.Data(new Vector2Int(5, 3), 1));
                for (int i = 2; i < 4; i++)
                {
                    tempEdge = new Edge(new Vector2Int(i, 2), true);
                    Edges.Add(tempEdge);
                    tempEdge = new Edge(new Vector2Int(i + 5, 4), true);
                    Edges.Add(tempEdge);
                }
                for (int i = 1; i < 3; i++)
                {
                    tempEdge = new Edge(new Vector2Int(2, i + 3), false);
                    Edges.Add(tempEdge);
                    tempEdge = new Edge(new Vector2Int(9, i), false);
                    Edges.Add(tempEdge);
                }
                Rocks.Add(new Vector2Int(2, 5));
                Rocks.Add(new Vector2Int(4, 4));
                Rocks.Add(new Vector2Int(6, 2));
                Rocks.Add(new Vector2Int(6, 0));
                break;

            case "5":
                Size = new Vector2Int(8, 6);
                Player = new Vector2Int(4, 3);
                Depth = new int[Size.x, Size.y];
                Rocks = new List<Vector2Int>();
                Targets = new List<Target.Data>();
                Targets.Add(new Target.Data(new Vector2Int(2, 1), 1));
                Targets.Add(new Target.Data(new Vector2Int(2, 4), 1));
                Targets.Add(new Target.Data(new Vector2Int(5, 1), 1));
                Targets.Add(new Target.Data(new Vector2Int(5, 4), 1));
                Targets.Add(new Target.Data(new Vector2Int(3, 2), 2));
                Targets.Add(new Target.Data(new Vector2Int(3, 3), 2));
                Edges.Add(new Edge(new Vector2Int(1, 1), true));
                Edges.Add(new Edge(new Vector2Int(2, 1), true));
                Edges.Add(new Edge(new Vector2Int(5, 1), true));
                Edges.Add(new Edge(new Vector2Int(6, 1), true));
                Edges.Add(new Edge(new Vector2Int(1, 5), true));
                Edges.Add(new Edge(new Vector2Int(2, 5), true));
                Edges.Add(new Edge(new Vector2Int(5, 5), true));
                Edges.Add(new Edge(new Vector2Int(6, 5), true));
                for (int i = 1; i < 5; i++)
                {
                    Edges.Add(new Edge(new Vector2Int(1, i), false));
                    Edges.Add(new Edge(new Vector2Int(7, i), false));
                }
                Edges.Add(new Edge(new Vector2Int(3, 2), true));
                Edges.Add(new Edge(new Vector2Int(3, 2), false));
                Edges.Add(new Edge(new Vector2Int(3, 3), false));
                Edges.Add(new Edge(new Vector2Int(4, 4), true));
                Edges.Add(new Edge(new Vector2Int(5, 2), false));
                Edges.Add(new Edge(new Vector2Int(5, 3), false));
                break;

            case "6":
                Size = new Vector2Int(7, 7);
                Player = new Vector2Int(0, 0);
                Depth = new int[Size.x, Size.y];
                Rocks = new List<Vector2Int>();
                Targets = new List<Target.Data>();
                Targets.Add(new Target.Data(new Vector2Int(2, 2), 1));
                Targets.Add(new Target.Data(new Vector2Int(3, 2), 1));
                Targets.Add(new Target.Data(new Vector2Int(2, 3), 1));
                Targets.Add(new Target.Data(new Vector2Int(3, 3), 1));
                Targets.Add(new Target.Data(new Vector2Int(4, 3), 1));
                Targets.Add(new Target.Data(new Vector2Int(3, 4), 1));
                Targets.Add(new Target.Data(new Vector2Int(4, 4), 1));
                Edges.Add(new Edge(new Vector2Int(0, 1), true));
                Edges.Add(new Edge(new Vector2Int(2, 1), true));
                Edges.Add(new Edge(new Vector2Int(3, 1), true));
                Edges.Add(new Edge(new Vector2Int(1, 4), true));
                Edges.Add(new Edge(new Vector2Int(2, 4), true));
                Edges.Add(new Edge(new Vector2Int(4, 4), true));
                Edges.Add(new Edge(new Vector2Int(5, 4), true));
                Edges.Add(new Edge(new Vector2Int(4, 6), true));
                Edges.Add(new Edge(new Vector2Int(1, 0), false));
                Edges.Add(new Edge(new Vector2Int(1, 2), false));
                Edges.Add(new Edge(new Vector2Int(1, 3), false));
                Edges.Add(new Edge(new Vector2Int(4, 1), false));
                Edges.Add(new Edge(new Vector2Int(4, 2), false));
                Edges.Add(new Edge(new Vector2Int(4, 4), false));
                Edges.Add(new Edge(new Vector2Int(4, 5), false));
                Edges.Add(new Edge(new Vector2Int(6, 4), false));
                break;

            case "debug":
            default:
                if (tag != "debug" && tag != "1")
                {
                    Debug.LogWarning($"Initialize: level tagged {tag} was not found");
                }
                Size = new Vector2Int(9, 7);
                Player = new Vector2Int(5, 1);
                Depth = new int[Size.x, Size.y];
                Rocks = new List<Vector2Int>();
                Targets = new List<Target.Data>();
                Rocks.Add(new Vector2Int(0, 6));
                Rocks.Add(new Vector2Int(1, 6));
                for (int i = 3; i < 6; i++)
                {
                    for (int j = 3; j < 5; j++)
                    {
                        Targets.Add(new Target.Data(new Vector2Int(i, j), 1));
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    tempEdge = new Edge(new Vector2Int(i, 2), true);
                    Edges.Add(tempEdge);
                    tempEdge = new Edge(new Vector2Int(i + 5, 6), true);
                    Edges.Add(tempEdge);
                }
                for (int i = 0; i < 2; i++)
                {
                    tempEdge = new Edge(new Vector2Int(3, i + 2), false);
                    Edges.Add(tempEdge);
                    tempEdge = new Edge(new Vector2Int(5, i + 4), false);
                    Edges.Add(tempEdge);
                }
                break;
        }

        if (!InMap(Player))
        {
            Debug.LogWarning($"MapData.Initialize: start position {Player} is out of the map in level {tag}");
            Player = Vector2Int.zero;
        }
        PlayerDepth = 0;
    }
    public MapData Clone()
    {
        MapData clone = (MapData)this.MemberwiseClone();
        clone.Depth = (int[,])Depth.Clone();
        clone.Rocks = new List<Vector2Int>(Rocks);
        clone.Targets = new List<Target.Data>(Targets.Count);
        for (int i = 0; i < Targets.Count; i++) clone.Targets.Add(Targets[i].Clone());
        clone.Edges = new List<Edge>(Edges.Count);
        for (int i = 0; i < Edges.Count; i++) clone.Edges.Add((Edge)Edges[i].Clone());
        return clone;
    }
    public void Reset(MapData initialState)
    {
        Player = initialState.Player;
        PlayerDepth = initialState.PlayerDepth;
        Depth = (int[,])initialState.Depth.Clone();
        for (int i = 0; i < Edges.Count; i++)
        {
            Edges[i].ChangePosition(initialState.Edges[i].Position);
            Edges[i].ChangeDepth(initialState.Edges[i].Depth);
            Edges[i].SetActive(true);
        }
    }
    public bool Win()
    {
        for (int i = 0; i < Targets.Count; i++)
        {
            if (Depth[Targets[i].Position.x, Targets[i].Position.y] != Targets[i].Depth) return false;
        }
        return true;
    }

    // MicroHistory is to store data about what happens in each execution of game logic within a turn
    public class MicroHistory
    {
        // FallDistance is each edge's distance to fall
        // Overlapped is if the edge is disabled because it has overlapped with other edge
        // UsedInLoop is if the edge is disabled it has been part of a loop
        // FallInExcavation is if the edge falls during the excavation
        // InLoop is if the cell is excavated
        public int[] FallDistance;
        public bool[] Overlapped, UsedInLoop, FallInExcavation;
        public bool[,] Excavated;
        public int PlayerFallDistance { get; set; }
        public int MaxFallDistance { get; set; }
        public MicroHistory(int edgeCount, Vector2Int Size)
        {
            FallDistance = new int[edgeCount];
            Overlapped = new bool[edgeCount];
            UsedInLoop = new bool[edgeCount];
            FallInExcavation = new bool[edgeCount];
            Excavated = new bool[Size.x, Size.y];
            PlayerFallDistance = 0;
            MaxFallDistance = 0;
        }
    }

    public bool Move(Direction direction)
    {
        Histories = new Queue<MicroHistory>();
        MovedEdges = new bool[Edges.Count];
        FallDistanceSum = 0;
        Vector2Int targetPosition = Player + directionDictionary[(int)direction];
        if (!InMap(targetPosition)) return false;

        // check if there is an edge in the moving direction
        int edgeIndex = -1;
        switch (direction)
        {
            case Direction.Up:
                edgeIndex = Edges.FindIndex(x => x.Active && x.Position == Player + Vector2Int.up && x.Horizontal && x.Depth == PlayerDepth);
                break;
            case Direction.Right:
                edgeIndex = Edges.FindIndex(x => x.Active && x.Position == Player + Vector2Int.right && !x.Horizontal && x.Depth == PlayerDepth);
                break;
            case Direction.Down:
                edgeIndex = Edges.FindIndex(x => x.Active && x.Position == Player && x.Horizontal && x.Depth == PlayerDepth);
                break;
            case Direction.Left:
                edgeIndex = Edges.FindIndex(x => x.Active && x.Position == Player && !x.Horizontal && x.Depth == PlayerDepth);
                break;
            default:
                Debug.LogWarning($"MapData.Move-find: not implemented for Direction {Enum.GetName(typeof(Direction), direction)}");
                return false;
        }

        // there is no edge at the place the player is moving into
        if (edgeIndex < 0)
        {
            if (Rocks.Contains(targetPosition) || Depth[targetPosition.x, targetPosition.y] < PlayerDepth) return false;
            else
            {
                Player = targetPosition;
                while (SimulateLogic()) { }
                PlayerDepth = Depth[targetPosition.x, targetPosition.y];
                return true;
            }
        }
        // there IS an edge at the place the player is moving into
        else
        {
            bool canMove = !Rocks.Contains(targetPosition) && Depth[targetPosition.x, targetPosition.y] >= PlayerDepth;
            if (!canMove) return false;

            // check if the edges are blocked
            List<Edge> connectedEdges = GetConnectedEdges(Edges, edgeIndex);
            switch (direction)
            {
                case Direction.Up:
                    foreach (Edge edge in connectedEdges)
                    {
                        if (edge.Horizontal) canMove &= !(GetDepth(edge.Position) == edge.Depth && Rocks.Contains(edge.Position)) && GetDepth(edge.Position) >= PlayerDepth;
                        else canMove &= GetEdgeDepth(edge.Position + Vector2Int.up, false) >= PlayerDepth;
                    }
                    break;
                case Direction.Right:
                    foreach (Edge edge in connectedEdges)
                    {
                        if (!edge.Horizontal) canMove &= !(GetDepth(edge.Position) == edge.Depth && Rocks.Contains(edge.Position)) && GetDepth(edge.Position) >= PlayerDepth;
                        else canMove &= GetEdgeDepth(edge.Position + Vector2Int.right, true) >= PlayerDepth;
                    }
                    break;
                case Direction.Down:
                    foreach (Edge edge in connectedEdges)
                    {
                        if (edge.Horizontal)
                        {
                            canMove &= !(GetDepth(edge.Position + Vector2Int.down) == edge.Depth && Rocks.Contains(edge.Position + Vector2Int.down)) &&
                                GetDepth(edge.Position + Vector2Int.down) >= PlayerDepth;
                        }
                        else canMove &= GetEdgeDepth(edge.Position + Vector2Int.down, false) >= PlayerDepth;
                    }
                    break;
                case Direction.Left:
                    foreach (Edge edge in connectedEdges)
                    {
                        if (!edge.Horizontal)
                        {
                            canMove &= !(GetDepth(edge.Position + Vector2Int.left) == edge.Depth && Rocks.Contains(edge.Position + Vector2Int.left)) &&
                                GetDepth(edge.Position + Vector2Int.left) >= PlayerDepth;
                        }
                        else canMove &= GetEdgeDepth(edge.Position + Vector2Int.left, true) >= PlayerDepth;
                    }
                    break;
                default:
                    Debug.LogWarning($"MapData.Move-move: not implemented for Direction {Enum.GetName(typeof(Direction), direction)}");
                    return false;
            }

            if (!canMove) return false;

            // if the player can movable, move
            Player = targetPosition;
            foreach (Edge edge in connectedEdges)
            {
                edge.ChangePosition(edge.Position + directionDictionary[(int)direction]);
                MovedEdges[Edges.IndexOf(edge)] = true;
            }

            while (SimulateLogic()) { }
            PlayerDepth = Depth[targetPosition.x, targetPosition.y];
            return true;
        }
    }
    public bool InMap(Vector2Int coordinates, bool isEdge = false, bool horizontal = true)
    {
        if (isEdge)
        {
            if (horizontal) return coordinates.x >= 0 && coordinates.y >= 0 && coordinates.x < Size.x && coordinates.y <= Size.y;
            else return coordinates.x >= 0 && coordinates.y >= 0 && coordinates.x <= Size.x && coordinates.y < Size.y;
        }
        else return coordinates.x >= 0 && coordinates.y >= 0 && coordinates.x < Size.x && coordinates.y < Size.y;
    }
    // find an active edge in the described position and orientatin, and return its index in edgeList
    private int FindEdge(Vector2Int position, bool horizontal, int depth, List<Edge> edgeList)
    {
        return edgeList.FindIndex(x => x.Active && x.Position == position && x.Horizontal == horizontal && x.Depth == depth);
    }
    private int GetDepth(Vector2Int position)
    {
        if (InMap(position)) return Depth[position.x, position.y];
        else return -1;
    }
    private int GetEdgeDepth(Vector2Int position, bool horizontal)
    {
        if (horizontal) return Math.Max(GetDepth(position + Vector2Int.down), GetDepth(position));
        else return Math.Max(GetDepth(position + Vector2Int.left), GetDepth(position));
    }
    // get a list of edges that are connected to Edges[index]
    private List<Edge> GetConnectedEdges(List<Edge> edges, int index, bool removeFromOriginal = false)
    {
        if (index >= edges.Count)
        {
            Debug.LogError($"MapData.GetConnectedEdges: index {index} exceeds the edge list count {edges.Count}");
            return null;
        }
        List<Edge> uncheckedEdges = removeFromOriginal ? edges : new List<Edge>(edges), connectedEdges = new List<Edge>();
        Stack<Edge> checkingEdges = new Stack<Edge>();
        Edge tempEdge = uncheckedEdges[index];
        checkingEdges.Push(tempEdge);
        uncheckedEdges.RemoveAt(index);

        while (checkingEdges.Count > 0)
        {
            tempEdge = checkingEdges.Pop();
            connectedEdges.Add(tempEdge);
            if (tempEdge.Horizontal)
            {
                // left
                index = FindEdge(tempEdge.Position + Vector2Int.left, true, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // right
                index = FindEdge(tempEdge.Position + Vector2Int.right, true, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // up left
                index = FindEdge(tempEdge.Position, false, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // up right
                index = FindEdge(tempEdge.Position + Vector2Int.right, false, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // up left
                index = FindEdge(tempEdge.Position + Vector2Int.down, false, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // up right
                index = FindEdge(tempEdge.Position + Vector2Int.down + Vector2Int.right, false, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
            }
            else
            {
                // up
                index = FindEdge(tempEdge.Position + Vector2Int.up, false, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // down
                index = FindEdge(tempEdge.Position + Vector2Int.down, false, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // up left
                index = FindEdge(tempEdge.Position + Vector2Int.left + Vector2Int.up, true, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // up right
                index = FindEdge(tempEdge.Position + Vector2Int.up, true, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // down left
                index = FindEdge(tempEdge.Position + Vector2Int.left, true, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
                // down right
                index = FindEdge(tempEdge.Position, true, tempEdge.Depth, uncheckedEdges);
                if (index >= 0)
                {
                    checkingEdges.Push(uncheckedEdges[index]);
                    uncheckedEdges.RemoveAt(index);
                }
            }
        }
        return connectedEdges;
    }
    // edges in exactly the same position are connected too
    private bool AreConnected(Edge edge1, Edge edge2, bool ignoreDepth = false)
    {
        if (!edge1.Active || !edge2.Active) return false;
        if (!ignoreDepth && edge1.Depth != edge2.Depth) return false;
        if (edge1.Horizontal)
        {
            return edge2.Horizontal && (edge2.Position == edge1.Position || edge2.Position == edge1.Position + Vector2Int.left ||
                edge2.Position == edge1.Position + Vector2Int.right) || !edge2.Horizontal && (edge2.Position == edge1.Position ||
                edge2.Position == edge1.Position + Vector2Int.right || edge2.Position == edge1.Position + Vector2Int.down ||
                edge2.Position == edge1.Position + Vector2Int.down + Vector2Int.right);
        }
        else
        {
            return !edge2.Horizontal && (edge2.Position == edge1.Position || edge2.Position == edge1.Position + Vector2Int.down ||
                edge2.Position == edge1.Position + Vector2Int.up) || edge2.Horizontal && (edge2.Position == edge1.Position + Vector2Int.up + Vector2Int.left ||
                edge2.Position == edge1.Position + Vector2Int.up || edge2.Position == edge1.Position + Vector2Int.left || edge2.Position == edge1.Position);
        }
    }
    // returns if there were any excavated cells
    private bool SimulateLogic()
    {
        MicroHistory microHistory = new MicroHistory(Edges.Count, Size);
        int maxEdgeDepth = -1, maxCellDepth = -1;
        foreach (Edge edge in Edges) if (maxEdgeDepth < edge.Depth) maxEdgeDepth = edge.Depth;
        for (int i = 0; i < Size.x; i++)
        {
            for (int j = 0; j < Size.y; j++)
            {
                if (maxCellDepth < Depth[i, j]) maxCellDepth = Depth[i, j];
            }
        }

        // drop edges
        for (int layer = maxEdgeDepth; layer >= 0; layer--)
        {
            // get fall distance for all edges in this layer
            bool[] visited = new bool[Edges.Count];
            for (int i = 0; i < Edges.Count; i++)
            {
                if (visited[i] || !Edges[i].Active || Edges[i].Depth != layer) continue;
                List<Edge> unconnectedEdges = new List<Edge>(Edges), connectedEdges;
                connectedEdges = GetConnectedEdges(unconnectedEdges, i, true);

                // catchLayer is the layer the connected edges are catched
                int catchLayer;
                for (catchLayer = layer; catchLayer <= maxCellDepth; catchLayer++)
                {
                    bool cathed = false;
                    foreach (Edge edge in connectedEdges)
                    {
                        if (GetEdgeDepth(edge.Position, edge.Horizontal) == catchLayer)
                        {
                            cathed = true;
                            break;
                        }
                        if (unconnectedEdges.Find(x => x.Depth == catchLayer && AreConnected(edge, x, true)) != null)
                        {
                            cathed = true;
                            break;
                        }
                    }
                    if (cathed) break;
                }
                if (catchLayer > maxCellDepth)
                {
                    Debug.LogWarning($"MapData.SimulateLogic-drop: edge{i} ({Edges[i].Position}) was not catched by layer {maxCellDepth}");
                    catchLayer = maxCellDepth;
                }

                // register fall distance
                for (int j = 0; j < Edges.Count; j++)
                {
                    if (connectedEdges.Contains(Edges[j]))
                    {
                        microHistory.FallDistance[j] = catchLayer - layer;
                        if (microHistory.MaxFallDistance < microHistory.FallDistance[j]) microHistory.MaxFallDistance = microHistory.FallDistance[j];
                        visited[j] = true;
                    }
                }
            }

            // drop
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].Depth == layer) Edges[i].ChangeDepth(Edges[i].Depth + microHistory.FallDistance[i]);
            }
        }

        // drop the player
        if (PlayerDepth < Depth[Player.x, Player.y])
        {
            microHistory.PlayerFallDistance = Depth[Player.x, Player.y] - PlayerDepth;
            PlayerDepth = Depth[Player.x, Player.y];
            if (microHistory.MaxFallDistance < microHistory.PlayerFallDistance) microHistory.MaxFallDistance = microHistory.PlayerFallDistance;
        }
        FallDistanceSum += microHistory.MaxFallDistance;

        // remove overlapping edges
        for (int i = 0; i < Edges.Count - 1; i++)
        {
            if (!Edges[i].Active) continue;
            for (int j = i + 1; j < Edges.Count; j++)
            {
                if (Edges[j].Active && Edges[j].Position == Edges[i].Position && Edges[j].Horizontal == Edges[i].Horizontal && Edges[j].Depth == Edges[i].Depth)
                {
                    microHistory.Overlapped[i] = true;
                    Edges[i].SetActive(false);
                    break;
                }
            }
        }

        // excavate enclosed cells
        bool excavated = false;
        for (int layer = maxCellDepth; layer >= 0; layer--)
        {
            // find edges in the layer
            List<Edge> edgesInLayer = new List<Edge>();
            foreach (Edge edge in Edges)
            {
                if (edge.Active && edge.Depth == layer) edgesInLayer.Add(edge);
            }

            // find enclosed cells
            // use cells outside of the map
            // e.g. since (-1, -1) cannot be enclosed, notEnclosed[0, 0] is true
            bool[,] notEnclosed = new bool[Size.x + 2, Size.y + 2];
            Stack<Vector2Int> checkingCells = new Stack<Vector2Int>();
            checkingCells.Push(new Vector2Int(-1, -1));
            notEnclosed[0, 0] = true;
            while (checkingCells.Count > 0)
            {
                Vector2Int checkingCell = checkingCells.Pop();
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                {
                    Vector2Int candidate = checkingCell + directionDictionary[(int)direction];
                    if (candidate.x < -1 || candidate.y < -1 || candidate.x > Size.x || candidate.y > Size.y) continue;
                    if (notEnclosed[candidate.x + 1, candidate.y + 1]) continue;
                    switch (direction)
                    {
                        case Direction.Up:
                            if (FindEdge(candidate, true, layer, edgesInLayer) < 0)
                            {
                                notEnclosed[candidate.x + 1, candidate.y + 1] = true;
                                checkingCells.Push(candidate);
                            }
                            break;
                        case Direction.Right:
                            if (FindEdge(candidate, false, layer, edgesInLayer) < 0)
                            {
                                notEnclosed[candidate.x + 1, candidate.y + 1] = true;
                                checkingCells.Push(candidate);
                            }
                            break;
                        case Direction.Down:
                            if (FindEdge(checkingCell, true, layer, edgesInLayer) < 0)
                            {
                                notEnclosed[candidate.x + 1, candidate.y + 1] = true;
                                checkingCells.Push(candidate);
                            }
                            break;
                        case Direction.Left:
                            if (FindEdge(checkingCell, false, layer, edgesInLayer) < 0)
                            {
                                notEnclosed[candidate.x + 1, candidate.y + 1] = true;
                                checkingCells.Push(candidate);
                            }
                            break;
                        default:
                            Debug.LogWarning($"MapData.SimulateLogic-find enclosed cells: not implemented for Direction {Enum.GetName(typeof(Direction), direction)}");
                            continue;
                    }
                }
            }

            // excavate cells inside the found loop
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    if (!notEnclosed[i + 1, j + 1] && Depth[i, j] == layer)
                    {
                        excavated = true;
                        microHistory.Excavated[i, j] = true;
                        Depth[i, j]++;
                    }
                }
            }

            if (microHistory.Excavated[Player.x, Player.y]) PlayerDepth++;

            // register edges used in loops
            for (int i = 0; i < Edges.Count; i++)
            {
                Edge edge = Edges[i];
                if (edge.Active && edge.Depth == layer)
                {
                    if (edge.Horizontal)
                    {
                        if (notEnclosed[edge.Position.x + 1, edge.Position.y] != notEnclosed[edge.Position.x + 1, edge.Position.y + 1])
                        {
                            microHistory.UsedInLoop[i] = true;
                        }
                    }
                    else
                    {
                        if (notEnclosed[edge.Position.x, edge.Position.y + 1] != notEnclosed[edge.Position.x + 1, edge.Position.y + 1])
                        {
                            microHistory.UsedInLoop[i] = true;
                        }
                    }
                }
            }

            // find edges that fall during the excavation
            bool[] visited = new bool[Edges.Count];
            for (int i = 0; i < Edges.Count; i++)
            {
                if (!Edges[i].Active || microHistory.UsedInLoop[i] || Edges[i].Depth != layer || visited[i]) continue;
                List<Edge> connectedEdges;
                connectedEdges = GetConnectedEdges(Edges, i);

                bool fallInExcavation = true;
                for (int j = i; j < Edges.Count; j++)
                {
                    if (!connectedEdges.Contains(Edges[j])) continue;
                    visited[j] = true;
                    fallInExcavation &= !(microHistory.UsedInLoop[j] || GetEdgeDepth(Edges[j].Position, Edges[j].Horizontal) == layer);
                }

                if (fallInExcavation)
                {
                    for (int j = i; j < Edges.Count; j++)
                    {
                        if (!connectedEdges.Contains(Edges[j])) continue;
                        microHistory.FallInExcavation[j] = true;
                        Edges[j].ChangeDepth(layer + 1);
                    }
                }
            }

            // remove edges used in loops
            for (int i = 0; i < Edges.Count; i++)
            {
                if (microHistory.UsedInLoop[i]) Edges[i].SetActive(false);
            }
        }

        Histories.Enqueue(microHistory);
        return excavated;
    }

    public static string GetLevelTag(int level)
    {
        switch (level)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
                return (level + 1).ToString();
            default:
                return (level % 2 == 0) ? "debugalpha" : "debug";
        }
    }
    public static string GetLevelName(string tag)
    {
        switch (tag)
        {
            case "1":
            case "2":
            case "3":
            case "4":
            case "5":
            case "6":
            case "7":
                return $"level {tag}";
            case "debug":
                return "Debug";
            case "debugalpha":
                return "Debug Alpha";
            default:
                Debug.LogWarning($"GetLevelName: no assigned name for level tag {tag}");
                return "Error: Unnamed Level";
        }
    }
}
