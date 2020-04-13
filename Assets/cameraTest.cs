using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraTest : MonoBehaviour {
    // Start is called before the first frame update
    public Camera camera1;
    public Camera camera2;
    public Shader EffectShader1;

    void Start () {
        var cube1 = GameObject.Find ("Cube");
        var cube2 = GameObject.Find ("Cube (1)");
        var cube3 = GameObject.Find ("Cube (2)");
        cube3.GetComponent<Renderer> ().material.SetOverrideTag ("RenderType1", "RenderType1");

        camera1.enabled = true;
        // camera1.SetReplacementShader (EffectShader1, "RenderType1");

        // camera2.enabled = true;
        // camera2.SetReplacementShader (EffectShader1, "RenderType");

        var renderTex = camera2.targetTexture;
        Shader.SetGlobalTexture ("_OtherTex", renderTex);

        points = new List<Vector4> (1000);
        Shader.SetGlobalVectorArray ("_regions", points);
        points.Clear ();
        points.Add (new Vector2 (0.4f, 0.4f));
        points.Add (new Vector2 (0.6f, 0.4f));
        points.Add (new Vector2 (0.6f, 0.6f));
        points.Add (new Vector2 (0.5f, 0.7f));
        points.Add (new Vector2 (0.4f, 0.6f));
        Shader.SetGlobalInt ("_regionCount", points.Count);
        Shader.SetGlobalVectorArray ("_regions", points);
        // cube1.GetComponent<Renderer> ().material.SetVectorArray ("_regions", points);
    }

    // Update is called once per frame
    float sign = 1f;
    void Update () {
        for (int i = 0; i < points.Count; i++) {
            points[i] += sign * 0.01f * Vector4.one;
        }
        if (points[3].y > 1f) {
            sign *= -1f;
        }
        if (points[0].x < 0f) {
            sign *= -1f;
        }

        Shader.SetGlobalVectorArray ("_regions", points);
    }

    private Material material;
    void Awake () {
        material = new Material (shader);
    }

    public Shader shader;
    private List<Vector4> points;

    void OnRenderImage (RenderTexture source, RenderTexture destination) {
        Graphics.Blit (source, destination, material);
    }
}