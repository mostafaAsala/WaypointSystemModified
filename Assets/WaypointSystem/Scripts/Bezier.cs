using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASWS { 
public class Bezier : MonoBehaviour
{

        public static Vector3 EvaluateQuadratic(Vector3 A, Vector3 B, Vector3 C, float t)
        {
            Vector3 p0 = Vector3.Lerp(A, B, t);
            Vector3 p1 = Vector3.Lerp(B, C, t);
            return Vector3.Lerp(p0, p1, t);
        }
        public static Vector3 EvaluateQuadraticF(Vector3 A, Vector3 B, Vector3 C, float t)
        {
            return A * (t * t - 2 * t + 1) +
                   B * (2 - t - 2 * t * t) +
                   C * (t * t);
        }
        
        public static Vector3 EvaluteQubic(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            Vector3 p0 = EvaluateQuadratic(A, B, C, t);
            Vector3 p1 = EvaluateQuadratic(B, C, D, t);
            return Vector3.Lerp(p0, p1, t);
        }
        
        
        public static Vector3 EvaluteQubicF(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            return A * (-t * t * t + 3 * t * t + -3 * t + 1) +
                   B * (3 * t * t * t - 6 * t * t + 3 * t) +
                   C * (-3 * t * t * t + 3 * t * t) +
                   D * (t * t * t);
        }
        public static Vector3 GetCubicFirstOrderDerivative(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            return A * (-3 * t * t + 6 * t - 3) +
                   B * (9 * t * t - 12 * t + 3) +
                   C * (-9 * t * t + 6 * t) +
                   D * (3 * t * t);
        }
        
        public static Vector3 GetCubicSecondOrderDerivative(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            return A * (-6 * t + 6) +
                   B * (18 * t - 12) +
                   C * (-18 * t + 6 ) +
                   D * (6 * t);
        }

        /// <summary>
        /// get the curvature measure of the bezier curve
        /// </summary>
        /// <param name="A">point 1</param>
        /// <param name="B">point 2</param>
        /// <param name="C">point 3</param>
        /// <param name="D">point 4</param>
        /// <param name="t">progress along the curve</param>
        /// <returns>returns a normal vector to the curve with length propotional to the curve</returns>
        public static Vector3 GetCurveture(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            Vector3 vel = GetCubicFirstOrderDerivative(A, B, C, D, t);
            Vector3 acc = GetCubicSecondOrderDerivative(A, B, C, D, t);
            Vector3 det = Vector3.Cross(vel, acc);

            float velmag = vel.magnitude * vel.magnitude * vel.magnitude;
            Vector3 normal_curve = det / velmag;
            return normal_curve;
        }
        /// <summary>
        /// get the radius of the curve at point t
        /// </summary>
        /// <param name="A">point 1</param>
        /// <param name="B">point 2</param>
        /// <param name="C">point 3</param>
        /// <param name="D">point 4</param>
        /// <param name="t">progress along the curve</param>
        /// <returns>returns the radius of the curve at point t</returns>
        public static Vector3 GetCurveRadius(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            Vector3 vel = GetCubicFirstOrderDerivative(A, B, C, D, t);
            Vector3 normal_curve = GetCurveture(A, B, C, D, t);
            float radius = 1 / normal_curve.magnitude;
            Vector3 radius_dir = radius * Vector3.Cross(normal_curve, vel);
            return radius_dir;
        }
        
        public static Vector3 EvaluteQubicFuncSpaceBased(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            return Mathf.Pow(t, 3) * (-A + 3 * B - 3 * C + D)+
                   Mathf.Pow(t, 2) * (3*A-6*B+3*C)+
                   t               * (-3*A +3*B)+
                                     (A);
        }
        public static Vector3 EvaluteQubicFirstDerivativeSpaceBased(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            return 3 * Mathf.Pow(t, 2) * (-A + 3 * B - 3 * C + D) +
                   2 * Mathf.Pow(t, 1) * (3 * A - 6 * B + 3 * C) +
                    (-3 * A + 3 * B);
        }

        public float GetArcLength(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            float len=0;

            for(int i = 0; i < 10; i++)
            {

            }

            return len;
        }
        public static List<PathPoint> getSegmentPoints(Vector3 A, Vector3 B, Vector3 C, Vector3 D,Vector3 normalA,Vector3 normalD,int numpoerOfPoints)
        {
           
            normalA = Vector3.ProjectOnPlane(normalA, B - A).normalized;
            normalD = Vector3.ProjectOnPlane(normalD, D - C).normalized;
            float dt = 1.0f / numpoerOfPoints;
            if (dt == 0) return null;
            List<Bezier.PathPoint> points = new List<PathPoint>();
            Vector3 fpoint = A;
            Vector3 point;
            Quaternion ARot = Quaternion.LookRotation((B - A).normalized,normalA);
            var a = Quaternion.Angle(
                    Quaternion.LookRotation((B - A).normalized),
                    ARot
                    );
            
            var d = Quaternion.Angle(
                    Quaternion.LookRotation((D - C).normalized),
                    Quaternion.LookRotation((D - C).normalized, normalD)
                    );


            bool DA = Vector3.Cross(normalA, B - A).y > 0;
            if (DA) a = -a;
            bool DB = Vector3.Cross(normalD, D - C).y > 0;
            if (DB) d = -d;
            var angle = d - a;


            if (angle >= 180) angle = (360 - angle);
            else if (angle <= -180) angle = 360 + angle;
            angle /= numpoerOfPoints;
            
            Vector3 lastNorm=normalA,lastDir=B-A;
            for (float i = 0; i <0.95; i+=dt)
            {
                

                point = EvaluteQubic(A, B, C, D, i);
                var ndir = (point - fpoint).normalized;
                var q2 = Quaternion.FromToRotation(lastDir, ndir );
                var n = q2 * lastNorm ;

                n = Vector3.ProjectOnPlane(n, ndir).normalized;
                var q = Quaternion.AngleAxis(-angle , ndir);
                
                n = (q * n).normalized;
                points.Add(new PathPoint() { pos = point, normal = n });
                fpoint = point;
                lastDir = ndir;
                lastNorm = n;
            }


            return points;
        }

        public static Vector3[] EqualSpacedPath(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float space,Vector3 NormalA,Vector3 NormalD ,float resolution = 1)
        {
            List<Vector3> pathl = new List<Vector3>();


            float distanceLastPoint = 0;
            float ControlLength = Vector3.Distance(A, B) + Vector3.Distance(B,C) + Vector3.Distance(C, D);
            float estimatedCurveLength = Vector3.Distance(A, D) + ControlLength / 2;
            int divitions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            var lastpoint = D;
            while (t <= 1)
            {
                t += 1f / divitions;
                var point = EvaluteQubic(A, B, C, D, t);
                distanceLastPoint += Vector3.Distance(point, lastpoint);
                while (distanceLastPoint >= space)
                {
                    var overshoot = distanceLastPoint - space;
                    var newp = point + (lastpoint - point).normalized * overshoot;
                    pathl.Add(newp);
                    distanceLastPoint = overshoot;
                    lastpoint = newp;
                }
                lastpoint = point;

            }
            return pathl.ToArray();
        }

        private static (float,float,float) GetNormalAngle(BezierSegment bezierSegments)
        {
            
            var vecA = (bezierSegments.B - bezierSegments.A);
            Vector3 normalA = Vector3.Cross((vecA),  Vector3.right);
            if (normalA.y < 0) normalA = -normalA;
            float angleA = Vector3.SignedAngle(normalA, bezierSegments.NormalA, bezierSegments.B - bezierSegments.A);

            Vector3 normalD = Vector3.Cross( bezierSegments.D- bezierSegments.C,  Vector3.right);
            if (normalD.y < 0) normalD = -normalD;
            float angleD = Vector3.SignedAngle(normalD, bezierSegments.NormalD, bezierSegments.D- bezierSegments.C);
            float angle = angleD - angleA;
            if (angle > 180) angle = 360 - angle;
            if (angle < -180) angle = 360 + angle;

            //Debug.Log(angleA+", "+angleD+", "+angle);
            return (angleA, angleD,angle);

            
        }
        public Vector3 newVec(BezierSegment bezierSegments)
        {
            var vecA = (bezierSegments.B - bezierSegments.A);
            Vector3 normalA = Vector3.Cross((vecA), Vector3.right);
            if (normalA.y < 0) normalA = -normalA;
            float angleA = Vector3.SignedAngle(normalA, bezierSegments.NormalA, bezierSegments.B - bezierSegments.A);

            Vector3 normalD = Vector3.Cross(bezierSegments.D - bezierSegments.C, Vector3.right);
            if (normalD.y < 0) normalD = -normalD;
            float angleD = Vector3.SignedAngle(normalD, bezierSegments.NormalD, bezierSegments.D - bezierSegments.C);
            float angle = angleD - angleA;
            if (angle > 180) angle = 360 - angle;
            if (angle < -180) angle = 360 + angle;

            var q = Quaternion.LookRotation(vecA, bezierSegments.NormalA);
            var q2 = Quaternion.AngleAxis(angle, vecA);
            var q3 = q * q2 * Vector3.up;
            return q3;
        }


        public static float calcCurveLength(BezierSegment seg)
        {
            float dis = 0;
            Vector3 lastPoint = seg.A;
            for(int i = 0; i <= 10; i++)
            {
                Vector3 point = EvaluteQubic(seg.A, seg.B, seg.C, seg.D, i / 10);
                dis += Mathf.Abs((point - lastPoint).magnitude);
                lastPoint = point;
            }


            return dis;


        }
        public static List<PathPoint> EvalPath(List<BezierSegment> bezierSegments, float spacing, float resolution = 1)
        {
            List<PathPoint> pathl = new List<PathPoint>();
            for (int i = 0; i < bezierSegments.Count; i++)
            {
                var seg = bezierSegments[i];
                List<PathPoint> points =  Bezier.getSegmentPoints(seg.A, seg.B, seg.C, seg.D, seg.NormalA, seg.NormalD, 10);
                pathl.AddRange(points);
            }

            return pathl;
        }
            public static List<PathPoint> EvalPathEqualSpace(List<BezierSegment> bezierSegments,float spacing, float resolution=1)
        {
            if (bezierSegments==null|| bezierSegments.Count < 1) return null;
            List<PathPoint> pathl = new List<PathPoint>();
            Vector3 lastpoint = bezierSegments[0].A;
            float distanceLastPoint = 0;


            
            for (int i = 0; i < bezierSegments.Count ; i++)
            {
                //float ControlLength = Vector3.Distance(bezierSegments[i].A, bezierSegments[i].B) + Vector3.Distance(bezierSegments[i].B, bezierSegments[i].C) + Vector3.Distance(bezierSegments[i].C, bezierSegments[i].D);
                //float estimatedCurveLength = Vector3.Distance(bezierSegments[i].A, bezierSegments[i].D) + ControlLength / 2;
                //Debug.Log("i= " + estimatedCurveLength);
                float estimatedCurveLength = calcCurveLength(bezierSegments[i]);
                //Debug.Log("i= "+estimatedCurveLength);
                int divitions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
                divitions = divitions == 0 ? 1 : divitions;
                float t = 0;
                
                var bezierSegment = bezierSegments[i];
                var vecA = (bezierSegment.B - bezierSegment.A);
                var vecD = bezierSegment.D - bezierSegment.C;


                float cummulativeDist = 0;  
                GameObject obj = new GameObject();
                obj.transform.position = bezierSegments[i].A;
                obj.transform.rotation = Quaternion.LookRotation((bezierSegments[i].B- bezierSegments[i].A).normalized);
                float a = Vector3.SignedAngle(obj.transform.up, bezierSegment.NormalA, vecA);
                //Vector3 norm= obj.transform.InverseTransformPoint(bezierSegments[i].A+bezierSegments[i].NormalA);
                GameObject g = new GameObject();
                g.transform.position = bezierSegments[i].D;
                g.transform.rotation = Quaternion.LookRotation((bezierSegments[i].D - bezierSegments[i].C).normalized);
                float b = Vector3.SignedAngle(g.transform.up, bezierSegment.NormalD, vecD);

                float ang = a-b; //Vector3.SignedAngle(bezierSegments[i].NormalD, g.transform.up,vecD);
                if (ang > 180) ang = 360 - ang;
                if (ang < -180) ang = 360 + ang;

                float deltaangle = ang / divitions;
                obj.transform.rotation = Quaternion.LookRotation((bezierSegments[i].B - bezierSegments[i].A).normalized,bezierSegment.NormalA);
                var rot = obj.transform.rotation;

                Vector3 forward = obj.transform.forward;
                int pointnum = 0;
                DestroyImmediate(g);
                var ab = (bezierSegments[i].B - bezierSegments[i].A).normalized;
                while (t <= 1)
                {
                    //normal = GetCurveture(bezierSegments[i].A, bezierSegments[i].B, bezierSegments[i].C, bezierSegments[i].D, t).normalized;
                    /*pointnum++;
                    t += 1f / divitions;
                    var point = EvaluteQubic(bezierSegments[i].A, bezierSegments[i].B, bezierSegments[i].C, bezierSegments[i].D, t);
                    
                    var quat = Quaternion.LookRotation((bezierSegments[i].B - bezierSegments[i].A).normalized, bezierSegment.NormalA); 
                    obj.transform.rotation = Quaternion.FromToRotation(obj.transform.forward, point - lastpoint)*obj.transform.rotation ; //Quaternion.AngleAxis(-deltaangle * pointnum, point - lastpoint)* Quaternion.LookRotation((point - lastpoint).normalized, bezierSegment.NormalA); // * Quaternion.AngleAxis(deltaangle, (point - lastpoint).normalized);
                    obj.transform.position = point;
                    obj.transform.rotation = Quaternion.AngleAxis(-deltaangle ,point - lastpoint) * obj.transform.rotation;
                    Vector3 normal = obj.transform.TransformPoint(0, 1, 0) - obj.transform.position;                    //var normalP = EvaluteQubic(bezierSegments[i].A+bezierSegment.NormalA, bezierSegments[i].B, bezierSegments[i].C, bezierSegments[i].D+bezierSegment.NormalD, t);
                    distanceLastPoint += Vector3.Distance(point, lastpoint);
                    */

                    pointnum++;
                    t += 1f / divitions;
                    var point = EvaluteQubic(bezierSegments[i].A, bezierSegments[i].B, bezierSegments[i].C, bezierSegments[i].D, t);
                    var rot2 = Quaternion.FromToRotation((bezierSegments[i].B - bezierSegments[i].A).normalized, (point - lastpoint).normalized);

                    //var quat = Quaternion.LookRotation((bezierSegments[i].B - bezierSegments[i].A).normalized, bezierSegment.NormalA);
                    obj.transform.rotation = rot2 * rot; //Quaternion.AngleAxis(-deltaangle * pointnum, point - lastpoint)* Quaternion.LookRotation((point - lastpoint).normalized, bezierSegment.NormalA); // * Quaternion.AngleAxis(deltaangle, (point - lastpoint).normalized);
                    obj.transform.position = point;
                    obj.transform.rotation = Quaternion.AngleAxis(-deltaangle* (pointnum), point - lastpoint) * obj.transform.rotation;
                    Vector3 normal = obj.transform.TransformPoint(0, 1, 0) - obj.transform.position;                    //var normalP = EvaluteQubic(bezierSegments[i].A+bezierSegment.NormalA, bezierSegments[i].B, bezierSegments[i].C, bezierSegments[i].D+bezierSegment.NormalD, t);
                    distanceLastPoint += Vector3.Distance(point, lastpoint);


                    //normal = Vector3.Cross(normal, point - lastpoint).normalized;
                    //Quaternion deltaRot = Quaternion.AngleAxis(angle2 / divitions,point-lastpoint);

                    //normal = (deltaRot * normal).normalized;
                    //normal = Vector3.ProjectOnPlane((normalP - point),point-lastpoint).normalized;
                    while (distanceLastPoint >= spacing)
                    {
                        cummulativeDist += distanceLastPoint;
                        var overshoot = distanceLastPoint - spacing;
                        var newp = point + (lastpoint - point).normalized * overshoot;
                        

                        pathl.Add(new PathPoint { pos = newp, normal = normal });
                        distanceLastPoint = overshoot;
                        lastpoint = newp;
                    }
                    lastpoint = point;

                }
            DestroyImmediate(obj);
            }
            return pathl;
        }

        public static List<Vector3> EvalEqualSpacedPath(List<Vector3> points,float space,float resolution=1)
        {
            if (points.Count < 4) return null;
            List<Vector3> pathl=new List<Vector3>();
            Vector3 lastpoint= points[0];
            float distanceLastPoint = 0;
            for(int i = 0; i < points.Count - 3; i+=3)
            {
                float ControlLength = Vector3.Distance(points[i], points[i + 1]) + Vector3.Distance(points[i + 1], points[i + 2]) + Vector3.Distance(points[i + 2], points[i + 3]);
                float estimatedCurveLength = Vector3.Distance(points[i], points[i + 3]) + ControlLength/2;
                int divitions = Mathf.CeilToInt( estimatedCurveLength * resolution*10);
                float t = 0;
                while (t <= 1)
                {
                    t +=    1f/divitions;
                    var point = EvaluteQubic(points[i], points[i + 1], points[i + 2], points[i + 3], t);
                    distanceLastPoint += Vector3.Distance(point, lastpoint);
                    while (distanceLastPoint >= space)
                    {
                        var overshoot = distanceLastPoint - space;
                        var newp = point + (lastpoint - point).normalized * overshoot;
                        pathl.Add(newp);
                        distanceLastPoint = overshoot;
                        lastpoint = newp;
                    }
                    lastpoint = point;

                }
            }
            return pathl;

        }



        public static float CalculateApproxCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
        
            float shor = (p0 - p3).magnitude;
            float lon = (p0 - p1).magnitude + (p1 - p2).magnitude + (p3 - p2).magnitude;
            return shor+lon/2;
        }
        [System.Serializable ]
        public class PathPoint
        {
            public Vector3 pos, normal;
            public PathPoint(Vector3 ppos,Vector3 n)
            {
                pos = ppos;normal = n;
            }
            public PathPoint() { }
        }
        public class BezierSegment
        {
            public Vector3 A, B, C, D;
            public Vector3 NormalA, NormalD;
            public float CurveLength=-1;
        }
    }
}