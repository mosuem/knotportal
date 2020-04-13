using System.Collections.Generic;
using UnityEngine;

public class AreaManager
{
    public static float lowerViewPortBound = 0f;
    public static float upperViewPortBound = 1f;

    public static bool IsInViewPort(Vector3 p)
    {
        return p.x >= lowerViewPortBound && p.x <= upperViewPortBound && p.y >= lowerViewPortBound && p.y <= upperViewPortBound;
    }

    internal static List<string> AssignGenerators(DualCamera actPlayer, List<Vector2> pointsInRegions, Vector3 startposition, bool knotTouchesSide, List<float> precisions)
    {
        var actWorld = PortalTextureSetup.actWorld;
        var g = KnotInfos.getGenerator(actWorld);
        List<string> list = new List<string>();

        //Raycast the points
        Vector3 cameraPos = actPlayer.gameObject.transform.position - PortalTextureSetup.getOffset(actWorld);
        var distToStart = Vector3.Distance(cameraPos, PortalTextureSetup.GetStartPosition(actWorld));
        var distToKnot = Vector3.Distance(cameraPos, PortalTextureSetup.GetKnotPosition(actWorld));
        var maxDistance = Mathf.Max(distToStart, distToKnot, 10f);
        for (int i = 0; i < pointsInRegions.Count; i++)
        {
            Vector3 point = pointsInRegions[i];
            var worldPoint = actPlayer.ViewportToWorldPoint(point + Vector3.forward);
            var pointWorld = worldPoint - PortalTextureSetup.getOffset(actWorld);
            list.Add(GetPointGenerator(cameraPos, pointWorld, g, actWorld, maxDistance, precisions[i]));
            // if (Debugger.DebugMode)
            // {
            //     var debugString = "";
            //     foreach (var hit in hits)
            //     {
            //         debugString += hit.collider.gameObject.name + ", ";
            //         PortalTextureSetup.DebugSphere(hit.point, Color.black, "Region " + i + " hit");
            //     }
            //     Debugger.Log("The Region " + i + " has generator " + list.Last() + " with " + debugString);
            // }
        }
        if (!knotTouchesSide)
        {
            Vector3 point = new Vector3(0, 0, 0);
            var worldPoint = actPlayer.ViewportToWorldPoint(point + Vector3.forward);
            var pointWorld = worldPoint - PortalTextureSetup.getOffset(actWorld);
            list.Add(GetPointGenerator(cameraPos, pointWorld, g, actWorld, maxDistance, 0.0001f));
        }

        return list;
    }

    private static string GetPointGenerator(Vector3 a, Vector3 b, string generator, int actWorld, float maxDistance, float precision)
    {
        string preGen = generator;
        var generators = new Dictionary<string, int>();
        for (int i = 0; i < 5; i++)
        {
            var bPertubated = b + UnityEngine.Random.onUnitSphere * Mathf.Min(0.01f, precision);
            Vector3 directionPertubated = bPertubated - a;
            Debug.DrawRay(a, directionPertubated.normalized * maxDistance, Color.red, 0.1f);
            var genCandidate = GetGenerator(a, directionPertubated, maxDistance, preGen);
            if (generators.ContainsKey(genCandidate))
            {
                generators[genCandidate]++;
            }
            else
            {
                generators[genCandidate] = 1;
            }
        }
        var max = -1;
        var maxGen = "";
        foreach (var gen in generators.Keys)
        {
            if (generators[gen] > max)
            {
                max = generators[gen];
                maxGen = gen;
            }
        }
        return maxGen;
    }

    private static string GetGenerator(Vector3 a, Vector3 direction, float maxDistance, string preGen)
    {
        var hits = RaycastAllGenerators(a, direction, maxDistance);
        foreach (var hit in hits)
        {
            string hitGen = hit.collider.gameObject.name;
            preGen = KnotInfos.multiply(preGen, hitGen);
        }
        return preGen;
    }

    //Raycast All that works with non-convex mesh colliders
    public static List<RaycastHit> RaycastAllGenerators(Vector3 origin, Vector3 direction, float maxDistance)
    {
        var hits = new List<RaycastHit>();
        var clear = false;
        int m = LayerMask.GetMask("Generator");
        var counter = 0;
        Vector3 dir = direction.normalized;
        var savedOrigin = origin;
        var savedMaxDistance = maxDistance;
        while (!clear && counter++ < 10)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, dir, out hit, maxDistance, m))
            {
                var dist = Vector3.Distance(hit.point, origin);
                hits.Add(hit);
                origin += (dist + 0.0001f) * dir;
                maxDistance -= dist;
            }
            else
            {
                clear = true;
            }
        }
        if (counter > 9)
        {
            Debug.LogWarning("Raycast has gone too far " + maxDistance.ToString("F4") + " hits: " + hits.Count);
            var unsortedHits = Physics.RaycastAll(savedOrigin, dir, savedMaxDistance, m);
            var hits2 = SortHits(unsortedHits);
            return hits2;
            // foreach (var hit in hits)
            // {
            //     Debug.LogError(hit.collider.gameObject.name);
            // }
        }
        return hits;
    }

    public static List<RaycastHit> SortHits(RaycastHit[] hits, bool increasing = true)
    {
        var sortedHits = new List<RaycastHit>();
        for (int i = 0; i < hits.Length; i++)
        {
            var min = float.MaxValue;
            if (!increasing)
                min = float.MinValue;
            RaycastHit candidate = hits[0];
            for (int j = 0; j < hits.Length; j++)
            {
                RaycastHit raycastHit = hits[j];
                bool isOrder = raycastHit.distance < min;
                if (!increasing)
                    isOrder = raycastHit.distance > min;
                if (isOrder && !sortedHits.Contains(hits[j]))
                {
                    min = raycastHit.distance;
                    candidate = raycastHit;
                }
            }

            sortedHits.Add(candidate);
        }
        return sortedHits;
    }

    internal static List<Polygon> GetRegions(Polygon viewPortPoints, bool isVisible, out bool knotTouchesSide)
    {
        var cg = new CrossingGraph();
        if (isVisible)
        {
            // PortalTextureSetup.stw("Intersections");
            cg.GetIntersections(viewPortPoints);
            // PortalTextureSetup.stw("Intersections");
            // PortalTextureSetup.stw("CutUpEdges");
            Debugger.Log("Number of non cut up edges is " + cg.edges.Count);
            foreach (var edge in cg.edges)
            {
                Debugger.Log("Edge " + edge + " from " + edge.start + " to " + edge.end);
            }
            cg.CutUpEdges();
            Debugger.Log("Number of cut up edges is " + cg.edges.Count);
            // Debugger.Log("Number of cut up edges is " + cg.edges.Count);
            // PortalTextureSetup.stw("CutUpEdges");
            // PortalTextureSetup.stw("SetRegions");
            foreach (var edge in cg.edges)
            {
                Debugger.Log("Edge " + edge + " from " + edge.start + " to " + edge.end);
            }
            foreach (var crossing in cg.crossings)
            {
                Debugger.Log("Crossing " + crossing);
            }
            try
            {
                knotTouchesSide = cg.SetRegions();
            }
            catch (YourCustomException)
            {
                knotTouchesSide = false;
                return null;
            }
            // Debugger.Log("Number regions is " + cg.regions.Count);
            // PortalTextureSetup.stw("SetRegions");
        }
        else
        {
            knotTouchesSide = false;
        }
        if (cg.regions == null || cg.regions.Count == 0)
        {
            cg.SetScreenRegion();
        }
        return cg.regions;
    }
}