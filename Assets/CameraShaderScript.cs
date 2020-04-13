using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class CameraShaderScript : MonoBehaviour
{
    private PortalTextureSetup portalTextureSetup;
    public Shader EffectShader;
    private Material ppMaterial;
    private Knot knot;
    private List<Vector4> longRegionList;
    private List<Vector4> startEndList;
    private List<Vector4> boundingBoxes;
    private List<Vector4> generatorsVec;

    void Awake()
    {
        ppMaterial = new Material(EffectShader);

        List<Vector4> fillArray = new List<Vector4>(1000);
        for (int i = 0; i < 1000; i++)
        {
            fillArray.Add(Vector4.zero);
        }
        ppMaterial.SetVectorArray("_generators", fillArray);
        ppMaterial.SetVectorArray("_regions", fillArray);
        ppMaterial.SetVectorArray("_boundingBoxes", fillArray);
        ppMaterial.SetVectorArray("_startsEnds", fillArray);
        ppMaterial.SetVectorArray("_spheres", fillArray);
    }
    void Start()
    {
        portalTextureSetup = GameObject.Find("GameManager").GetComponent<PortalTextureSetup>();
        var dualCameras = PortalTextureSetup.dualCameras;
        var worlds = portalTextureSetup.worlds;

        for (int i = 0; i < dualCameras.Length; i++)
        {
            Camera camera = dualCameras[i].GetTextureCamera();
            RenderTextureDescriptor desc;
            if (XRSettings.enabled)
            {
                desc = XRSettings.eyeTextureDesc;
                desc.width = desc.width / 2;
            }
            else
            {
                desc = new RenderTextureDescriptor(Screen.width, Screen.height); // Not XR
            }
            camera.targetTexture = new RenderTexture(desc);
            var renderTex = camera.targetTexture;
            string texName = "_CameraTex" + i;
            ppMaterial.SetTexture(texName, renderTex);
        }

        List<Vector4> fillUpArray = new List<Vector4>();
        for (int i = 0; i < 1000; i++)
        {
            fillUpArray.Add(Vector4.zero);
        }
        ppMaterial.SetVectorArray("_generators", fillUpArray);
        ppMaterial.SetVectorArray("_boundingBoxes", fillUpArray);
        ppMaterial.SetVectorArray("_regions", fillUpArray);
        ppMaterial.SetVectorArray("_startsEnds", fillUpArray);
        ppMaterial.SetVectorArray("_spheres", fillUpArray);

        longRegionList = new List<Vector4>();
        startEndList = new List<Vector4>();
        boundingBoxes = new List<Vector4>();
    }

    public void SetCameraShaders(List<int> generators, List<Polygon> regions, List<Vector4> spheres, int defaultGenerator)
    {
        {
            longRegionList.Clear();
            startEndList.Clear();
            boundingBoxes.Clear();
            for (int i = 0; i < regions.Count; i++)
            {
                List<Vector4> region = GetBoundingBox(regions[i], out Vector4 boundingBox);
                boundingBoxes.Add(boundingBox);
                startEndList.Add(new Vector2(longRegionList.Count, longRegionList.Count + region.Count));
                longRegionList.AddRange(region);
            }
        }
        {
            generatorsVec = new List<Vector4>();
            foreach (var gen in generators)
            {
                generatorsVec.Add(new Vector2(gen, 0));
            }
        }
        if (regions.Count > 0)
        {
            ppMaterial.SetVectorArray("_generators", generatorsVec);
            ppMaterial.SetVectorArray("_boundingBoxes", boundingBoxes);
            ppMaterial.SetVectorArray("_regions", longRegionList);
            ppMaterial.SetVectorArray("_startsEnds", startEndList);
            for (int i = 0; i < spheres.Count; i++)
            {
                ppMaterial.SetVector("_spheres" + i, spheres[i]);
            }
        }
        ppMaterial.SetInt("_startsEndsCount", startEndList.Count);
        ppMaterial.SetInt("_defaultGenerator", defaultGenerator);
    }

    public static List<Vector4> GetBoundingBox(Polygon polygon, out Vector4 boundingBox, bool setList = true)
    {
        var list = new List<Vector4>();
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach (var p in polygon)
        {
            if (p.x < minX)
            {
                minX = p.x;
            }
            if (p.x > maxX)
            {
                maxX = p.x;
            }
            if (p.y < minY)
            {
                minY = p.y;
            }
            if (p.y > maxY)
            {
                maxY = p.y;
            }
            if (setList)
            {
                list.Add(p);
            }
        }
        boundingBox = new Vector4(minX, maxX, minY, maxY);
        return list;
    }
    // bool firstPass = true;
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, ppMaterial);
    }
}