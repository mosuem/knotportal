using System.Collections.Generic;
using UnityEngine;

internal class Cone
{

    internal static void doCone(Knot knot, int world)
    {
        var startPosition = PortalTextureSetup.GetStartPosition(world);
        int count = knot.points.Count;
        //Find Crossings
        List<LineSegment> crossings = new List<LineSegment>();
        for (int i = 0; i < count - 1; i++)
        {
            var p1 = knot.points[i];
            var p2 = knot.points[i + 1];
            var planeNormal = Vector3.Cross(p2 - p1, p1 - startPosition);
            for (int j = i + 2; j < count - 1; j++)
            {
                var q1 = knot.points[j];
                var q2 = knot.points[j + 1];
                var planeNormal2 = Vector3.Cross(q2 - q1, q1 - startPosition);
                bool qOnTop;
                Vector3 crossPoint;
                Vector3 crossPoint2;
                var doesIntersect = intersect3D_SegmentPlane(q1, q2, planeNormal, p1, out crossPoint);
                var doesIntersect2 = intersect3D_SegmentPlane(p1, p2, planeNormal2, q1, out crossPoint2);
                if (doesIntersect == 1 && doesIntersect2 == 1)
                {
                    // DebugSphere (intersection, Color.red, "Intersection", false);
                    // var crossPoint = q1 - (q2 - q1) * t;
                    qOnTop = Vector3.Distance(crossPoint, startPosition) > Vector3.Distance(crossPoint2, startPosition);
                    if (qOnTop)
                    {
                        crossPoint = crossPoint2;
                    }
                    LineSegment line;
                    // Debug.Log ("Cross at " + i + ", " + (i + 1) + " with" + j + ", " + (j + 1));
                    // DebugSphere (crossPoint, Color.black, "Cross point", false);
                    if (qOnTop)
                    {
                        line = new LineSegment(i, crossPoint, i + 1, p2);
                    }
                    else
                    {
                        line = new LineSegment(j, crossPoint, j + 1, q2);
                    }
                    crossings.Add(line);
                }
            }
        }
        for (int i = 0; i < crossings.Count; i++)
        {
            LineSegment cross = crossings[i];
            knot.points.Insert(knot.points.IndexOf(cross.p2), cross.p1);
        }
        var crossIndices = new List<int>();
        foreach (var cross in crossings)
        {
            crossIndices.Add(knot.points.IndexOf(cross.p1));
        }
        BuildCones(knot, world, crossIndices);
    }

    // intersect3D_SegmentPlane(): find the 3D intersection of a segment and a plane
    //    Input:  S = a segment, and Pn = a plane = {Point V0;  Vector n;}
    //    Output: *I0 = the intersect point (when it exists)
    //    Return: 0 = disjoint (no intersection)
    //            1 =  intersection in the unique point *I0
    //            2 = the  segment lies in the plane
    private static int intersect3D_SegmentPlane(Vector3 q0, Vector3 q1, Vector3 Pn, Vector3 Pv, out Vector3 I)
    {
        I = default(Vector3);
        Vector3 u = q1 - q0;
        Vector3 w = q0 - Pv;

        float D = Vector3.Dot(Pn, u);
        float N = -Vector3.Dot(Pn, w);

        if (Mathf.Abs(D) == 0)
        { // segment is parallel to plane
            if (N == 0) // segment lies in plane
                return 2;
            else
                return 0; // no intersection
        }
        // they are not parallel
        // compute intersect param
        float sI = N / D;
        if (sI < 0 || sI > 1)
            return 0; // no intersection
        I = q0 + sI * u; // compute segment intersect point
        if (Vector3.Distance(I, Pv) > 1)
            return 0;
        return 1;
    }

    private static void BuildCones(Knot knot, int world, List<int> crossings)
    {
        for (int index = 0; index < crossings.Count; index++)
        {
            BuildConePart(knot, crossings, false, world, index);
            BuildConePart(knot, crossings, true, world, index);
        }
        if (crossings.Count == 0)
        {
            BuildConePart(knot, crossings, false, world);
            BuildConePart(knot, crossings, true, world);
        }
    }

    private static void BuildConePart(Knot knot, List<int> crossings, bool outer, int world, int i1 = 0)
    {
        var crossingPoint = -1;
        if (crossings.Count > 0)
        {
            crossingPoint = crossings[i1];
        }
        var meshObject = new GameObject("Cone");

        var generator = KnotInfos.getConeComponent(i1);
        if (outer)
            meshObject.name = KnotInfos.GetInverse(generator);
        else
            meshObject.name = generator;
        meshObject.tag = "Generator";
        meshObject.layer = LayerMask.NameToLayer("Generator");
        var mf = meshObject.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        mf.mesh = mesh;
        mesh.Clear();
        var vertexList = new List<Vector3>();
        var startPosition = PortalTextureSetup.GetStartPosition(world);
        vertexList.Add(startPosition);
        if (crossingPoint != -1)
        {
            int i = crossingPoint + 1;
            var counter = 0;
            vertexList.Add(knot.points[crossingPoint]);
            while (!isEndPoint(i, crossings) && counter++ < 500)
            {
                vertexList.Add(knot.points[i]);
                i = i == knot.points.Count - 1 ? 0 : i + 1;
            }
            vertexList.Add(knot.points[i]);
            if (counter > 499)
            {
                Debug.LogError("No end point");
            }
        }
        else
        {
            vertexList.AddRange(knot.points);
        }

        // Create the mesh
        int numVertices = vertexList.Count;
        var vertices = vertexList.ToArray();
        mesh.vertices = vertices;
        int[] triangles = new int[(numVertices - 2) * 3];
        var c = 0;
        for (int i = 1; i < numVertices - 1; i++)
        {
            triangles[c++] = 0;
            var next = i + 1;
            if (crossingPoint == -1 || crossings.Count == 1)
            { //is closed
                next = i + 1 < numVertices - 1 ? i + 1 : 1;
            }
            if (outer)
            {
                triangles[c++] = i;
                triangles[c++] = next;
            }
            else
            {
                triangles[c++] = next;
                triangles[c++] = i;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        // meshObject.AddComponent<MeshRenderer>();
        var coll = meshObject.AddComponent<MeshCollider>();
        coll.sharedMesh = mesh;
    }

    private static bool isEndPoint(int index, List<int> crossings)
    {
        foreach (var crossing in crossings)
        {
            if (index == crossing)
            {
                return true;
            }
        }
        return false;
    }
}