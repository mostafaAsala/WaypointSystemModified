using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ASWS { 
public class PathFollower : MonoBehaviour
{
    public float spacing = 0.1f;
    public float resolution = 1;

    public WaypointSystem system;
    public List<Bezier.PathPoint> path;
    public int startIndex = 0;
    public int endIndex = 0;
    public int StartLoopIndex = 0;
    public int EndLoopIndex = 0;
    public int pointIndex = 0;
    public float stepPerunit = 0;
    public float speed = 5;
    private Vector3 lastpos;
    private Quaternion lastQ;
    // Start is called before the first frame update
    public void Start()
    {

        CalcPath();
        if (path!=null && path.Count > 2) {
            transform.position = path[0].pos;
            lastpos = transform.position;
            transform.rotation = Quaternion.LookRotation(path[1].pos - path[0].pos, path[0].normal);
            lastQ = transform.rotation;
        }
    }

    public void step()
    {
        if (path == null) return;
        if (pointIndex >= path.Count - 2)
        {
            Debug.Log("reached final");
            return;
        }
        transform.position = Vector3.Lerp(lastpos, path[pointIndex + 1].pos, stepPerunit);
        transform.rotation = Quaternion.Lerp(lastQ, Quaternion.LookRotation(path[pointIndex + 2].pos - path[pointIndex + 1].pos, path[pointIndex + 1].normal), stepPerunit);
        if (stepPerunit < 1)
            stepPerunit += Time.deltaTime* speed;
        else
        {
            lastpos = transform.position;
            lastQ = transform.rotation;
            stepPerunit = 0;
            pointIndex++;

        }
    }
    // Update is called once per frame
    void Update()
    {
        step();
    }

    public void CalcPath()
    {
        if (system == null) return;
        var segs = system.GetPathpoints(system.loops[StartLoopIndex].waypoints[startIndex], system.loops[EndLoopIndex].waypoints[endIndex], true);
        
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
}