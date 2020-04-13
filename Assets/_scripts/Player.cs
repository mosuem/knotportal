using System;
using System.Collections.Generic;
using UnityEngine;

public class DualCamera
{
    private Camera textureCam;
    private Camera positionCam;
    internal GameObject gameObject;
    public DualCamera(string name, Camera template)
    {
        gameObject = new GameObject(name);
        SetCameras(template);
        // gameObject.transform.position = vector3;
        // gameObject.transform.rotation = rotation;
    }
    internal void SetCameras(Camera c)
    {
        textureCam = Camera.Instantiate(c, Vector3.zero, Quaternion.identity);
        textureCam.transform.SetParent(gameObject.transform);
        textureCam.name = "Texture Cam";
        textureCam.targetDisplay = 1;
        positionCam = Camera.Instantiate(c, Vector3.zero, Quaternion.identity);
        positionCam.transform.SetParent(gameObject.transform);
        positionCam.name = "Position Cam";
        positionCam.targetDisplay = 2;
    }
    internal Polygon GetViewPortPoints(List<Vector3> points)
    {
        var viewPortPoints = new Polygon(points.Count);
        foreach (var point in points)
        {
            var viewPortPoint = positionCam.WorldToViewportPoint(point, Camera.MonoOrStereoscopicEye.Mono);
            viewPortPoints.Add(viewPortPoint);
        }
        return viewPortPoints;
    }

    internal Camera GetTextureCamera()
    {
        return textureCam;
    }

    internal Vector3 ViewportToWorldPoint(Vector3 vector3)
    {
        return positionCam.ViewportToWorldPoint(vector3);
    }

    internal Camera GetPositionCamera()
    {
        return positionCam;
    }

    internal void ResetTransform(Transform player, int i)
    {
        textureCam.transform.localPosition = Vector3.zero;
        textureCam.transform.localRotation = Quaternion.identity;
        positionCam.transform.localPosition = Vector3.zero;
        positionCam.transform.localRotation = Quaternion.identity;

        gameObject.transform.position = player.position + PortalTextureSetup.getOffset(i) - PortalTextureSetup.getOffset(PortalTextureSetup.actWorld);
        gameObject.transform.rotation = player.rotation;
    }

    internal void SetCulling(int v)
    {
        textureCam.cullingMask ^= 1 << v;
    }
}