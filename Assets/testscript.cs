using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : MonoBehaviour {
    public GameObject c1;
    public GameObject c2;
    // Start is called before the first frame update
    void Start () {

    }

    // Update is called once per frame
    void Update () {
        Debug.Log (c1.transform.position.ToString ("F4"));
        Debug.Log (c2.transform.position.ToString ("F4"));
        Debug.Log (Angle2 (c1.transform.position, c2.transform.position));
        Debug.Log (Angle3 (c1.transform.position, c2.transform.position));
        Debug.Log (Angle4 (c1.transform.position, c2.transform.position));
        Debug.Log ("C: " + Cross (c1.transform.position, c2.transform.position));

    }

    private float Cross (Vector2 position1, Vector2 position2) {
        return position1.x * position2.y - position1.y * position2.x;
    }

    private float Angle2 (Vector2 v1, Vector2 v2) {
        var n1 = v1.magnitude;
        var n2 = v2.magnitude;
        float angle = Mathf.Rad2Deg * 2 * Mathf.Atan2 ((v1 * n2 - n1 * v2).magnitude, (v1 * n2 + n1 * v2).magnitude);
        if (v1.x * v2.y - v1.y * v2.x < 0) {
            angle = 360f - angle;
        }
        return angle;
        // return Mathf.Rad2Deg * 2 * Mathf.Atan2 ((v1 / n1 + v2 / n2).magnitude, (v1 / n1 - v2 / n2).magnitude);
    }

    private float Angle4 (Vector2 v1, Vector2 v2) {
        var a = v1.magnitude;
        var b = v2.magnitude;
        var c = (v1 - v2).magnitude;
        if (b < a) {
            var t = a;
            a = b;
            b = t;
            var t2 = v1;
            v1 = v2;
            v2 = t2;
        }
        var mu = -1f;
        if (b >= c && c >= 0) {
            mu = c - (a - b);
        } else if (c > b && b >= 0) {
            mu = b - (a - c);
        }
        var top = ((a - b) + c) * mu;
        var bottom = (a + (b + c)) * ((a - c) + b);
        float angle = Mathf.Rad2Deg * 2 * Mathf.Atan (Mathf.Sqrt (top / bottom));
        if (v1.x * v2.y - v1.y * v2.x < 0) {
            angle = angle * -1f + 180f;
        }
        return angle;
    }
    float Angle3 (Vector2 a, Vector2 b) {
        float angle = Vector2.SignedAngle (a, b);

        //clockwise

        //counter clockwise
        // else if (Mathf.Sign (angle) == 1)
        //     angle = -angle;
        return angle;
    }
    float Angle1 (Vector2 a, Vector2 b) {
        float angle = Vector2.Angle (a, b);

        //clockwise
        if (Mathf.Sign (angle) == -1)
            angle += 360;

        //counter clockwise
        // else if (Mathf.Sign (angle) == 1)
        //     angle = -angle;
        return angle;
    }
}