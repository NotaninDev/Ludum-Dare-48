using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// edge can be of any length, but in practical length 1
public class Edge
{
    public Vector2Int Position { get; private set; }
    public readonly bool Horizontal;
    public int Depth { get; private set; }
    public bool Active { get; private set; }

    public Edge(Vector2Int position, bool horizontal, int depth = 0, bool active = true)
    {
        Position = position;
        Horizontal = horizontal;
        Depth = depth;
        Active = active;
    }
    public Edge Clone()
    {
        return (Edge)this.MemberwiseClone();
    }
    public void ChangePosition(Vector2Int position)
    {
        Position = position;
    }
    public void ChangeDepth(int depth)
    {
        Depth = depth;
    }
    public void SetActive(bool active)
    {
        Active = active;
    }
}
