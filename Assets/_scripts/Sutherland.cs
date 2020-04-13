using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sutherland {
    public static class SutherlandHodgman {
        #region Class : Edge
        /// <summary>
        /// This represents a line segment
        /// </summary>
        private class Edge {
            public Edge (Vector2 from, Vector2 to) {
                this.From = from;
                this.To = to;
            }

            public readonly Vector2 From;
            public readonly Vector2 To;
        }

        #endregion

        /// <summary>
        /// This clips the subject polygon against the clip polygon (gets the intersection of the two polygons)
        /// </summary>
        /// <remarks>
        /// Based on the psuedocode from:
        /// http://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman
        /// </remarks>
        /// <param name="subjectPoly">Can be concave or convex</param>
        /// <param name="clipPoly">Must be convex</param>
        /// <returns>The intersection of the two polygons (or null)</returns>
        public static Vector2[] GetIntersectedPolygon (Vector2[] subjectPoly, Vector2[] clipPoly) {
            if (subjectPoly.Length < 3 || clipPoly.Length < 3) {
                throw new ArgumentException (string.Format ("The polygons passed in must have at least 3 points: subject={0}, clip={1}", subjectPoly.Length.ToString (), clipPoly.Length.ToString ()));
            }

            List<Vector2> outputList = subjectPoly.ToList ();

            //	Make sure it's clockwise
            if (!IsClockwise (subjectPoly)) {
                outputList.Reverse ();
            }

            //	Walk around the clip polygon clockwise
            foreach (Edge clipEdge in IterateEdgesClockwise (clipPoly)) {
                List<Vector2> inputList = outputList.ToList (); //	clone it
                outputList.Clear ();

                if (inputList.Count == 0) {
                    //	Sometimes when the polygons don't intersect, this list goes to zero.  Jump out to avoid an index out of range exception
                    break;
                }

                Vector2 S = inputList[inputList.Count - 1];

                foreach (Vector2 E in inputList) {
                    if (IsInside (clipEdge, E)) {
                        if (!IsInside (clipEdge, S)) {
                            Vector2? point = GetIntersect (S, E, clipEdge.From, clipEdge.To);
                            if (point == null) {
                                throw new ApplicationException ("Line segments don't intersect"); //	may be colinear, or may be a bug
                            } else {
                                outputList.Add (point.Value);
                            }
                        }

                        outputList.Add (E);
                    } else if (IsInside (clipEdge, S)) {
                        Vector2? point = GetIntersect (S, E, clipEdge.From, clipEdge.To);
                        if (point == null) {
                            throw new ApplicationException ("Line segments don't intersect"); //	may be colinear, or may be a bug
                        } else {
                            outputList.Add (point.Value);
                        }
                    }

                    S = E;
                }
            }

            //	Exit Function
            return outputList.ToArray ();
        }

        #region Private Methods

        /// <summary>
        /// This iterates through the edges of the polygon, always clockwise
        /// </summary>
        private static IEnumerable<Edge> IterateEdgesClockwise (Vector2[] polygon) {
            if (IsClockwise (polygon)) {
                #region Already clockwise

                for (int cntr = 0; cntr < polygon.Length - 1; cntr++) {
                    yield return new Edge (polygon[cntr], polygon[cntr + 1]);
                }

                yield return new Edge (polygon[polygon.Length - 1], polygon[0]);

                #endregion
            } else {
                #region Reverse

                for (int cntr = polygon.Length - 1; cntr > 0; cntr--) {
                    yield return new Edge (polygon[cntr], polygon[cntr - 1]);
                }

                yield return new Edge (polygon[0], polygon[polygon.Length - 1]);

                #endregion
            }
        }

        /// <summary>
        /// Returns the intersection of the two lines (line segments are passed in, but they are treated like infinite lines)
        /// </summary>
        /// <remarks>
        /// Got this here:
        /// http://stackoverflow.com/questions/14480124/how-do-i-detect-triangle-and-rectangle-intersection
        /// </remarks>
        private static Vector2? GetIntersect (Vector2 line1From, Vector2 line1To, Vector2 line2From, Vector2 line2To) {
            Vector2 direction1 = line1To - line1From;
            Vector2 direction2 = line2To - line2From;
            var dotPerp = (direction1.x * direction2.y) - (direction1.y * direction2.x);

            // If it's 0, it means the lines are parallel so have infinite intersection points
            if (IsNearZero (dotPerp)) {
                return null;
            }

            Vector2 c = line2From - line1From;
            var t = (c.x * direction2.y - c.y * direction2.x) / dotPerp;
            //if (t < 0 || t > 1)
            //{
            //    return null;		//	lies outside the line segment
            //}

            //double u = (c.X * direction1.Y - c.Y * direction1.X) / dotPerp;
            //if (u < 0 || u > 1)
            //{
            //    return null;		//	lies outside the line segment
            //}

            //	Return the intersection point
            return line1From + (t * direction1);
        }

        private static bool IsInside (Edge edge, Vector2 test) {
            bool? isLeft = IsLeftOf (edge, test);
            if (isLeft == null) {
                //	Colinear points should be considered inside
                return true;
            }

            return !isLeft.Value;
        }
        private static bool IsClockwise (Vector2[] polygon) {
            for (int cntr = 2; cntr < polygon.Length; cntr++) {
                bool? isLeft = IsLeftOf (new Edge (polygon[0], polygon[1]), polygon[cntr]);
                if (isLeft != null) //	some of the points may be colinear.  That's ok as long as the overall is a polygon
                {
                    return !isLeft.Value;
                }
            }

            throw new ArgumentException ("All the points in the polygon are colinear");
        }

        /// <summary>
        /// Tells if the test point lies on the left side of the edge line
        /// </summary>
        private static bool? IsLeftOf (Edge edge, Vector2 test) {
            var tmp1 = edge.To - edge.From;
            var tmp2 = test - edge.To;

            double x = (tmp1.x * tmp2.y) - (tmp1.y * tmp2.x); //	dot product of perpendicular?

            if (x < 0) {
                return false;
            } else if (x > 0) {
                return true;
            } else {
                //	Colinear points;
                return null;
            }
        }

        private static bool IsNearZero (double testValue) {
            return Math.Abs (testValue) <= .000000001d;
        }

        #endregion
    }
}