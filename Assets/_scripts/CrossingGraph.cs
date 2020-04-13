using System;
using System.Collections.Generic;
using UnityEngine;

public class CrossingGraph
{

    public List<Crossing> crossings = new List<Crossing>();
    List<Vector3> nodes = new List<Vector3>();
    public List<Polygon> edges = new List<Polygon>();
    Dictionary<Pair<int>, float> edgeWeight = new Dictionary<Pair<int>, float>();

    public int numCrossings
    {
        get { return crossings.Count; }
    }

    // internal void GetIntersections(Polygon points)
    // {
    //     Crossing startEndCrossing = null;
    //     var start = points[0];
    //     var end = points[points.Count - 1];
    //     if (AreaManager.IsInViewPort(end) && AreaManager.IsInViewPort(start))
    //     {
    //         var center = Vector3.Lerp(start, end, 0.5f);
    //         startEndCrossing = new Crossing(center);
    //         startEndCrossing = AddCrossing(startEndCrossing);
    //         Vector3 Nstart = Nudge2D(center, start);
    //         startEndCrossing.Add(Nstart);
    //         Vector3 Nend = Nudge2D(center, end);
    //         startEndCrossing.Add(Nend);
    //         points.Insert(0, Nstart);
    //         points.Add(Nend);
    //     }
    //     GetIntersections(points);
    // }

    internal void GetIntersections(Polygon points)
    {
        var crossingIndexDict = new Dictionary<Vector3, Crossing>();
        var actEdge = new Polygon();

        int count = points.Count;
        int overflowCounter1 = 0;

        int i = 0;
        while (i < points.Count && overflowCounter1++ < 2 * count)
        {
            Vector3 act = points[i];
            var nextI = i + 1 < points.Count ? i + 1 : 0;
            Vector3 next = points[nextI];

            actEdge.Add(act);
            if (crossingIndexDict.ContainsKey(act))
            {
                var crossing = crossingIndexDict[act];
                Debugger.Log("Add edge intersection at Crossing " + crossing + " as act is " + act.ToString("F4"));
                actEdge.end = crossing;
                if (actEdge.Count > 0)
                    edges.Add(actEdge);
                actEdge = new Polygon();
                actEdge.start = crossing;
            }

            int isIntersectionFound = 0;

            int overflowCounter2 = 0;

            int j = nextI;
            while (j < points.Count && overflowCounter2++ < 2 * count)
            {
                Vector3 act2 = points[j];
                var nextJ = j + 1 < points.Count ? j + 1 : 0;
                Vector3 next2 = points[nextJ];
                if (act != act2 && next != next2)
                {
                    bool thisPairInViewPort = AreaManager.IsInViewPort(act) || AreaManager.IsInViewPort(next);
                    bool otherPairInViewPort = AreaManager.IsInViewPort(act2) || AreaManager.IsInViewPort(next2);
                    if (thisPairInViewPort && otherPairInViewPort)
                    {
                        if (intersects(act, next, act2, next2))
                        {
                            Debugger.Log("Does intersect at " + i + " and " + j + " of " + points.Count + " " + isIntersectionFound);
                            actEdge = IntersectionFound(points, actEdge, i, act, next, j, act2, next2, crossingIndexDict);
                            isIntersectionFound++;
                        }
                    }
                }
                if (isIntersectionFound > 0)
                {
                    j += 4;
                }
                j++;
            }
            if (overflowCounter2 > count * 2 - 2)
            {
                Debug.LogError("Too many points added2");
            }
            if (isIntersectionFound > 0)
            {
                i += isIntersectionFound;
                isIntersectionFound = 0;
            }
            i++;
        }
        if (overflowCounter1 > count * 2 - 2)
        {
            Debug.LogError("Too many points added");
        }
        if (actEdge.Count > 0)
        {
            actEdge.Add(points[points.Count - 1]);
            Debugger.Log("Adding intersection edge " + actEdge);
            edges.Add(actEdge);
        }
        var last = edges[edges.Count - 1];
        var first = edges[0];
        if (crossings.Count > 0)
        {
            last.AddRange(first);
            last.end = first.end;
            edges[0] = last;
            edges.RemoveAt(edges.Count - 1);
        }
        else
        {
            Crossing startEndCrossing = null;
            var start = points[0];
            var end = points[points.Count - 1];
            if (AreaManager.IsInViewPort(end) && AreaManager.IsInViewPort(start))
            {
                var center = Vector3.Lerp(start, end, 0.5f);
                startEndCrossing = new Crossing(center);
                startEndCrossing = AddCrossing(startEndCrossing);
                // Vector3 Nstart = Nudge2D(center, start);
                startEndCrossing.Add(start, start);
                // Vector3 Nend = Nudge2D(center, end);
                startEndCrossing.Add(end, end);
                // points.Insert(0, Nstart);
                // points.Add(Nend);
            }
            first.start = startEndCrossing;
            last.end = startEndCrossing;
        }
    }

    private Polygon IntersectionFound(Polygon points, Polygon actEdge, int i, Vector2 act, Vector2 next, int j, Vector2 act2, Vector2 next2, Dictionary<Vector3, Crossing> crossingIndexDict)
    {
        Vector2 center = FindIntersection(act, next, act2, next2);
        if (AreaManager.IsInViewPort(center))
        {
            var crossing = new Crossing(center);
            var Nact = Nudge2D(center, act);
            var Nnext = Nudge2D(center, next);
            var Nact2 = Nudge2D(center, act2);
            var Nnext2 = Nudge2D(center, next2);
            var nextI = i + 1;
            points.Insert(nextI, Nnext);
            points.Insert(nextI, Nact);
            var nextJ = j + 1 + 2;
            points.Insert(nextJ, Nnext2);
            points.Insert(nextJ, Nact2);
            crossing.Add(Nact, center + (act - center).normalized * 0.05f);
            crossing.Add(Nnext, center + (next - center).normalized * 0.05f);
            crossing.Add(Nact2, center + (act2 - center).normalized * 0.05f);
            crossing.Add(Nnext2, center + (next2 - center).normalized * 0.05f);
            // if ((i == points.Count - 1 || j == points.Count - 1) && startEndCrossing != null)
            //     crossing = MergeCrossings(crossing, startEndCrossing);
            // else
            crossing = AddCrossing(crossing);
            Debugger.Log("Add crossing " + crossing + " at (" + i + "," + nextI + ") and (" + j + "," + nextJ + ")" + " at " + center.ToString("F4"));
            actEdge.end = crossing;
            actEdge.Add(Nact);
            if (actEdge.Count > 0)
                edges.Add(actEdge);
            actEdge = new Polygon();
            actEdge.start = crossing;
            crossingIndexDict[Nact2] = crossing;
        }

        return actEdge;
    }

    /// <summary>
    /// Check if B and A intersect
    /// </summary> 
    /// <param name="bp">B1</param>
    /// <param name="bq">B2</param>
    /// <param name="ap">A1</param>
    /// <param name="aq">A2</param>
    public static bool intersects(Vector2 bp, Vector2 bq, Vector2 ap, Vector2 aq)
    {
        if (ccw(ap, aq, bp) * ccw(ap, aq, bq) >= 0)
            return false;
        if (ccw(bp, bq, ap) * ccw(bp, bq, aq) >= 0)
            return false;
        return true;
    }
    public static float ccw(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
    }

    static Vector3 FindIntersection(Vector3 s1, Vector2 e1, Vector2 s2, Vector2 e2)
    {
        float a1 = e1.y - s1.y;
        float b1 = s1.x - e1.x;
        float c1 = a1 * s1.x + b1 * s1.y;

        float a2 = e2.y - s2.y;
        float b2 = s2.x - e2.x;
        float c2 = a2 * s2.x + b2 * s2.y;

        float delta = a1 * b2 - a2 * b1;
        //If lines are parallel, the result will be (NaN, NaN).
        return delta == 0 ? Vector3.positiveInfinity :
            new Vector3((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta, s1.z);
    }

    public Crossing AddCrossing(Crossing crossing)
    {
        // var threshold = 0.0000f;
        // foreach (var c in crossings)
        // {
        //     if (Vector2.Distance(c.center, crossing.center) <= threshold)
        //     {
        //         Debug.LogWarning("Crossings Merged at new " + crossing + " and old " + c);
        //         return MergeCrossings(crossing, c);
        //     }
        // }
        crossings.Add(crossing);
        return crossing;
    }

    public Crossing MergeCrossings(Crossing crossing, Crossing c)
    {
        // Debugger.Log("Crossing at " + c.center.ToString("F4") + " exists already, so merge " + crossing + " to " + c);
        for (int i = 0; i < crossing.Count; i++)
        {
            var position = crossing.positions[i];
            if (!c.positions.Contains(position))
            {
                c.Add(position, crossing.anglePositions[i]);
            }
        }
        foreach (var edge in edges)
        {
            if (edge.start == crossing)
            {
                edge.start = c;
            }
            if (edge.end == crossing)
            {
                edge.end = c;
            }
        }
        return c;
    }

    [System.Diagnostics.Conditional("MYDEBUG")]
    public void DebugCrossings(Camera cam)
    {
        GameObject[] gameObject = GameObject.FindGameObjectsWithTag("Debug");
        foreach (var item in gameObject)
        {
            GameObject.Destroy(item);
        }
        for (int i = 0; i < crossings.Count; i++)
        {
            DebugSphereCrossing(cam, i);
        }
    }


    [System.Diagnostics.Conditional("MYDEBUG")]
    private void DebugSphereCrossing(Camera cam, int i)
    {
        var z = Vector3.Distance(cam.transform.position, PortalTextureSetup.GetKnotPosition(PortalTextureSetup.actWorld));
        Vector3 vector31 = new Vector3(0f, 0f, z);
        Vector3 vector3 = cam.ViewportToWorldPoint(crossings[i].center + vector31);
        string v = crossings[i].ToString();
        DebugSphere(vector3, v);
        for (int i1 = 0; i1 < crossings[i].anglePositions.Count; i1++)
        {
            Vector3 c = crossings[i].anglePositions[i1];
            DebugSphere(cam.ViewportToWorldPoint(c + vector31), i + ": " + crossings[i].positions[i1].ToString("F4"));
        }
    }

    [System.Diagnostics.Conditional("MYDEBUG")]
    public static void DebugSphere(Vector3 position, string name)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        s.transform.position = position;
        s.name = name;
        s.transform.localScale *= 0.2f;
        s.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        s.tag = "Debug";
    }
    const float cornerZ = 1;
    private const float NudgeSize = 0.001f;
    static Vector3[] corners = new Vector3[] { new Vector3(AreaManager.lowerViewPortBound, AreaManager.lowerViewPortBound, cornerZ), new Vector3(AreaManager.upperViewPortBound, AreaManager.lowerViewPortBound, cornerZ), new Vector3(AreaManager.upperViewPortBound, AreaManager.upperViewPortBound, cornerZ), new Vector3(AreaManager.lowerViewPortBound, AreaManager.upperViewPortBound, cornerZ) };
    Vector2[][] sides = new Vector2[][] { new Vector2[] { corners[0], corners[1] }, new Vector2[] { corners[1], corners[2] }, new Vector2[] { corners[2], corners[3] }, new Vector2[] { corners[3], corners[0] } };
    internal List<Polygon> regions;

    internal bool SetRegions()
    {
        HashSet<Crossing> borderCrossings = GetBorderCrossings();
        if (borderCrossings == null)
        {
            throw new YourCustomException("Put your error message here.");
        }
        bool knotTouchesSide;
        var newEdges = AddScreenEdges(borderCrossings, out knotTouchesSide);
        List<Polygon> doubleEdges = GetDoubleEdges();
        doubleEdges.AddRange(newEdges);
        // NormalizeCrossings();

        DebugCrossings(GameObject.Find("PlayerCamera").GetComponent<Camera>());
        regions = new List<Polygon>();
        var overflowCounter1 = 0;
        var overflowCounter2 = 0;
        while (overflowCounter1++ < 100)
        {
            var edgeString = "";
            int first = GetStartingEdge(doubleEdges);
            if (first == -1)
                break;
            var currentEdge = doubleEdges[first];
            // doubleEdges.RemoveAt(first);

            var startEdge = currentEdge;
            // if (startCrossing == null)
            // Debug.LogWarning("Start crossing of " + currentEdge + " is null");
            // if (endCrossing == null)
            // Debug.LogWarning("End crossing of " + currentEdge + " is null");
            var nudgedEnd = currentEdge[currentEdge.Count - 1];

            bool addRegion = false;
            bool isOuterBoundary = true;
            var region = new Polygon();
            region.Add(startEdge.start.center);
            while (overflowCounter2++ < 100)
            {
                region.AddRange(currentEdge);
                region.Add(currentEdge.end.center);
                edgeString += currentEdge + "; ";
                if (!currentEdge.isScreenEdge)
                    isOuterBoundary = false;

                var left = GetLeft(currentEdge.end, GetAngleEnd(currentEdge));
                int firstEdge = GetEdgeWithStart(doubleEdges, currentEdge.end, left);
                Debugger.Log("--Add " + currentEdge + ", firstEdge is " + (firstEdge > -1 ? doubleEdges[firstEdge].ToString() : "-") + " and startEdge is " + startEdge);
                if (startEdge.start.Equals(currentEdge.end) && (firstEdge == -1 || doubleEdges[firstEdge].Equals(startEdge)))
                {
                    addRegion = true;
                    doubleEdges.RemoveAt(first);
                    break;
                }
                else if (firstEdge != -1)
                {
                    currentEdge = doubleEdges[firstEdge];
                    doubleEdges.RemoveAt(firstEdge);

                    nudgedEnd = currentEdge[currentEdge.Count - 1];
                }
                else
                {
                    Debugger.Log("Error, as " + left.ToString("F4") + " and " + firstEdge);
                    foreach (var edge in doubleEdges)
                    {
                        Debugger.Log(edge.ToString());
                    }
                    throw new YourCustomException("Put your error message here.");
                }
            }
            if (overflowCounter2 > 99)
                Debug.LogError("Counter2");
            else if (isOuterBoundary)
            {
                Debugger.Log("Region is Screen edge, so add hole");
                region.isScreenEdge = true;
            }
            else if (addRegion)
            {
                Debugger.Log("Region done: " + edgeString);
                regions.Add(region);
            }

        }
        if (overflowCounter1 > 99)
            Debug.LogError("Counter1");
        return knotTouchesSide;
    }

    private Vector2 GetAngleEnd(Polygon currentEdge)
    {
        var edgeEnd = currentEdge[currentEdge.Count - 1];
        var index = currentEdge.end.positions.IndexOf(edgeEnd);
        if (index == -1)
        {
            Debug.LogError("Has no edgeEnd named " + edgeEnd.ToString("F4") + " in " + currentEdge.end);
            throw new YourCustomException("Put your error message here.");
        }
        return currentEdge.end.anglePositions[index];
    }

    private HashSet<Crossing> GetBorderCrossings()
    {
        var borderCrossings = new HashSet<Crossing>();
        {
            int j = 0;
            while (j < crossings.Count)
            {
                Crossing crossing = crossings[j];
                if (!AreaManager.IsInViewPort(crossing.center))
                {
                    crossings.RemoveAt(j);
                }
                else
                {
                    int i = 0;
                    var counter = 0;
                    while (i < crossing.Count)
                    {
                        Vector2 pos = crossing.positions[i];
                        if (!AreaManager.IsInViewPort(pos))
                        {
                            counter++;
                            // Debugger.Log("Remove " + pos.ToString("F4") + " from " + crossing);
                            borderCrossings.Add(crossing);
                            crossing.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                    j++;
                    if (counter % 2 != 0)
                    {
                        Debug.LogWarning("Removed an uneven number of positions");
                        // return null;
                    }
                }
            }
        }

        return borderCrossings;
    }

    //By Kahan
    private float Angle360(Vector2 v1, Vector2 v2)
    {
        var n1 = v1.magnitude;
        var n2 = v2.magnitude;
        float angle = Mathf.Rad2Deg * 2 * Mathf.Atan2((v1 * n2 - n1 * v2).magnitude, (v1 * n2 + n1 * v2).magnitude);
        if (v1.x * v2.y - v1.y * v2.x < 0)
        {
            angle = 360f - angle;
        }
        angle = 360f - angle;
        if (Mathf.Abs(angle - 360f) < NudgeSize)
        {
            return 0f;
        }
        return angle;
    }

    private int GetStartingEdge(List<Polygon> edges)
    {
        // return edges.Count > 0 ? 0 : -1;
        for (int i = 0; i < edges.Count; i++)
        {
            if (!edges[i].isScreenEdge)
            {
                return i;
            }
        }
        return -1;
    }

    private List<Polygon> AddScreenEdges(HashSet<Crossing> borderCrossings, out bool knotTouchesSide)
    {
        var doubleEdges = new List<Polygon>();
        var sideList = new List<List<Crossing>>();
        for (int i = 0; i < corners.Length; i++)
        {
            var cornerCrossing = new Crossing(corners[i]);
            var list = new List<Crossing>();
            list.Add(cornerCrossing);
            sideList.Add(list);
        }
        // {
        //     var deleteEdges = new List<Polygon>();
        //     foreach (var edge in edges)
        //     {
        //         if (edge.Count == 1)
        //         {
        //             Debugger.Log("Look at " + edge);
        //             if (edge.start == null && edge.end == null)
        //             {
        //                 deleteEdges.Add(edge);
        //             }
        //         }
        //     }
        //     foreach (var edge in deleteEdges)
        //     {
        //         edges.Remove(edge);
        //     }
        // }
        foreach (var crossing in borderCrossings)
        {
            var center = crossing.center;
            var side = GetSide(center);
            SortInSide(sideList, crossing, side);
        }
        foreach (var edge in edges)
        {
            if (edge.start == null)
            {
                var entryVector = edge[0];
                var entrySide = GetSide(entryVector);

                var crossing = new Crossing(entryVector);
                crossing = AddCrossing(crossing);
                edge.start = crossing;
                edge.RemoveAt(0);
                crossing.Add(edge[0], edge[0]);
                Debugger.Log("Set start of edge " + edge + " to " + crossing);

                SortInSide(sideList, crossing, entrySide);
            }
            if (edge.end == null)
            {
                var exitVector = edge[edge.Count - 1];
                var exitSide = GetSide(exitVector);

                var crossing = new Crossing(exitVector);
                crossing = AddCrossing(crossing);
                edge.end = crossing;

                edge.RemoveAt(edge.Count - 1);
                crossing.Add(edge[edge.Count - 1], edge[edge.Count - 1]);
                Debugger.Log("Set end of edge " + edge + " to " + crossing);

                SortInSide(sideList, crossing, exitSide);
            }
        }
        knotTouchesSide = false;
        foreach (var side in sideList)
        {
            if (side.Count > 1)
            {
                knotTouchesSide = true;
            }
        }
        if (knotTouchesSide)
        {
            Debugger.Log("Knot touches side");
            for (int i = 0; i < sideList.Count; i++)
            {
                List<Crossing> crossingsOnSide = sideList[i];
                for (int j = 0; j < crossingsOnSide.Count; j++)
                {
                    var sidePart = new Polygon();
                    List<Crossing> beforeList = sideList[Before(sideList.Count, i)];
                    var before = j > 0 ? crossingsOnSide[j - 1] : beforeList[beforeList.Count - 1];
                    var next = j + 1 < crossingsOnSide.Count ? crossingsOnSide[j + 1] : sideList[Next(sideList.Count, i)][0];
                    var thisCenter = crossingsOnSide[j].center;
                    var nextCenter = next.center;
                    if (borderCrossings.Contains(crossingsOnSide[j]))
                    {
                        thisCenter = SetOnSide(thisCenter);
                    }
                    if (borderCrossings.Contains(next))
                    {
                        nextCenter = SetOnSide(nextCenter);
                    }
                    var nudgeStart = Nudge2D(thisCenter, nextCenter);
                    var nudgeEnd = Nudge2D(nextCenter, thisCenter);
                    sidePart.Add(nudgeStart);
                    sidePart.Add(nudgeEnd);
                    sidePart.isScreenEdge = true;
                    sidePart.start = crossingsOnSide[j];
                    sidePart.end = next;

                    Polygon sidePartR = new Polygon(sidePart);
                    sidePartR.ReverseSE();
                    sidePartR.isScreenEdge = true;

                    if (sidePart.start != sidePart.end)//crossings.Contains(sidePart.start) && crossings.Contains(sidePart.end) &&
                    {
                        crossingsOnSide[j].Add(nudgeStart, nextCenter);
                        next.Add(nudgeEnd, thisCenter);
                        doubleEdges.Add(sidePart);
                        // if (knotTouchesSide)
                        doubleEdges.Add(sidePartR);
                    }
                }
            }
        }
        else
        {
            // Debugger.Log("Knot does not touch side");
        }
        foreach (var newCrossings in sideList)
        {
            foreach (var crossing in newCrossings)
            {
                AddCrossing(crossing);
            }
        }

        {
            var deleteEdges = new List<Polygon>();
            foreach (var edge in edges)
            {
                if (edge.Count == 1 && edge.start.Equals(edge.end))
                {
                    // Debugger.Log("Look at " + edge);
                    foreach (var crossing in crossings)
                    {
                        crossing.Remove(edge[0]);
                    }
                    deleteEdges.Add(edge);
                }
            }
            foreach (var edge in deleteEdges)
            {
                edges.Remove(edge);
            }
        }
        // foreach (var crossing in crossings)
        // {
        //     Debugger.Log("Crossing: " + crossing);
        // }
        // foreach (var edge in edges)
        // {
        //     Debugger.Log("Edge: " + edge + " from " + edge.start + " to " + edge.end);
        // }
        // foreach (var edge in doubleEdges)
        // {
        //     Debugger.Log("NEdge: " + edge + " from " + edge.start + " to " + edge.end);
        // }
        return doubleEdges;
    }

    private Vector3 SetOnSide(Vector3 center)
    {
        var side = GetSide(center);
        Vector3 onSide;
        if (side == 0)
        {
            onSide = new Vector3(center.x, 0f, center.z);
        }
        else if (side == 1)
        {
            onSide = new Vector3(1f, center.y, center.z);
        }
        else if (side == 2)
        {
            onSide = new Vector3(center.x, 1f, center.z);
        }
        else
        {
            onSide = new Vector3(0f, center.y, center.z);
        }
        return onSide;
    }

    private int Before(int count, int i)
    {
        if (i > 0)
        {
            return i - 1;
        }
        else
        {
            return count - 1;
        }
    }

    private static Vector2 Nudge2D(Vector2 c, Vector2 next)
    {
        if (NudgeSize > Vector2.Distance(c, next))
        {
            // Debugger.Count1++;
            return Vector2.Lerp(c, next, NudgeSize);
        }
        else
        {
            // Debugger.Count2++;
            return c + (next - c).normalized * NudgeSize;
        }
        // return Vector2.Lerp(c, next, NudgeSize);
    }

    private static int Next(int count, int i)
    {
        if (i + 1 < count)
        {
            return i + 1;
        }
        else
        {
            return 0;
        }
    }

    private List<Polygon> GetDoubleEdges()
    {
        var newEdges = new List<Polygon>(2 * edges.Count);
        foreach (var edge in edges)
        {
            newEdges.Add(edge);

            var edgeCopy = new Polygon(edge);

            edgeCopy.ReverseSE();
            newEdges.Add(edgeCopy);
        }

        return newEdges;
    }

    private int GetEdgeWithStart(List<Polygon> edges, Crossing c, Vector3 start)
    {
        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            if (edge.start.Equals(c))
            {
                var first = edge[0];
                if (first.x == start.x && first.y == start.y)
                {
                    return i;
                }
            }
        }
        // Debug.LogWarning(start.ToString("F4") + " does not constitute a first element of any edge");
        return -1;
    }

    private Vector3 GetLeft(Crossing crossing, Vector2 comingIn)
    {
        var minAngleVal = float.MaxValue;
        var minAnglePos = -1;
        var minAngleVal2 = float.MaxValue;
        var minAnglePos2 = -1;
        // var s = "--Angles are: ";
        for (int i = 0; i < crossing.Count; i++)
        {
            var position = crossing.anglePositions[i];
            Vector2 center = crossing.center;
            var angle = Angle360(comingIn - center, position - center);
            if (angle < minAngleVal)
            {
                minAngleVal2 = minAngleVal;
                minAngleVal = angle;
                minAnglePos2 = minAnglePos;
                minAnglePos = i;
            }
            else if (angle < minAngleVal2)
            {
                minAngleVal2 = angle;
                minAnglePos2 = i;
            }
            // if (Debugger.DebugMode)
            // {
            // s += comingIn.ToString("F4") + " to " + position.ToString("F4") + ": " + angle + "; ";
            // }
        }
        if (minAnglePos2 == -1)
        {
            // Debug.LogWarning("In Crossing " + crossing.ToString() + ", there is no vector closer to " + comingIn.ToString("F4"));
        }
        // Debug.Log ("center " + crossing.center.ToString ("F4"));
        // Debugger.Log(s + " so min is " + minAngleVal2 + " for " + crossing.positions[minAnglePos2].ToString("F4"));
        return crossing.positions[minAnglePos2];
    }

    internal void CutUpEdges()
    {
        var newEdges = new List<Polygon>();
        var currentEdge = new Polygon();
        var justChanged = false;
        for (int i1 = 0; i1 < edges.Count; i1++)
        {
            Polygon edge = edges[i1];

            // Debugger.Log("Cut up edge " + edge + " from " + edge.start + " to " + edge.end);
            if (AreaManager.IsInViewPort(edge[0]))
            {
                // Debugger.Log("Set currentEdge start to " + edge.start);
                currentEdge.start = edge.start;
            }
            var edgeBefore = i1 == 0 ? edges[edges.Count - 1] : edges[i1 - 1];
            Vector3 pointBefore = edgeBefore[edgeBefore.Count - 1];
            Vector3 point;
            var endIntersectionSet = false;
            for (int i = 0; i < edge.Count; i++)
            {
                point = edge[i];
                bool isIn = AreaManager.IsInViewPort(point);
                if (isIn)
                {
                    if (currentEdge.Count == 0 && !AreaManager.IsInViewPort(pointBefore))
                    {
                        var intersectionPoint = GetSideIntersection(pointBefore, point);
                        // if (currentEdge.start != null)
                        // {
                        //     var positions = currentEdge.start.positions;
                        //     positions.Remove((Vector2)edge[0]);
                        //     positions.Add(intersectionPoint);
                        // }
                        if (currentEdge.start == null)
                        {
                            currentEdge.Add(intersectionPoint);
                        }
                    }
                    currentEdge.Add(point);
                    endIntersectionSet = false;
                    justChanged = true;
                }
                else if (justChanged)
                {
                    justChanged = false;
                    var intersectionPoint = GetSideIntersection(pointBefore, point);
                    endIntersectionSet = true;
                    currentEdge.Add(intersectionPoint);
                    // Debugger.Log("Add Edge " + currentEdge + " from " + currentEdge.start + " to " + currentEdge.end);
                    newEdges.Add(currentEdge);
                    currentEdge = new Polygon();
                }
                pointBefore = point;
            }
            if (currentEdge.Count > 0)
            {
                if (AreaManager.IsInViewPort(currentEdge[currentEdge.Count - 1]))
                {
                    // Debugger.Log("Set Edge " + currentEdge + " end to " + edge.end);
                    currentEdge.end = edge.end;
                    // if (currentEdge.end != null)
                    // {
                    // }
                    if (currentEdge.end != null && endIntersectionSet && !currentEdge.end.positions.Contains((Vector2)currentEdge[currentEdge.Count - 1]))
                    {
                        currentEdge.RemoveAt(currentEdge.Count - 1);
                        //     currentEdge.end.positions.Remove((Vector2)edge[edge.Count - 1]);
                        //     currentEdge.end.positions.Add(currentEdge[currentEdge.Count - 1]);
                    }
                }
                newEdges.Add(currentEdge);
            }
            endIntersectionSet = false;
            justChanged = false;
            currentEdge = new Polygon();
        }
        newEdges.RemoveAll(l => l.Count == 0);
        edges = newEdges;
    }

    private Vector2 GetSideIntersection(Vector2 pointBefore, Vector2 point)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2 ap = sides[i][0];
            Vector2 aq = sides[i][1];
            if (intersects(pointBefore, point, ap, aq))
            {
                var intersection = FindIntersection(pointBefore, point, ap, aq);
                return intersection;
            }
        }
        Debug.LogWarning("No intersection with sides found");
        return Vector2.zero;
    }

    private bool SortInSide(List<List<Crossing>> sideList, Crossing c, int entrySide)
    {
        float thisDist = Vector2.Distance(corners[entrySide], c.center);
        int i;
        for (i = 0; i < sideList[entrySide].Count; i++)
        {
            var point = sideList[entrySide][i].center;
            var dist = Vector2.Distance(corners[entrySide], point);
            if (dist > thisDist)
            {
                sideList[entrySide].Insert(i, c);
                return true;
            }
        }
        sideList[entrySide].Add(c);
        return false;
    }

    private int GetSide(Vector3 v)
    {
        var sideDistances = new float[4];
        for (int i = 0; i < 4; i++)
        {
            sideDistances[i] = distanceToSegment(v, sides[i][0], sides[i][1]);
        }
        var minDist = float.MaxValue;
        var min = -1;
        for (int i = 0; i < 4; i++)
        {
            if (sideDistances[i] < minDist)
            {
                minDist = sideDistances[i];
                min = i;
            }
        }
        // if (min == 0) {
        //     onSide = new Vector3 (v.x, 0f, v.z);
        // } else if (min == 1) {
        //     onSide = new Vector3 (1f, v.y, v.z);
        // } else if (min == 2) {
        //     onSide = new Vector3 (v.x, 1f, v.z);
        // } else {
        //     onSide = new Vector3 (0f, v.y, v.z);
        // }
        return min;
    }

    public static float distanceToSegment(Vector3 v, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        Vector3 av = v - a;

        if (Vector3.Dot(av, ab) <= 0f) // Point is lagging behind start of the segment, so perpendicular distance is not viable.
            return av.magnitude; // Use distance to start of segment instead.

        Vector3 bv = v - b;

        if (Vector3.Dot(bv, ab) >= 0.0) // Point is advanced past the end of the segment, so perpendicular distance is not viable.
            return bv.magnitude; // Use distance to end of the segment instead.

        return Vector3.Cross(ab, av).magnitude / ab.magnitude; // Perpendicular distance of point to segment.
    }

    internal void SetScreenRegion()
    {
        regions = new List<Polygon>();
        Polygon cornerRect = new Polygon(4);
        foreach (var corner in corners)
        {
            cornerRect.Add(corner);
        }
        regions.Add(cornerRect);
    }
}