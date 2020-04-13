using System;
using System.Collections.Generic;
using UnityEngine;

public class Region : List<int>
{

    public Vector3 GetCenter(List<Vector3> points)
    {
        var sum = Vector3.zero;
        foreach (var index in this)
        {
            sum += points[index];
        }
        return sum / Count;
    }
}