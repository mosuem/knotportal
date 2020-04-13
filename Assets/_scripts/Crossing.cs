using System;
using System.Collections.Generic;
using UnityEngine;

public class Crossing
{
    public Vector3 center;
    public List<Vector2> positions = new List<Vector2>();
    public List<Vector2> anglePositions = new List<Vector2>();
    public Crossing(Vector3 center)
    {
        this.center = center;
    }

    public int Count { get { return positions.Count; } }

    public void Add(Vector3 pos, Vector3 anglePos)
    {
        if (!positions.Contains(pos))
        {
            positions.Add(pos);
            anglePositions.Add(anglePos);
        }
    }

    public override string ToString()
    {
        var s = "C: " + center.ToString("F4") + ": ";
        for (int i = 0; i < Count; i++)
        {
            s += positions[i].ToString("F4");
            if (i + 1 < Count)
            {
                s += "; ";
            }
        }
        return s;
    }

    public override int GetHashCode()
    {
        return this.center.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return this.center.Equals(((Crossing)obj)?.center);
    }

    internal void RemoveAt(int i)
    {
        positions.RemoveAt(i);
        anglePositions.RemoveAt(i);
    }

    internal void Remove(Vector3 vector3)
    {
        var index = positions.IndexOf(vector3);
        if (index > -1)
        {
            positions.RemoveAt(index);
            anglePositions.RemoveAt(index);
        }
    }
}