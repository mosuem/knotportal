using System;
using System.Collections;
using System.Collections.Generic;
using Curve;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupKnot : MonoBehaviour
{

    void Update()
    {
        var numberOfKnots = System.Enum.GetValues(typeof(KnotType)).Length;
        if (Input.GetKeyDown(KeyCode.O))
        {
            PortalTextureSetup.useAltWorlds = !PortalTextureSetup.useAltWorlds;
            ChangeKnot();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PortalTextureSetup.teleport = !PortalTextureSetup.teleport;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if ((int)PortalTextureSetup.knotType < numberOfKnots - 1)
            {
                PortalTextureSetup.knotType++;
            }
            else
            {
                PortalTextureSetup.knotType = 0;
            }
            ChangeKnot();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if ((int)PortalTextureSetup.knotType > 0)
            {
                PortalTextureSetup.knotType--;
            }
            else
            {
                PortalTextureSetup.knotType = (KnotType)numberOfKnots - 1;
            }
            ChangeKnot();
        }
    }

    private static void ChangeKnot()
    {
        Debug.Log((int)PortalTextureSetup.knotType);
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}