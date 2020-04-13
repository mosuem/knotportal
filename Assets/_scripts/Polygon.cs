using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polygon : List<Vector3>
{

    public Crossing start;
    public Crossing end;

    public Polygon() : base() { }
    public bool isClosed = false;

    public bool atCorner = false;
    internal bool isScreenEdge = false;

    public Polygon(int capacity) : base(capacity) { }

    public Polygon(Polygon collection) : base(collection)
    {
        this.start = collection.start;
        this.end = collection.end;
    }

    internal Vector3 GetPrevious(int index)
    {
        if (index - 1 < 0)
        {
            return this.Last();
        }
        else
        {
            return this[index - 1];
        }
    }

    internal Vector3 GetPrevious(Vector3 v)
    {
        return GetPrevious(IndexOf(v));
    }

    internal void ReverseSE()
    {
        this.Reverse();
        var t = start;
        start = end;
        end = t;
    }

    public override string ToString()
    {
        if (this.Count > 1)
        {
            return ((Vector2)this[0]).ToString("F4") + " -" + Count + "- " + ((Vector2)this[Count - 1]).ToString("F4");
        }
        else if (this.Count == 1)
        {
            return "-" + this[0].ToString("F4") + "-";
        }
        else
        {
            return "-Empty-";
        }
    }
}