using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ASWS;
using System;

using System.Linq;
namespace ASWS { 
public class RoadMeshGenerator : MonoBehaviour
{
    public WaypointSystem system;
    public List<Bezier.PathPoint> roadPoints; // List of road points
    public float roadWidth = 1f; // Width of the road
    
    Dictionary<WaypointLoop, CombineInstance> combineI;
    Dictionary<Waypoint, List<CombineInstance>> CombineBrances;
    [Range(-1,1)]
    public float branchShift = 0;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    public Mesh systemMesh;
    void Start()
    {
       
    }

    Mesh GenerateRoadMesh(List<Bezier.PathPoint> roadPoints=null ,bool connected=true,bool isBranch=false)
    {
        if (roadPoints == null)
            roadPoints = this.roadPoints;
        // Create a new mesh
        Mesh mesh = new Mesh();

        // Generate vertices and triangles for the road mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Bezier.PathPoint next;
        int roadSegmentCount = connected ? roadPoints.Count : roadPoints.Count - 1;
        for (int i = 0; i <= roadSegmentCount; i++)
        {
            Bezier.PathPoint currentPoint = roadPoints[i% roadPoints.Count];
            next = roadPoints[(i + 1)% roadPoints.Count];
            
            // Calculate the perpendicular direction to the road surface
            Vector3 perpendicular = -Vector3.Cross(currentPoint.normal, next.pos-currentPoint.pos).normalized;

            // Calculate the half width of the road
            float halfWidth = roadWidth * 0.5f *( (isBranch)?0.5f:1);
            // Calculate the left and right positions of the road segment
            Vector3 leftPosition = currentPoint.pos - perpendicular * halfWidth + halfWidth* perpendicular * branchShift * 0.5f;
            Vector3 rightPosition = currentPoint.pos + perpendicular * halfWidth + halfWidth* perpendicular * branchShift * 0.5f;

            // Add vertices for the left and right positions
            vertices.Add(leftPosition);
            vertices.Add(rightPosition);

            // Add triangles for the road segment
            if (i > 0)
            {
                int vertexIndex = i * 2;
                int prevVertexIndex = (i - 1) * 2;

                // First triangle
                triangles.Add(prevVertexIndex);
                triangles.Add(prevVertexIndex + 1);
                triangles.Add(vertexIndex);

                // Second triangle
                triangles.Add(vertexIndex);
                triangles.Add(prevVertexIndex + 1);
                triangles.Add(vertexIndex + 1);
            }
        }

        // Assign vertices and triangles to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Recalculate normals to ensure proper lighting
        mesh.RecalculateNormals();

        // Set the generated mesh to the mesh filter
        
        // Save the mesh as an asset
        return mesh;
    }
    public void setParam()
    {
        if (system == null) return;
        var loops = system.loops;
        
        combineI = new Dictionary<WaypointLoop, CombineInstance>();
        CombineBrances = new Dictionary<Waypoint, List<CombineInstance>>();
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        roadPoints.Clear();
        

        

        CombineInstance[] c = new CombineInstance[loops.Count];
        for (int i = 0; i < loops.Count; i++)
        {
            roadPoints.Clear();
            var w = loops[i].waypoints;
            
            for (int j = 0; j < w.Count; j++)
            {
                var wp = w[j];
                roadPoints.AddRange(wp.inBetweenPoints);    
                wp.onStateChanged += (object sender, EventArgs args) => {
                    onPointChanged(wp);
                    
                };
                if (wp.InBetweenBranches!=null && wp.InBetweenBranches.Length > 0)
                {
                    
                    List<CombineInstance> cI=new List<CombineInstance>();
                    for (int ib = 0; ib < wp.Branches.Count; ib++)
                    {

                        var branchPoints = wp.InBetweenBranches[ib];
                        var bp = wp.Branches[ib];
                        bp.onStateChanged+= (object sender, EventArgs args) => {
                            onPointChanged(wp);
                        };
                        Mesh branchMesh = GenerateRoadMesh(new List<Bezier.PathPoint>(branchPoints), false,true);
                        CombineInstance BranchCM = new CombineInstance();
                        BranchCM.mesh = branchMesh;
                        BranchCM.transform = system.transform.localToWorldMatrix;
                        cI.Add(BranchCM);
                    }
                    CombineBrances.Add(wp,cI);
                }
                /*
                  if (wp.inBetweenBranches!=null && wp.inBetweenBranches.Count > 0)
                {
                    print("branch found");
                    List<CombineInstance> cI=new List<CombineInstance>();
                    for (int ib = 0; ib < wp.Branches.Count; ib++)
                    {

                        var branchPoints = wp.inBetweenBranches[ib];
                        var bp = wp.Branches[ib];
                        bp.onStateChanged+= (object sender, EventArgs args) => {
                            onPointChanged(wp);
                        };
                        Mesh branchMesh = GenerateRoadMesh(branchPoints, false,true);
                        CombineInstance BranchCM = new CombineInstance();
                        BranchCM.mesh = branchMesh;
                        BranchCM.transform = transform.localToWorldMatrix;
                        cI.Add(BranchCM);
                    }
                    CombineBrances.Add(wp,cI);
                }
                */

            }

            Mesh m = GenerateRoadMesh();
            CombineInstance cW = new CombineInstance();
            cW.mesh = m; 
            cW.transform = system.transform.localToWorldMatrix;
            combineI.Add(loops[i], cW);
        }

        
        LoadMesh();
        GenMesh();
        


    }
    public void onPointChanged(Waypoint w)
    {
        
        roadPoints.Clear();
        var p = w.parent.waypoints;
        for (int k = 0; k < p.Count; k++)
        {
            roadPoints.AddRange(p[k].inBetweenPoints);
        }
           
        Mesh m = GenerateRoadMesh();
        CombineInstance cW = new CombineInstance();
        cW.mesh = m;
        cW.transform = system.transform.localToWorldMatrix;
        combineI[w.parent] = cW;
        GenMesh();
        /*
        if(w.inBetweenBranches!=null && w.inBetweenBranches.Count>0)
        {
            List<CombineInstance> branchI = new List<CombineInstance>();
            var bs = w.inBetweenBranches;
            for (int k = 0; k < bs.Count; k++)
            {
                m = GenerateRoadMesh(bs[k],false,true);
                cW = new CombineInstance();
                cW.mesh = m;
                cW.transform = transform.localToWorldMatrix;
                branchI.Add(cW);


            }

            CombineBrances[w] = branchI;
            

        }
        */

        if (w.InBetweenBranches != null && w.InBetweenBranches.Length > 0)
        {
            List<CombineInstance> branchI = new List<CombineInstance>();
            var bs = w.InBetweenBranches;
            for (int k = 0; k < bs.Length; k++)
            {
                m = GenerateRoadMesh(new List<Bezier.PathPoint>(bs[k]), false, true);
                cW = new CombineInstance();
                cW.mesh = m;
                cW.transform = system.transform.localToWorldMatrix;
                branchI.Add(cW);


            }

            CombineBrances[w] = branchI;


        }
        
    }
    public void GenMesh()
    {
        if (systemMesh == null) systemMesh = new Mesh();
        var mainList = new List<CombineInstance>(combineI.Values);
        var flatArray = new List<List<CombineInstance>>(CombineBrances.Values).SelectMany(list => list);
        mainList.AddRange(flatArray);
        var x = mainList.ToArray();
        
        systemMesh.CombineMeshes(x);

        meshFilter.sharedMesh = systemMesh;

    }
    public void LoadMesh()
    {
        string path = "Assets/RoadMesh.asset";
        string directory = System.IO.Path.GetDirectoryName(path);
        if (System.IO.Directory.Exists(directory))
        {
            meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        }
        else
        {
            systemMesh = new Mesh();
            meshFilter.sharedMesh = systemMesh;
            AssetDatabase.CreateAsset(systemMesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }
    public void SaveMesh()
    {
        string path = "Assets/RoadMesh.asset";
        AssetDatabase.CreateAsset(systemMesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    public void setup()
    {
        roadPoints.Clear();
        
        var loops = system.loops;

        CombineInstance[] c= new CombineInstance[loops.Count];
        for (int i = 0; i < loops.Count; i++)
        {
            roadPoints.Clear();
            var w = loops[i].waypoints;
            for(int j = 0; j < w.Count; j++) 
            {
                var vectors = w[j].inBetweenPoints;
                roadPoints.AddRange(vectors);
            }
            
        }
        
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(c);
        systemMesh = mesh;
        meshFilter.sharedMesh = systemMesh;
        
        string path = "Assets/RoadMesh.asset";
        AssetDatabase.CreateAsset(systemMesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
    }


    private void OnDrawGizmos()
    {
        /*for (int i = 0; i < roadPoints.Count; i++)
        {
            Gizmos.DrawSphere(roadPoints[i].pos, 2);
        }*/
    }


}



#if UNITY_EDITOR

[CustomEditor(typeof(RoadMeshGenerator))]
class RoadMeshGeneratorEditor : Editor
{
    RoadMeshGenerator self;
    bool en = false;
    private void OnEnable()
    {
        self = (RoadMeshGenerator)(target);
        self.setParam();
        //self.system.onSystemChanged -= self.UpdateEvent;
        //self.system.onSystemChanged += self.UpdateEvent;
    }
    
    public override void OnInspectorGUI()
    {
        
        base.OnInspectorGUI();

        if (GUILayout.Button("linkSystem"))
        {
            self.setParam();
        }
        
        if (en)
        {
            if (GUILayout.Button("setup"))
            {
                self.setup();
            }
        }



    }
}

#endif

}
