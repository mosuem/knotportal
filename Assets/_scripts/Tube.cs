using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class Tube
{
    Mesh m;
    List<Vector3> line;
    const int nSegments = 10;
    const float radius = 0.1f;
    Mesh mesh;
    void generateTube()
    {
        var vertices = new List<Vector3>();
        for (int i = 0; i < line.Count - 1; i++)
        {
            var point = line[i];
            var direction = line[i + 1] - line[i];

            var coordinates = new Vector2[nSegments];
            for (int j = 0; j < nSegments; j++)
            {
                float t = 2f * Mathf.PI * radius * (float)j / nSegments;
                var coordinate = new Vector2(Mathf.Cos(t), Mathf.Sin(t));
                coordinates[j] = coordinate;
            }

        }
    }
}


