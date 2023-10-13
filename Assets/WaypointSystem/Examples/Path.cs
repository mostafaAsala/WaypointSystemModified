using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ASWS { 
public class Path : MonoBehaviour
{

        public float spacing = 0.1f;
        public float resolution = 1;
        public List<Bezier.PathPoint> path;
        public int startIndex = 0;
        public int endIndex = 0;
        public int LoopIndex = 0;
        WaypointSystem system;
        public void Start()
        {
            system = FindObjectOfType<WaypointSystem>();
            CalcPath();
        }

        public void Update()
        {
            
        }

        public void CalcPath()
        {
            if (system == null) system = FindObjectOfType<WaypointSystem>();
            var segs = system.TraverseLoopSegments(LoopIndex, startIndex, endIndex);
            path = Bezier.EvalPath(segs, spacing, resolution);
        }
        public void CalcPathLinear()
        {
            if (system == null) system = FindObjectOfType<WaypointSystem>();
            var segs =system.GetPathpoints(system.loops[0].waypoints[0], system.loops[3].waypoints[7], true);
            path = Bezier.EvalPath(segs, spacing, resolution);
        }
        private void OnDrawGizmos()
        {
            if (path != null)
                for (int i = 0; i < path.Count; i++)
                {
                    Gizmos.DrawSphere(path[i].pos, 0.05f);
                    Gizmos.DrawLine(path[i].pos, path[i].pos + path[i].normal);
                }
        }

}


#if UNITY_EDITOR
[CustomEditor(typeof(Path))]
class Testeditor : Editor
{

    public override void OnInspectorGUI()
    {
        var t = (Path)target;
        base.OnInspectorGUI();
            if (GUILayout.Button("calculate Path"))
            {
                t.CalcPath();
            }
            if (GUILayout.Button("calculate Path Linear"))
            {
                t.CalcPathLinear();
            }
        }
}

#endif
}