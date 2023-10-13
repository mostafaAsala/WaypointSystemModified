using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
namespace ASWS { 
public class SegmantGenPoints : MonoBehaviour
{
    public Transform A, B, C, D,NormA,normD;
    List<Bezier.PathPoint> points = new List<Bezier.PathPoint>();
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void createPoints()
    {
        points = Bezier.getSegmentPoints(A.position, B.position, C.position, D.position, NormA.position - A.position, normD.position - D.position,30);
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawSphere(points[i].pos,0.1f);
            Gizmos.DrawLine(points[i].pos, points[i].pos + points[i].normal);
        }
        Gizmos.DrawLine(A.position, NormA.position);
        Gizmos.DrawLine(D.position,normD.position);
        Vector3 Lpoint = normD.position-D.position;
        Vector3 Fpoint = NormA.position-A.position;
        //Debug.ClearDeveloperConsole();
        //Debug.LogError("A dir: "+(Vector3.Cross(Fpoint, B.position - A.position).y>0));
        //Debug.LogError("B dir: "+(Vector3.Cross(Lpoint, D.position - C.position).y>0));
        
        var a = Quaternion.Angle(
                Quaternion.LookRotation((B.position - A.position).normalized),
                Quaternion.LookRotation(((B.position - A.position)).normalized, NormA.position-A.position)
                );

        var d = Quaternion.Angle(
                Quaternion.LookRotation((D.position - C.position).normalized),
                Quaternion.LookRotation((D.position - C.position).normalized, normD.position-D.position)
                );
        Vector3 dir = (B.position - A.position).normalized;
        bool DA = Vector3.Cross(Fpoint, B.position - A.position).y > 0;
        if (DA) a = -a;
        bool DB = Vector3.Cross(Lpoint, D.position - C.position).y > 0;
        if (DB) d = -d;
        var angle = d - a;

        if (DA)
        {
            //angle = -angle;
        }
        if (angle >= 180) angle = (360  -angle);
        else if (angle <= -180) angle = 360+angle;
        
        if (/*(DA && !DB)  || (!DA && DB)*/ DA^DB)
        {
            //dir = -dir;
        }
        /*
        var qt = Quaternion.AngleAxis(angle, dir);
        var q2t = Quaternion.FromToRotation(dir, (D.position - C.position).normalized);
        var nt = qt * q2t * (NormA.position - A.position);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(D.position, D.position + nt);
        */

        /*


        //A  if Vector3.Cross(Fpoint, B.position - A.position).y>0  false

        //B  Vector3.Cross(Lpoint, D.position - C.position).y>0   false
        var qt = Quaternion.AngleAxis(angle, B.position - A.position);
        var q2t = Quaternion.FromToRotation((B.position - A.position).normalized, (D.position - C.position).normalized);
        var nt = qt * q2t * (NormA.position-A.position);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(D.position, D.position+nt);

        //A  if Vector3.Cross(Fpoint, B.position - A.position).y>0  false

        //B Vector3.Cross(Lpoint, D.position - C.position).y>0 true
        qt = Quaternion.AngleAxis(angle, A.position - B.position);
        q2t = Quaternion.FromToRotation((A.position - B.position).normalized, (D.position - C.position).normalized);
        nt = qt * q2t * (NormA.position - A.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(D.position, D.position + nt);


        //A  if Vector3.Cross(Fpoint, B.position - A.position).y>0  true
        angle = -angle;
        //B Vector3.Cross(Lpoint, D.position - C.position).y>0 true
        qt = Quaternion.AngleAxis(angle, B.position - A.position);
        q2t = Quaternion.FromToRotation((B.position - A.position).normalized, (D.position - C.position).normalized);
        nt = qt * q2t * (NormA.position - A.position);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(D.position, D.position + nt);

        //A  if Vector3.Cross(Fpoint, B.position - A.position).y>0  true

        //B  Vector3.Cross(Lpoint, D.position - C.position).y>0 false
        qt = Quaternion.AngleAxis(angle, A.position - B.position);
        q2t = Quaternion.FromToRotation((A.position - B.position).normalized, (D.position - C.position).normalized);
        nt = qt * q2t * (NormA.position - A.position);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(D.position, D.position + nt);
        */
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(SegmantGenPoints))]
class regEditor: Editor
{
    SegmantGenPoints s;
    private void OnEnable()
    {
        s= (SegmantGenPoints)target;
    }
    public override void OnInspectorGUI()
    {
        
        
        base.OnInspectorGUI();
        if (GUILayout.Button("test"))
        {
            
            s.createPoints();
        }
    }
    
    private void OnSceneGUI()
    {
        s.createPoints();
    
    }
    


}

#endif

}