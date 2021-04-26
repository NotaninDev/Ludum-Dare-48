using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class History : MonoBehaviour
{
    public enum Type
    {
        Player, // position, depth
        Edge, // target, position, depth, active
        Tile, // position, depth
        Target, // target, depth
        MoveCount,
    }
    public struct HistoryUnit
    {
        public Type type;
        public int target, depth;
        public Vector2Int position;
        public bool flag;
        public Map.Direction direction;
    }
    public static Stack<HistoryUnit> historyTurn = null;
    public static Stack<Stack<HistoryUnit>> history;

    public static void Initialize()
    {
        history = new Stack<Stack<HistoryUnit>>();
    }

    // initialize the next turn's history container
    public static void StartTurn()
    {
        if (historyTurn != null)
        {
            if (historyTurn.Count > 0) history.Push(historyTurn);
        }
        historyTurn = new Stack<HistoryUnit>();
    }
    public static void AddHistory(Type type, int target)
    {
        switch (type)
        {
            case Type.MoveCount:
                HistoryUnit unit = new HistoryUnit();
                unit.type = type;
                unit.target = target;
                historyTurn.Push(unit);
                break;
            default:
                Debug.LogWarning(String.Format("AddHistory(int): type {0} is invalid", Enum.GetNames(typeof(Type))[(int)type]));
                break;
        }
    }
    public static void AddHistory(Type type, int target, int depth)
    {
        switch (type)
        {
            case Type.Target:
                HistoryUnit unit = new HistoryUnit();
                unit.type = type;
                unit.target = target;
                unit.depth = depth;
                historyTurn.Push(unit);
                break;
            default:
                Debug.LogWarning(String.Format("AddHistory(int, int): type {0} is invalid", Enum.GetNames(typeof(Type))[(int)type]));
                break;
        }
    }
    public static void AddHistory(Type type, Vector2Int position, int depth)
    {
        switch (type)
        {
            case Type.Player:
            case Type.Tile:
                HistoryUnit unit = new HistoryUnit();
                unit.type = type;
                unit.position = position;
                unit.depth = depth;
                historyTurn.Push(unit);
                break;
            default:
                Debug.LogWarning(String.Format("AddHistory(Vector2Int, int): type {0} is invalid", Enum.GetNames(typeof(Type))[(int)type]));
                break;
        }
    }
    public static void AddHistory(Type type, int target, Vector2Int position, int depth, bool active)
    {
        switch (type)
        {
            case Type.Edge:
                HistoryUnit unit = new HistoryUnit();
                unit.type = type;
                unit.target = target;
                unit.position = position;
                unit.depth = depth;
                unit.flag = active;
                historyTurn.Push(unit);
                break;
            default:
                Debug.LogWarning($"AddHistory(int, Vector2Int, int, bool): type {Enum.GetNames(typeof(Type))[(int)type]} is invalid");
                break;
        }
    }

    // undo a turn
    // returns false if there was no history
    public static void RollBack()
    {
        if (history.Count == 0)
        {
            Debug.LogWarning("RollBack: no history registered to roll back");
            return;
        }
        Stack<HistoryUnit> undoneHistory = history.Pop();
        while (undoneHistory.Count > 0)
        {
            HistoryUnit unit = undoneHistory.Pop();
            MainGame.RollBack(unit);
        }
    }
}
