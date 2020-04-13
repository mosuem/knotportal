using System;
using System.Collections;
using System.Collections.Generic;
using Curve;
using UnityEngine;

public class Knot
{
    public GameObject knotObject;
    public List<Vector3> points;
    private const float nSegments = 100f;
    private const float radius = 0.05f;
    private const int radialSegments = 16;

    public Knot(Vector3 position, List<Vector3> controls, Material mat, bool enableRenderer)
    {
        points = controls;

        knotObject = new GameObject("Knot");
        knotObject.transform.position = position;
        var mf = knotObject.AddComponent<MeshFilter>();

        var curve = new CatmullRomCurve(points);

        // Build tubular mesh with Curve
        int tubularSegments = points.Count;
        var mesh = Tubular.Tubular.Build(curve, tubularSegments, radius, radialSegments, true);

        // visualize mesh
        mf.sharedMesh = mesh;
        var renderer = knotObject.AddComponent<MeshRenderer>();
        renderer.enabled = enableRenderer;
        renderer.material = mat;
        for (int i = 0; i < points.Count; i++)
        {
            points[i] += position;
        }
    }

    internal static Knot BuildKnot(Vector3 position, Material KnotMaterial, bool buildUp)
    {
        Material mat = KnotMaterial;

        var controls = new List<Vector3>();
        for (float i = 0f; i < nSegments; i++)
        {
            float v = i / nSegments;
            var t = v * Mathf.PI * 2f + UnityEngine.Random.Range(-0.001f, 0.001f);
            Vector3 item;
            if (PortalTextureSetup.knotType == KnotType.Trefoil)
            {
                item = TrefoilParameters(t);
            }
            else if (PortalTextureSetup.knotType == KnotType.Figureeight)
            {
                item = FigureEightParameters(t);
            }
            else if (PortalTextureSetup.knotType == KnotType.Twisted)
            {
                item = TwistedParameters(t);
            }
            else
            {
                item = UnknotParameters(t);
            }
            controls.Add(item);
        }
        return new Knot(position, controls, mat, !buildUp);
    }


    private static Vector3 TrefoilParameters(float t)
    {
        var x = Mathf.Sin(t) + 2 * Mathf.Sin(2 * t);
        var y = Mathf.Cos(t) - 2 * Mathf.Cos(2 * t)-1.2f;
        var z = -Mathf.Sin(3 * t);
        Vector3 item = new Vector3(x, y, z) * 0.4f;
        return item;
    }

    private static Vector3 FigureEightParameters(float t)
    {
        var x = (2 + Mathf.Cos(2 * t)) * Mathf.Cos(3 * t);
        var y = (2 + Mathf.Cos(2 * t)) * Mathf.Sin(3 * t);
        var z = Mathf.Sin(4 * t);
        Vector3 item = new Vector3(x, y, z) * 0.5f;
        return item;
    }

    private static Vector3 UnknotParameters(float t)
    {
        var x = 0.8f * Mathf.Sin(t);
        var y = 1.5f * Mathf.Cos(t);
        var z = 0f;
        Vector3 item = new Vector3(x, y, z);
        return item;
    }

    private static Vector3 TwistedParameters(float t)
    {
        var x = 2f * Mathf.Sin(t + 1f);
        var y = 3f * Mathf.Sin(t + 1f) * Mathf.Cos(t + 1f);
        var z = 1f * Mathf.Sin(t);
        Vector3 item = new Vector3(x, y, z);
        return item;
    }
}

