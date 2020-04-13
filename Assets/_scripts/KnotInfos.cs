using System;
using System.Collections.Generic;
using UnityEngine;

internal class KnotInfos
{

    private static readonly Dictionary<KnotType, int> numWorlds
    = new Dictionary<KnotType, int>
    {
        { KnotType.Unknot, 2},
        { KnotType.Unknot3, 3},
        { KnotType.Twisted, 2},
        { KnotType.Trefoil, 6},
        { KnotType.Figureeight, 10},
    };

    private static readonly Dictionary<KnotType, int> numPortals
    = new Dictionary<KnotType, int>
    {
        { KnotType.Unknot, 1},
        { KnotType.Unknot3, 1},
        { KnotType.Twisted, 2},
        { KnotType.Trefoil, 4},
        { KnotType.Figureeight, 5},
    };

    private static readonly Dictionary<KnotType, int[]> coneComponents
    = new Dictionary<KnotType, int[]>
    {
        { KnotType.Unknot, new int[] { 1 }},
        { KnotType.Unknot3, new int[] { 1 }},
        { KnotType.Twisted, new int[] { 1, 1 }},
        { KnotType.Trefoil, new int[] { 1, 2, 3 }},
        { KnotType.Figureeight, new int[] { 1,2,3,4,5,6,7,8 }},
    };

    private static readonly string[][] a1cayley = new string[][] {
        new string[] { "e", "a" },
        new string[] { "a", "e" }
    };
    private static readonly string[][] dihedral6cayley = new string[][] {
        new string[] { "e", "a", "b", "c", "d", "f", },
        new string[] { "a", "e", "d", "f", "b", "c" },
        new string[] { "b", "f", "e", "d", "c", "a" },
        new string[] { "c", "d", "f", "e", "a", "b" },
        new string[] { "d", "c", "a", "b", "f", "e" },
        new string[] { "f", "b", "c", "a", "e", "d" }
    };
    private static readonly string[][] dihedral10cayley = new string[][] {
        new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" },
        new string[] { "b", "a", "d", "c", "f", "e", "h", "g", "j", "i" },
        new string[] { "c", "j", "e", "b", "g", "d", "i", "f", "a", "h" },
        new string[] { "d", "i", "f", "a", "h", "c", "j", "e", "b", "g" },
        new string[] { "e", "h", "g", "j", "i", "b", "a", "d", "c", "f" },
        new string[] { "f", "g", "h", "i", "j", "a", "b", "c", "d", "e" },
        new string[] { "g", "f", "i", "h", "a", "j", "c", "b", "e", "d" },
        new string[] { "h", "e", "j", "g", "b", "i", "d", "a", "f", "c" },
        new string[] { "i", "d", "a", "f", "c", "h", "e", "j", "g", "b" },
        new string[] { "j", "c", "b", "e", "d", "g", "f", "i", "h", "a" }
    };


    private static readonly string[][] a2cayley = new string[][] {
        new string[] { "e", "a", "b" },
        new string[] { "a", "b", "e" },
        new string[] { "b", "e", "a" }
    };
    private static readonly Dictionary<KnotType, string[][]> matrices
    = new Dictionary<KnotType, string[][]>
    {
        { KnotType.Unknot, a1cayley},
        { KnotType.Unknot3, a2cayley},
        { KnotType.Twisted, a1cayley},
        { KnotType.Trefoil, dihedral6cayley},
        { KnotType.Figureeight, dihedral10cayley},
    };



    internal static int getNumberWorlds()
    {
        var k = PortalTextureSetup.knotType;
        return numWorlds[k];
    }

    internal static string getConeComponent(int k)
    {
        return getGenerator(coneComponents[PortalTextureSetup.knotType][k]);
    }
    internal static int getNumberPortals()
    {
        var k = PortalTextureSetup.knotType;
        return numPortals[k];
    }

    internal static string multiply(string g1, string g2)
    {
        int a = Array.IndexOf(getElements(), g1);
        int b = Array.IndexOf(getElements(), g2);
        string[][] v = getMatrix(PortalTextureSetup.knotType);
        if (a < 0 || b < 0)
            Debug.Log("Cannot multiply " + g1 + " and " + g2);
        return v[a][b];
    }

    internal static int getIndex(string g)
    {
        return Array.IndexOf(getElements(), g);
    }

    internal static string getGenerator(int world)
    {
        return getElements()[world];
    }

    private static string[][] getMatrix(KnotType k)
    {
        return matrices[k];
    }

    private static string[] getElements()
    {
        string[][] v = getMatrix(PortalTextureSetup.knotType);
        return v[0];
    }

    internal static string GetInverse(string g)
    {
        foreach (var h in getElements())
        {
            if (multiply(g, h).Equals("e"))
            {
                return h;
            }
        }
        return "";
    }
}