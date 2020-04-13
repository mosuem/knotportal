using System;
using System.Collections;
using System.Collections.Generic;
using Curve;
using UnityEngine;
using UnityEngine.XR;
using UnityStandardAssets.Water;
using Debug = UnityEngine.Debug;

public class PortalTextureSetup : MonoBehaviour
{

    public GameObject[] worldPrefabs;
    public Camera cameraTemplate;
    public Material cameraMat;
    public int numWorlds;
    public int numPortals;

    public Camera playerCamera;

    public Material[] mats;
    public static Vector3 offset = new Vector3(0, 1, 0) * 300f;
    public GameObject[] worlds;
    public List<Knot> knots;

    public static DualCamera[] dualCameras;

    private static Color color = new Color(255 / 255f, 229 / 255f, 132 / 255f, 1);
    private static Color color1 = new Color(184 / 255f, 219 / 255f, 160 / 255f, 1);
    private static Color color2 = new Color(215 / 255f, 157 / 255f, 199 / 255f, 1);
    private static Color color3 = new Color(59 / 255f, 60 / 255f, 102 / 255f, 1);
    private static Color white = Color.white;
    public static List<Color> colors = new List<Color>() { white, color1, color, color2, white, color3
    ,Color.grey,Color.blue, Color.black, Color.cyan };
    public static int actWorld = 0;
    public static KnotType knotType = KnotType.Unknot;
    public static int facingPortal;
    public static int oldFacingPortal;
    int layerWorlds = 14;

    public static GameObject meshPlane;

    public static bool isFlipped = false;
    // Use this for initialization
    void Awake()
    {
        playerCamera.transform.parent.position += new Vector3(0, 0, 7);
        playerCamera.transform.parent.position += playerPosition;
        playerCamera.transform.parent.Rotate(playerRotation);
        if (useAltWorlds)
        {
            worldPrefabs = altWorldPrefabs;
        }
        playerCamera.transform.parent.gameObject.GetComponent<PlayerMover>().enabled = !XRSettings.isDeviceActive;

        meshPlane = GameObject.Find("MeshPlane");
        TeleportedTime = Time.realtimeSinceStartup;

        numWorlds = KnotInfos.getNumberWorlds();
        numPortals = KnotInfos.getNumberPortals();

        knots = new List<Knot>();

        mats = new Material[numWorlds];
        for (int world = 0; world < numWorlds; world++)
        {
            mats[world] = Instantiate(cameraMat);
        }

        worlds = new GameObject[numWorlds];
        if (knotType == KnotType.Unknot || knotType == KnotType.Unknot3)
        {
            var worldTemp = worldPrefabs[1];
            worldPrefabs[1] = worldPrefabs[5];
            worldPrefabs[5] = worldTemp;
            if (!useAltWorlds)
            {
                Transform transform1 = worldPrefabs[1].transform.Find("WaterProDaytime");
                Water water = transform1.gameObject.GetComponent<Water>();
                for (int i = 0; i < numWorlds; i++)
                {
                    if (i != 1)
                    {
                        water.reflectLayers ^= 1 << layerWorlds + i;
                        water.refractLayers ^= 1 << layerWorlds + i;
                    }
                }
            }
        }
        else
        {
            if (!useAltWorlds)
            {
                Transform transform1 = worldPrefabs[5].transform.Find("WaterProDaytime");
                Water water = transform1.gameObject.GetComponent<Water>();
                for (int i = 0; i < numWorlds; i++)
                {
                    if (i != 5)
                    {
                        water.reflectLayers ^= 1 << layerWorlds + i;
                        water.refractLayers ^= 1 << layerWorlds + i;
                    }
                }
            }
        }
        for (int i = 0; i < numWorlds; i++)
        {
            worlds[i] = Instantiate(worldPrefabs[i], worldPrefabs[i].transform.position + getOffset(i), worldPrefabs[i].transform.rotation);
            worlds[i].name = "World " + i + " called " + KnotInfos.getGenerator(i);
            Debugger.Log("Create world " + i + " of " + numWorlds);
            foreach (var renderer in worlds[i].transform.Find("GroundPlate").GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.gameObject.CompareTag("Bounding"))
                {
                    // renderer.material.shader = Shader.Find("_Color");
                    renderer.material.SetColor("_Color", colors[i]);
                    // renderer.material.shader = Shader.Find("Specular");
                    // renderer.material.SetColor("_SpecColor", value);
                }
            }
        }

        dualCameras = new DualCamera[numWorlds];
        for (int world = 0; world < numWorlds; world++)
        {
            dualCameras[world] = new DualCamera("Camera " + world, cameraTemplate);
            var portalcam = dualCameras[world].gameObject.AddComponent<PortalCamera>();
            portalcam.playerCamera = playerCamera.transform;
            portalcam.world = world;
        }
        var light = GameObject.Find("Point Light");
        if (buildUp)
        {
            var flare = light.GetComponent<LensFlare>();
            flare.enabled = false;
        }
        else
        {
            Destroy(light);
        }
        for (int i = 0; i < worlds.Length; i++)
        {
            GameObject world = worlds[i];
            SetLayerRecursively(world, layerWorlds + i);
        }
        for (int i = 0; i < dualCameras.Length; i++)
        {
            DualCamera camera = dualCameras[i];
            for (int j = 0; j < numWorlds; j++)
            {
                if (j != i)
                    camera.SetCulling(layerWorlds + j);
            }
        }
    }
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    private void AddDescendantsWithTag(Transform parent, List<GameObject> list, string tag = "")
    {
        foreach (Transform child in parent)
        {
            if (tag == "" || child.gameObject.tag == tag)
            {
                list.Add(child.gameObject);
            }
            AddDescendantsWithTag(child, list, tag);
        }
    }

    public static Vector3 getOffset(int world)
    {
        return world * offset;
    }

    void Start()
    {
        Cursor.visible = false;
        facingPortal = -2;
        Debug.Log("Number worlds " + numWorlds);
        Debug.Log("Number portals " + numPortals);
        for (int world = 0; world < numWorlds; world++)
        {
            Debug.Log("Build Knot " + world + " / " + numWorlds);
            Debug.Log(KnotInfos.getNumberWorlds());
            Debug.Log(knotType);
            var knot = Knot.BuildKnot(GetKnotPosition(world), KnotMaterial, buildUp);
            knot.knotObject.name = "Knot " + world;
            knot.knotObject.transform.parent = worlds[world].transform;
            knots.Add(knot);
        }
        actWorld = 0;
        if (buildUp)
        {
            StartCoroutine(BuildUpKnot());
        }
        else
        {

            doCones();
            foreach (var knot in knots)
            {
                knot.knotObject.GetComponent<MeshRenderer>().enabled = true;
            }
            knotBuilt = true;
        }
    }

    private void doCones()
    {
        for (int world = 0; world < numWorlds; world++)
        {
            Cone.doCone(knots[world], world);
        }
    }

    IEnumerator BuildUpKnot()
    {
        var controls = new List<Vector3>();
        var lineObject = new GameObject();
        var lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = KnotMaterial;
        lineRenderer.material.SetColor("_Color", Color.red);
        lineRenderer.widthMultiplier = 0.01f;

        var knotObject = new GameObject("Knot");
        Vector3 position = GetKnotPosition(0);
        knotObject.transform.position = position;
        var mf = knotObject.AddComponent<MeshFilter>();
        var rendererKnot = knotObject.AddComponent<MeshRenderer>();
        rendererKnot.material = KnotMaterial;

        List<Vector3> points = new List<Vector3>();
        Vector3 startPos = GetStartPosition(0);

        const float V = 2;
        for (int i = 0; i < knots[0].points.Count; i++)
        {
            for (float j = 0; j < V; j++)
            {
                int next = i + 1 < knots[0].points.Count ? i + 1 : 0;
                var point = Vector3.Lerp(knots[0].points[i], knots[0].points[next], j % V / V);
                points.Add(point - position);
                lineRenderer.SetPositions(new Vector3[] { startPos, point });
                if (j > 0)
                {

                    var curve = new CatmullRomCurve(points);

                    // Build tubular mesh with Curve
                    int tubularSegments = points.Count;
                    float radius = 0.02f;
                    int radialSegments = 16;
                    var mesh = Tubular.Tubular.Build(curve, tubularSegments, radius, radialSegments, false);

                    // visualize mesh
                    mf.sharedMesh = mesh;
                }
                yield return null;
            }
        }
        var light = GameObject.Find("Point Light");
        light.transform.position = GetKnotPosition(0);
        var flare = light.GetComponent<LensFlare>();
        flare.enabled = true;
        for (int i = 0; i < 100; i++)
        {
            flare.brightness += 0.1f;
            yield return null;
        }
        doCones();
        flare.enabled = false;
        Destroy(knotObject);
        Destroy(lineObject);
        Destroy(light);
        foreach (var knot in knots)
        {
            knot.knotObject.GetComponent<MeshRenderer>().enabled = true;
        }
        knotBuilt = true;
    }

    public static Vector3 GetKnotPosition(int world)
    {
        return getOffset(world) + Vector3.up * 2.1f + Vector3.forward * 8f;
    }

    // private static Vector2 GetCentroid(Polygon vertices)
    // {
    //     Vector2 centroid = Vector2.zero;
    //     var signedArea = 0.0f;
    //     var x0 = 0.0f; // Current vertex X
    //     var y0 = 0.0f; // Current vertex Y
    //     var x1 = 0.0f; // Next vertex X
    //     var y1 = 0.0f; // Next vertex Y
    //     var a = 0.0f;  // Partial signed area

    //     // For all vertices except last
    //     int i = 0;
    //     for (i = 0; i < vertices.Count - 1; ++i)
    //     {
    //         x0 = vertices[i].x;
    //         y0 = vertices[i].y;
    //         x1 = vertices[i + 1].x;
    //         y1 = vertices[i + 1].y;
    //         a = x0 * y1 - x1 * y0;
    //         signedArea += a;
    //         centroid.x += (x0 + x1) * a;
    //         centroid.y += (y0 + y1) * a;
    //     }

    //     // Do last vertex separately to avoid performing an expensive
    //     // modulus operation in each iteration.
    //     x0 = vertices[i].x;
    //     y0 = vertices[i].y;
    //     x1 = vertices[0].x;
    //     y1 = vertices[0].y;
    //     a = x0 * y1 - x1 * y0;
    //     signedArea += a;
    //     centroid.x += (x0 + x1) * a;
    //     centroid.y += (y0 + y1) * a;

    //     signedArea *= 0.5f;
    //     centroid.x /= (6.0f * signedArea);
    //     centroid.y /= (6.0f * signedArea);

    //     return centroid;
    // }

    static Vector3 startOffset = new Vector3(0, 0, 2);
    public static Vector3 GetStartPosition(int world)
    {
        return startOffset + GetKnotPosition(world);
    }

    private float LinePlaneIntersect(Vector3 rayPoint, Vector3 rayVector, Vector3 planePoint, Vector3 planeNormal)
    {
        var v1 = rayPoint - planePoint;
        var d1 = Vector3.Dot(v1, planeNormal);
        var d2 = Vector3.Dot(rayVector, planeNormal);
        var t = d1 / d2;
        return t;
    }

    [System.Diagnostics.Conditional("MYDEBUG")]
    public static void DebugSphere(Vector3 intersection, Color black, string v = "Debug", bool isDebug = true)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(s.GetComponent<SphereCollider>());
        if (isDebug)
        {
            s.tag = "Debug";
        }
        s.transform.position = intersection;
        s.GetComponent<MeshRenderer>().material.color = black;
        s.transform.localScale *= 0.1f;
        s.name = v;
        // return s;
    }

    void Update()
    {
        Recenter();
        for (int i = 1; i < Mathf.Min(KnotInfos.getNumberWorlds() + 1, 10); i++)
        {
            if (Input.GetKeyUp(i + ""))
            {
                Teleport(i - 1);
                actWorld = i - 1;
                break;
            }
        }
    }
    private static Vector3 playerRotation = Vector3.zero;
    private static Vector3 playerPosition = Vector3.zero;
    private void Recenter()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            playerCamera.transform.parent.Rotate(new Vector3(0f, -1f, 0f));
            playerRotation += new Vector3(0f, -1f, 0f);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            playerCamera.transform.parent.Rotate(new Vector3(0f, 1f, 0f));
            playerRotation += new Vector3(0f, 1f, 0f);
        }
        else if (Input.GetKey(KeyCode.W))
        {
            Vector3 vector3 = new Vector3(0f, 0f, 0.1f);
            playerCamera.transform.parent.position += vector3;
            playerPosition += vector3;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Vector3 vector3 = new Vector3(0f, 0f, -0.1f);
            playerCamera.transform.parent.position += vector3;
            playerPosition += vector3;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Vector3 vector3 = new Vector3(-0.1f, 0f, 0f);
            playerCamera.transform.parent.position += vector3;
            playerPosition += vector3;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Vector3 vector3 = new Vector3(0.1f, 0f, 0f);
            playerCamera.transform.parent.position += vector3;
            playerPosition += vector3;
        }
    }

    private int CheckForTeleport(Vector3 oldPosition, string generator)
    {
        var traveled = playerCamera.transform.position - oldPosition;
        var sortedHits = AreaManager.RaycastAllGenerators(oldPosition, traveled, traveled.magnitude);
        // var hits = Physics.RaycastAll(oldPosition, traveled, traveled.magnitude);
        // var sortedHits = AreaManager.SortHits(hits);
        string newGen = generator;
        foreach (var hit in sortedHits)
        {
            if (hit.collider.CompareTag("Generator"))
            {
                string g2 = hit.collider.gameObject.name;
                newGen = KnotInfos.multiply(newGen, g2);
            }
        }
        return KnotInfos.getIndex(newGen);
    }

    private void Teleport(int v)
    {
        playerCamera.transform.parent.position -= getOffset(actWorld);
        playerCamera.transform.parent.position += getOffset(v);
    }

    Vector3 oldPosition = Vector3.zero;
    float teleportTime = 0f;
    public static bool teleport = true;
    void LateUpdate()
    {
        var debugObjects = GameObject.FindGameObjectsWithTag("Debug");
        foreach (var debugObject in debugObjects)
        {
            Destroy(debugObject);
        }
        if (teleport)
        {
            var newWorld = CheckForTeleport(oldPosition, KnotInfos.getGenerator(actWorld));
            if (newWorld != actWorld)// && Time.time - teleportTime > 0.01f
            {
                teleportTime = Time.time;
                Debug.Log("Teleport to " + newWorld + " from " + actWorld);
                Teleport(newWorld);
                actWorld = newWorld;
            }
        }
        oldPosition = playerCamera.transform.position;
        for (int i = 0; i < dualCameras.Length; i++)
        {
            DualCamera player = dualCameras[i];
            player.ResetTransform(playerCamera.transform, i);
        }
        // Debugger.Log("Count: " + Debugger.Count1 + " vs. " + Debugger.Count2);
        if (knotBuilt)
        {
            BuildPortals();
        }
    }

    // private static Dictionary<string, Stopwatch> sts = new Dictionary<string, Stopwatch>();

    // public static void stw(string s)
    // {
    //     if (Debugger.TimeMode)
    //     {
    //         if (sts.ContainsKey(s))
    //         {
    //             var st = sts[s];
    //             Debug.Log("Time for " + s + ": " + st.ElapsedTicks);
    //             st.Stop();
    //             sts.Remove(s);
    //         }
    //         else
    //         {
    //             Stopwatch stopwatch = new Stopwatch();
    //             stopwatch.Start();
    //             sts[s] = stopwatch;
    //         }
    //     }
    // }

    void BuildPortals()
    {
        Debugger.Log("---------------NEW CYCLE---------------------");
        // stw("Total");
        // stw("Start");
        Knot knot = knots[actWorld];
        int numberPoints = knot.points.Count;
        var viewPortPoints = dualCameras[actWorld].GetViewPortPoints(knot.points);
        // stw("Start");
        // stw("GetRegions");
        bool knotTouchesSide;
        var regions = AreaManager.GetRegions(viewPortPoints, isVisible(knot.knotObject), out knotTouchesSide);
        // stw("GetRegions");
        // stw("GetPointsInRegions");
        List<Vector2> midPoints = null;
        List<float> precisions = null;
        if (regions != null)
        {
            precisions = GetPrecisions(regions);
            midPoints = PolyLabel.GetPointsInRegions(regions, precisions);
        }
        if (regions != null && midPoints != null)
        {
            Debugger.Log("New Regions");
            oldKnotTouchesSide = knotTouchesSide;
            oldRegions = regions;
            oldMidPoints = midPoints;
        }
        else
        {
            Debugger.Log("Old Regions");
            knotTouchesSide = oldKnotTouchesSide;
            regions = oldRegions;
            midPoints = oldMidPoints;
            precisions = GetPrecisions(oldRegions);
        }
        // stw("GetPointsInRegions");
        // stw("Rest");
        var spheres = ComputeSpheres(regions, midPoints);
        for (int i = 0; i < regions.Count; i++)
        {
            var point = midPoints[i];
            DebugSphere(dualCameras[actWorld].ViewportToWorldPoint(new Vector3(point.x, point.y, 1f)), Color.blue, "Midpoint " + i + " " + regions[i].isScreenEdge);
        }
        var rayStart = dualCameras[actWorld].gameObject.transform.position;
        List<string> areaGenerators = AreaManager.AssignGenerators(dualCameras[actWorld], midPoints, rayStart, knotTouchesSide, precisions);
        var defaultGenerator = KnotInfos.getIndex(KnotInfos.getGenerator(actWorld));
        if (!knotTouchesSide)
        {
            int last = areaGenerators.Count - 1;
            defaultGenerator = KnotInfos.getIndex(areaGenerators[last]);
            areaGenerators.RemoveAt(last);
        }
        // Debug.Log("Default Gen: " + defaultGenerator);
        // Debugger.Log ("Rest: " + st.ElapsedMilliseconds);
        // st.Restart ();
        var generatorNumbers = new List<int>();
        for (int i = 0; i < areaGenerators.Count; i++)
        {
            var gen = areaGenerators[i];
            generatorNumbers.Add(KnotInfos.getIndex(gen));
            Debugger.Log("Generator for Region " + i + ": " + gen);
        }
        Debugger.Log("Defaultgen is " + defaultGenerator);
        // Debugger.Log (areaGenerators[0]);

        // stw("Rest");
        // stw("SetCameraShaders");
        CameraShaderScript cameraShaderScript = playerCamera.GetComponent<CameraShaderScript>();
        // Debugger.Log("Set Camera Shader with " + regions.Count + " and " + generatorNumbers.Count);

        cameraShaderScript.SetCameraShaders(generatorNumbers, regions, spheres, defaultGenerator);

        // stw("SetCameraShaders");
        // stw("Total");
        Debugger.Log("---------------END CYCLE---------------------");
    }

    private List<float> GetPrecisions(List<Polygon> regions)
    {
        var precisions = new List<float>();
        foreach (var region in regions)
        {
            Vector4 boundingBox;
            CameraShaderScript.GetBoundingBox(region, out boundingBox, false);
            var size1 = boundingBox.y - boundingBox.x;
            var size2 = boundingBox.w - boundingBox.z;
            float precision = Mathf.Min(size1, size2) / 2f;
            precision = Mathf.Clamp(precision, 0.001f, 0.5f);
            precisions.Add(precision);
        }
        return precisions;
    }

    private List<Vector4> ComputeSpheres(List<Polygon> regions, List<Vector2> midPoints)
    {
        var spheres = new List<Vector4>();
        for (int i = 0; i < regions.Count; i++)
        {
            var region = regions[i];
            var midPoint = midPoints[i];
            var minDist = float.MaxValue;
            foreach (var point in region)
            {
                float v = Vector2.Distance(point, midPoint);
                if (v < minDist)
                {
                    minDist = v;
                }
            }
            spheres.Add(new Vector4(midPoint.x, midPoint.y, minDist, 0f));
        }
        return spheres;
    }
    private bool isVisible(GameObject Object)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(dualCameras[actWorld].GetPositionCamera());
        if (GeometryUtility.TestPlanesAABB(planes, Object.GetComponent<MeshRenderer>().bounds))
            return true;
        else
            return false;
    }
    public Material KnotMaterial;
    internal static float TeleportedTime;
    public static bool useAltWorlds = true;
    public GameObject[] altWorldPrefabs;
    private bool oldKnotTouchesSide = false;
    private List<Polygon> oldRegions = new List<Polygon>();
    private List<Vector2> oldMidPoints = new List<Vector2>();
    private bool knotBuilt = false;
    private bool buildUp = false;

}