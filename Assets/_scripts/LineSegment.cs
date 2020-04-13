using System;
using UnityEngine;

public class LineSegment
{
    public int p1index;

    public int p2index;
    public UnityEngine.Vector3 p1;
    public UnityEngine.Vector3 p2;

    public LineSegment(int p1i, Vector3 p1, int p2i, Vector3 p2new)
    {
        this.p1index = p1i;
        this.p1 = p1;
        this.p2index = p2i;
        this.p2 = p2new;
    }

    internal bool Similar(LineSegment l1)
    {
        if (l1.p1index == p1index && l1.p2index == p2index)
        {
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return p1index + "-" + p2index;
    }

    internal LineSegment Reversed()
    {
        return new LineSegment(p2index, p2, p1index, p1);
    }
}