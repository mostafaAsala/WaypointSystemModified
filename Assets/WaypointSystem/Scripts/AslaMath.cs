using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ASWS { 
public class AslaMath 
{


    public static float shortestLineDistance(Vector3 p1, Vector3 dir1, Vector3 p2, Vector3 dir2)
    {

        return 0;
    }
     public static (Vector3 , Vector3) LineDistance(Vector3 p1,Vector3 dir1,Vector3 p2,Vector3 dir2)
        {
            dir2 = dir2.normalized;
            dir1 = dir1.normalized;
        {
            // i1 = (p1 + n *dir1.x)
            // i2 = (p2 + m *dir2.x)


            //dot(dir1 , p2+n*dir2 - p1+m*dir1)
            //dir1.x *( (p2.x + n *dir2.x)-(p1.x + m *dir1.x) ) +dir1.y *( (p2.y + n *dir2.y)-(p1.y + m *dir1.y) ) +dir1.z *( (p2.z + n *dir2.z)-(p1.z + m *dir1.z) ) = 0
            //dir2.x *( (p2.x + n *dir2.x)-(p1.x + m *dir1.x) ) +dir2.y *( (p2.y + n *dir2.y)-(p1.y + m *dir1.y) ) +dir2.z *( (p2.z + n *dir2.z)-(p1.z + m *dir1.z) ) = 0 
            // m=? , n=?

            //     eq1
            // n * (dir1.x*dir2.x  + dir1.y*dir2.y + dir1.z*dir2.y) - m *(dir1.x*dir1.x + dir1.y*dir1.y + dir1.z*dir1.z) + dot(dir1 , p2-p1 ) = 0
            //dir1.x (p2.x-p1.x) +  dir1.y (p2.y-p1.y) +  dir1.y (p2.y-p1.y) = dot(dir1 , p2-p1 )
            //(dir1.x*dir1.x + dir1.y*dir1.y + dir1.z*dir1.z) = 1
            //(dir1.x*dir2.x  + dir1.y*dir2.y + dir1.z*dir2.y) = dot(dir1,dir2)
            // n * dot(dir1,dir2) - m = -dot(dir1 , p2-p1 )


            // n - m /dot(dir1,dir2) = -dot(dir1 , p2-p1 )/dot(dir1,dir2);



            //     eq2
            //dir2.x *( (p2.x + n *dir2.x)-(p1.x + m *dir1.x) ) +dir2.y *( (p2.y + n *dir2.y)-(p1.y + m *dir1.y) ) +dir2.z *( (p2.z + n *dir2.z)-(p1.z + m *dir1.z) ) = 0 
            // n * (dir2.x*dir2.x  + dir2.y*dir2.y + dir2.z*dir2.y) - m *(dir2.x*dir1.x + dir2.y*dir1.y + dir2.z*dir1.z) + dot(dir2 , p2-p1 ) = 0
            //dir1.x (p2.x-p1.x) +  dir1.y (p2.y-p1.y) +  dir1.y (p2.y-p1.y) = dot(dir1 , p2-p1 )
            //(dir1.x*dir1.x + dir1.y*dir1.y + dir1.z*dir1.z) = 1
            //(dir1.x*dir2.x  + dir1.y*dir2.y + dir1.z*dir2.y) = dot(dir1,dir2)


            // -n  + m *dot(dir1,dir2) = dot(dir2 , p2-p1 )

            //m (dot(dir1,dir2) - 1/dot(dir1,dir2) = dot(dir2 , p2-p1 ) -dot(dir1 , p2-p1 )/dot(dir1,dir2)
            //m = [dot(dir2 , p2-p1 ) -dot(dir1 , p2-p1 )/dot(dir1,dir2)] / (dot(dir1,dir2) - 1/dot(dir1,dir2)


            // eq1 :  n - m /dot(dir1,dir2) + dot(dir1 , p2-p1 )/dot(dir1,dir2) = 0
            // eq2 :  n - m *dot(dir1,dir2) + dot(dir2 , p2-p1 ) = 0


            //s1: m * ( -dot(dir1,dir2)+ 1/ dot(dir1,dir2) ) + dot(dir2 , p2-p1 ) - dot(dir1 , p2-p1 )/dot(dir1,dir2) = 0
            //s1: m = ( dot(dir2 , p2-p1 ) - dot(dir1 , p2-p1 )/dot(dir1,dir2) ) / ( -dot(dir1,dir2)+ 1/ dot(dir1,dir2) ) ;
            //s2: n =  m *dot(dir1,dir2) - dot(dir2 , p2-p1 )
        }
        var deltap = p2 - p1;
            var DirDot = Vector3.Dot(dir1, dir2);
            var dir1delta = Vector3.Dot(dir1, deltap);
            var dir2delta = Vector3.Dot(dir2, deltap);

            var m = (dir2delta - dir1delta / DirDot) /(DirDot - 1 / DirDot);
            var n = m * DirDot - dir2delta;

            var i1 = p1 + m * dir1;
            var i2 = p2 + n * dir2;
            return (i1, i2);
            //( (p2.x + n *dir2.x)-(p1.x + m *dir1.x) ) *(dir1.x - dir2.x) +( (p2.y + n *dir2.y)-(p1.y + m *dir1.y) ) *(dir1.y - dir2.y) +( (p2.z + n *dir2.z)-(p1.z + m *dir1.z) ) *(dir1.z -dir2.z) = 0

            //( (p2.x + n *dir2.x)-(p1.x + m *dir1.x) ) *deltaDir.x +( (p2.y + n *dir2.y)-(p1.y + m *dir1.y) ) *deltaDir.y +( (p2.z + n *dir2.z)-(p1.z + m *dir1.z) ) *deltaDir.z = 0

        }

    public float ClosestPointinCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,float spacing, Ray ray, float dis=1f)
    {
        float ControlLength = Vector3.Distance(p1, p2) + Vector3.Distance(p2, p3) + Vector3.Distance(p3, p4);
        float estimatedCurveLength = Vector3.Distance(p1, p4) + ControlLength / 2;
        int divitions = Mathf.CeilToInt(estimatedCurveLength);
        
        var pnts = Bezier.EqualSpacedPath(p1, p4, p2, p3,spacing, Vector3.zero, Vector3.zero, divitions);

        for (int i = 0; i < pnts.Length - 1; i++)
        {
            Vector3 i1, i2;
            (i1, i2) = AslaMath.LineDistance(ray.origin, ray.direction, pnts[i], pnts[i + 1] - pnts[i]);
            if ((i2 - pnts[i]).magnitude < (pnts[i + 1] - pnts[i]).magnitude)
            {
                var d = Vector3.Distance(i1, i2);
                if (d < dis)
                {
                    dis = d;
                }
            }
        }
        return dis;
    }
    /// <summary>
    /// get the distance between point and a line
    /// </summary>
    /// <param name="worldRay">specify the point and direction</param>
    /// <param name="point">list of points</param>
    /// <returns></returns>
    public static float GetDistance(Ray worldRay, Vector3 point)
    {
        return GetDistance(worldRay.origin, worldRay.direction, point);
    }
    public static float GetDistance(Vector3 origin,Vector3 dir, Vector3 point)
    {
        var proj = Vector3.Project(point - origin, dir);
        var lastpoint = origin + proj;
        return Vector3.Distance(lastpoint, point);
    }
    /// <summary>
    /// get closest point to a ray from set of point
    /// </summary>
    /// <param name="ray"> specify the point and direction</param>
    /// <param name="points"> list of points</param>
    /// <param name="distanceSnap">return -1 if no point is closer than this distance (set to positive infinity if you want closest)</param>
    /// <returns></returns>
    public static int GetClosestPointToRay(Ray ray , Vector3[] points, ref float distanceSnap)
    {

        return GetClosestPointToRay(ray.origin, ray.direction, points, ref distanceSnap);
    }
    public static int GetClosestPointToRay(Vector3 origin, Vector3 dir, Vector3[] points, ref float distanceSnap)
    {

        int index = -1;
        for (int i = 0; i < points.Length; i++)
        {
            var dist = GetDistance(origin, dir, points[i]);
            if (dist < distanceSnap)
            {
                index = i;
                distanceSnap = dist;
            }
        }
        return index;
    }
}
}