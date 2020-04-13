using System.Collections.Generic;
using UnityEngine;

public class PolyLabel
{
    private const float EPSILON = 1E-8f;
    internal static List<Vector2> GetPointsInRegions(List<Polygon> regions, List<float> precisions)
    {
        //Get Points
        var list = new List<Vector2>();
        for (int i = 0; i < regions.Count; i++)
        {
            var region = regions[i];
            Vector3 midPoint = GetPointInRegion(region, precisions[i]);
            if (!isInPolygon(region, midPoint))
            {
                return null;
            }
            list.Add(midPoint);
        }
        //Remove bad regions
        {
            int i = 0;
            while (i < regions.Count)
            {
                var region = regions[i];
                var midPoint = list[i];
                var wn = WindingNumber(region, midPoint);
                if (wn != -1)
                {
                    regions.RemoveAt(i);
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
        return list;
    }
    private static int WindingNumber(Polygon poly, Vector2 p)
    {
        int n = poly.Count;

        poly.Add(new Vector2(poly[0].x, poly[0].y));

        var wn = 0; // the winding number counter

        // loop through all edges of the polygon
        for (int i = 0; i < n; i++)
        { // edge from V[i] to V[i+1]
            Vector3 thisP = poly[i];
            Vector3 nextP = poly[i + 1];
            if (thisP.x <= p.x)
            { // start y <= P.y
                if (nextP.x > p.x) // an upward crossing
                    if (isLeft(thisP, nextP, p) > 0) // P left of edge
                        ++wn; // have a valid up intersect
            }
            else
            { // start y > P.y (no test needed)
                if (nextP.x <= p.x) // a downward crossing
                    if (isLeft(thisP, nextP, p) < 0) // P right of edge
                        --wn; // have a valid down intersect
            }
        }
        return wn;
    }

    private static int isLeft(Vector3 P0, Vector3 P1, Vector3 P2)
    {
        var calc = ((P1.y - P0.y) * (P2.x - P0.x) -
            (P2.y - P0.y) * (P1.x - P0.x));
        if (calc > 0)
            return 1;
        else if (calc < 0)
            return -1;
        else
            return 0;
    }

    private static Vector3 GetPointInRegion(Polygon region, float precision)
    {
        if (region.Count > 3)
        {
            // precision = Mathf.Clamp(precision, 0.001f, 0.1f);
            Vector3 midPoint = GetMidPoint(region, precision);
            // var midPoint = PolyLabel.GetPolyLabel(region, precision);
            return midPoint;
        }
        else
        {
            var midPoint = Vector3.zero;
            foreach (var item in region)
            {
                midPoint += item;
            }
            return midPoint /= region.Count;
        }
    }

    private static Vector2 GetMidPoint(Polygon region, float precision)
    {
        var centroid = PolyLabel.GetPolyLabel(region, precision);
        if (!isInPolygon(region, centroid))
        {
            centroid = PolyLabel.GetPolyLabel(region, precision / 5f);
            if (!isInPolygon(region, centroid))
            {
                centroid = PolyLabel.GetPolyLabel(region, precision / 20f);
                return centroid;
            }
            else
            {
                return centroid;
            }
        }
        else
        {
            return centroid;
        }
    }

    static bool isInPolygon(Polygon region, Vector2 test)
    {
        bool c = false;
        int j = region.Count - 1;
        for (int i = 0; i < region.Count; j = i++)
        {
            if (((region[i].y > test.y) != (region[j].y > test.y)) &&
                (test.x < (region[j].x - region[i].x) * (test.y - region[i].y) / (region[j].y - region[i].y) + region[i].x))
            {
                c = !c;
            }
        }
        return c;
    }
    public static Vector2 GetPolyLabel(Polygon polygon, float precision = 0.01f)
    {
        //Find the bounding box of the outer ring
        float minX = 0, minY = 0, maxX = 0, maxY = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            var p = polygon[i];
            if (i == 0 || p.x < minX)
                minX = p.x;
            if (i == 0 || p.y < minY)
                minY = p.y;
            if (i == 0 || p.x > maxX)
                maxX = p.x;
            if (i == 0 || p.y > maxY)
                maxY = p.y;
        }

        float width = maxX - minX;
        float height = maxY - minY;
        float cellSize = Mathf.Min(width, height);
        float h = cellSize / 2;

        //A priority queue of cells in order of their "potential" (max distance to polygon)
        PriorityQueue<float, Cell> cellQueue = new PriorityQueue<float, Cell>();

        if (FloatEquals(cellSize, 0))
            return new Vector2(minX, minY);

        //Cover polygon with initial cells
        for (float x = minX; x < maxX; x += cellSize)
        {
            for (float y = minY; y < maxY; y += cellSize)
            {
                Cell cell = new Cell(x + h, y + h, h, polygon);
                cellQueue.Enqueue(cell.Max, cell);
            }
        }

        //Take centroid as the first best guess
        Cell bestCell = GetCentroidCell(polygon);

        //Special case for rectangular polygons
        Cell bboxCell = new Cell(minX + width / 2, minY + height / 2, 0, polygon);
        if (bboxCell.D > bestCell.D)
            bestCell = bboxCell;

        int numProbes = cellQueue.Count;

        while (cellQueue.Count > 0)
        {
            //Pick the most promising cell from the queue
            Cell cell = cellQueue.Dequeue();

            //Update the best cell if we found a better one
            if (cell.D > bestCell.D)
            {
                bestCell = cell;
            }

            //Do not drill down further if there's no chance of a better solution
            if (cell.Max - bestCell.D <= precision)
                continue;

            //Split the cell into four cells
            h = cell.H / 2;
            Cell cell1 = new Cell(cell.X - h, cell.Y - h, h, polygon);
            cellQueue.Enqueue(cell1.Max, cell1);
            Cell cell2 = new Cell(cell.X + h, cell.Y - h, h, polygon);
            cellQueue.Enqueue(cell2.Max, cell2);
            Cell cell3 = new Cell(cell.X - h, cell.Y + h, h, polygon);
            cellQueue.Enqueue(cell3.Max, cell3);
            Cell cell4 = new Cell(cell.X + h, cell.Y + h, h, polygon);
            cellQueue.Enqueue(cell4.Max, cell4);
            numProbes += 4;
        }

        return (new Vector2(bestCell.X, bestCell.Y));
    }

    //Signed distance from point to polygon outline (negative if point is outside)
    private static float PointToPolygonDist(float x, float y, Polygon polygon)
    {
        bool inside = false;
        float minDistSq = float.PositiveInfinity;

        var ring = polygon;

        for (int i = 0, len = ring.Count, j = len - 1; i < len; j = i++)
        {
            var a = ring[i];
            var b = ring[j];

            if ((a.y > y != b.y > y) && (x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x))
                inside = !inside;

            minDistSq = Mathf.Min(minDistSq, GetSegDistSq(x, y, a, b));
        }

        return ((inside ? 1 : -1) * (float)Mathf.Sqrt(minDistSq));
    }

    //Get squared distance from a point to a segment
    private static float GetSegDistSq(float px, float py, Vector2 a, Vector2 b)
    {
        float x = a.x;
        float y = a.y;
        float dx = b.x - x;
        float dy = b.y - y;

        if (!FloatEquals(dx, 0) || !FloatEquals(dy, 0))
        {
            float t = ((px - x) * dx + (py - y) * dy) / (dx * dx + dy * dy);
            if (t > 1)
            {
                x = b.x;
                y = b.y;
            }
            else if (t > 0)
            {
                x += dx * t;
                y += dy * t;
            }
        }
        dx = px - x;
        dy = py - y;
        return (dx * dx + dy * dy);
    }

    //Get polygon centroid
    private static Cell GetCentroidCell(Polygon polygon)
    {
        float area = 0;
        float x = 0;
        float y = 0;
        var ring = polygon;

        for (int i = 0, len = ring.Count, j = len - 1; i < len; j = i++)
        {
            var a = ring[i];
            var b = ring[j];
            float f = a.x * b.y - b.x * a.y;
            x += (a.x + b.x) * f;
            y += (a.y + b.y) * f;
            area += f * 3;
        }
        if (FloatEquals(area, 0))
            return (new Cell(ring[0].x, ring[0].y, 0, polygon));
        return (new Cell(x / area, y / area, 0, polygon));
    }

    private static bool FloatEquals(float a, float b)
    {
        return (Mathf.Abs(a - b) < EPSILON);
    }

    private class Cell
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float H { get; private set; }
        public float D { get; private set; }
        public float Max { get; private set; }

        public Cell(float x, float y, float h, Polygon polygon)
        {
            X = x;
            Y = y;
            H = h;
            D = PointToPolygonDist(X, Y, polygon);
            Max = D + H * (float)Mathf.Sqrt(2);
        }
    }
}