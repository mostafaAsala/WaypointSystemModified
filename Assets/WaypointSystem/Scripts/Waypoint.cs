using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ASWS { 


    enum WaypointType
    {
        Road,
        walker
    }


    public class Waypoint : MonoBehaviour, ISerializationCallbackReceiver
    {
        #region Variables
        [Tooltip("unique id based on the position")]
        private int _id;
        [Tooltip("the loop that contains the waypoint")]
        public WaypointLoop parent;
        [Tooltip("stores the data of the waypoint in a json format")]
        string json;
        [Tooltip("the next waypoint in the path, null if the last waypoint")]
        public Waypoint Next;

        [Tooltip("the previous waypoint in the path, null if the first waypoint")]
        public Waypoint previous;
        [Tooltip("the handle that control the previous link\nused in creating bezier bath")]
        public Transform HandleA;
        [Tooltip("the handle that control the next link\nused in creating bezier bath")]
        public Transform HandleB;
        [Tooltip("True: handles work together as one line result in continous path at the point\nfalse: handles work separately result in a break in the continouity of the path")]
        public bool LockHandles;
        [Tooltip("controls if the link to the next waypoint is visible or not")]
        private bool DrawLink = false;
        [Tooltip("temporary saved data for 2D loop")]
        private float localYoffset;
        [Tooltip("temporary saved data for 2D loop")]
        private float HandleAoffset;
        [Tooltip("temporary saved data for 2D loop")]
        private float HandleBoffset;
        [Tooltip("gets the intermediate path points between the the point and the next waypoint")]
        public List<Bezier.PathPoint> inBetweenPoints;
        [SerializeField]
        [Tooltip("gets the intermediate path points between the the point and the Branches")]
        public Bezier.PathPoint[][] InBetweenBranches;
        [Tooltip("list of waypoints to connect to")]
        public List<Waypoint> Branches; 
        [Tooltip("list of waypoints that is connected to this point")]
        public List<Waypoint> ReverseBranch; 
        public bool enterance=false, exit = false;
        public float normalangle = 0;
        [Tooltip("vector directed to the normal to the waypoint, normat to the direction to the nect point")]
        public Vector3 normalDir;
        [Tooltip("distane between intermediate path points between the point and the next point")]
        public float distanceBetweenPoints = 1f;
        [Tooltip("event is run when point change position or change direction or handles")]
        public event EventHandler onStateChanged;
        [Tooltip("event is run when point is deleted")]
        public event EventHandler onDeleted;
        [Tooltip("draw in between points and normals")]
        public bool drawinbetween = true;
        public int GizmoMode = 0;

        /// <summary>
        /// return the inbetween point using the next
        /// </summary>
        /// <param name="Next"></param>
        /// <returns>list of bezier points, null if the Next is not connected to the current point</returns>
        
        public List<Bezier.PathPoint> getInbetween(Waypoint Next)
        {
            if (Next == this.Next)
                return inBetweenPoints;

            for (int i = 0; i < Branches.Count; i++)
            {
                if (Branches[i] == Next)
                    return new List< Bezier.PathPoint > (InBetweenBranches[i]);
            }
            return null;
        }
        /// <summary>
        /// return the inbetween points using index of the waypoint
        /// </summary>
        /// <param name="id">the index of the branch, -1 if you want next</param>
        /// <returns>list of bezier points, null if the Next is not connected to the current point</returns>
        public List<Bezier.PathPoint> getInbetween(int id)
        {
            if (id == -1) return inBetweenPoints;
            else if (id< InBetweenBranches.Length)
                return new List<Bezier.PathPoint>(InBetweenBranches[id]);
            return null;
        }

        /// <summary>
        /// return the bezier path points between any two waypoints regardless the system
        /// </summary>
        /// <param name="A">start point</param>
        /// <param name="B">end point</param>
        /// <returns>return list of points</returns>
        public static List<Bezier.PathPoint> getInbetween(Waypoint A,Waypoint B)
        {
            if (A == null || B == null) return null;
            Vector3 p0, p1, p2, p3;
            p0 = A.GetPosition();
            p1 = A.HandleB.position;

            p2 = B.HandleA.position;
            p3 = B.GetPosition();
            float chord, cont_net, app_arc_length;

            chord = (p3 - p0).magnitude;
            cont_net = (p0 - p1).magnitude + (p2 - p1).magnitude + (p3 - p2).magnitude;
            app_arc_length = (cont_net + chord) / 2;
            return Bezier.getSegmentPoints(p0, p1, p2, p3, A.normalDir, B.normalDir, (int)(app_arc_length / A.distanceBetweenPoints));

            
        }
        public override string ToString()
        {
            return parent.name+"->"+name;
        }
        /// <summary>
        /// recalculate point parameters if state changed.
        /// you can call it whenever you want
        /// </summary>
        public void OnStateChanged() { 
            onStateChanged?.Invoke(this, EventArgs.Empty);
            RecalculateInBetween();
            RecalculateInBranches();
            RecalculateReverseBranches();
            if(previous!=null)
            previous.RecalculateInBetween();
           
        }
        /// <summary>
        /// clean up after deleting waypoint
        /// </summary>
        public void OnDeleted() { onDeleted?.Invoke(this, EventArgs.Empty); }

        #region Gizmo
        [Range(0,1)]
        public static float gizmoSize=0.1f;
        private Color selectedColor = Color.blue;
        private Color normalColor = Color.green;
        private Color color;

        #endregion
        #endregion


        #region Setters and Getters

        public int id { get { return _id; } }


        #endregion
        // Start is called before the first frame update

        void Start()
        {
            
            if (Branches == null)
                Branches = new List<Waypoint>();
            var relativeposition = (transform.position - WaypointSystem.Instance.transform.position);
            _id = (int)(relativeposition.x + WaypointSystem.Instance.WorldSize * relativeposition.z);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>return the vector to the next point tangent to the curve</returns>
        public Vector3 getForwardVec()
        {
            return (HandleB.transform.position - transform.position).normalized;
        }
        /// <summary>
        /// calculate in between opoints between waypoint and its brances.
        /// called from the waypoint when changed state
        /// </summary>
        /// <param name="Branches">list of waypoints to calculate hte inbetween, could be any points</param>
        public void RecalculateBranches(List<Waypoint> Branches)
        {   
            if (Branches == null) return;
            //inBetweenBranches = new List<List<Bezier.PathPoint>>();
            Vector3 p0, p1, p2, p3;
            p0 = GetPosition();
            p1 = HandleB.position;

            float chord, cont_net, app_arc_length;
            InBetweenBranches = new Bezier.PathPoint[Branches.Count][];
            for (int i = 0; i < Branches.Count; i++)
            {
                var b = Branches[i];
                p2 = b.HandleA.position;
                p3 = b.GetPosition();
                chord = (p3 - p0).magnitude;
                cont_net = (p0 - p1).magnitude + (p2 - p1).magnitude + (p3 - p2).magnitude;
                app_arc_length = (cont_net + chord) / 2;

                var points = Bezier.getSegmentPoints(GetPosition(), HandleB.position, b.HandleA.position, b.GetPosition(), normalDir, b.normalDir, (int)(app_arc_length / distanceBetweenPoints));
                points.Add(new Bezier.PathPoint(p3, b.normalDir));
                //inBetweenBranches.Add(points);
                InBetweenBranches[i] = points.ToArray();
            }
            


        }
        /// <summary>
        /// calculate in between opoints between waypoint and its brances.
        /// called from the waypoint when changed state
        /// </summary>
        public void RecalculateInBranches()
        {
            RecalculateBranches(Branches);
        }
        public void recalculateAll()
        {
            RecalculateInBetween();
            RecalculateInBranches();

        }
        /// <summary>
        /// calculate in between opoints between waypoint and its brances.
        /// called by the brance when it changes state to notify the source of branch (waypoint)
        /// </summary>
        public void RecalculateReverseBranches()
        {
            if (ReverseBranch == null) return;
            Vector3 p0, p1, p2, p3;
            p3 = GetPosition();
            p2 = HandleA.position;

            float chord, cont_net, app_arc_length;

            for (int i = 0; i < ReverseBranch.Count; i++)
            {
                var b = ReverseBranch[i];
                p1 = b.HandleB.position;
                p0 = b.GetPosition();
                chord = (p3 - p0).magnitude;
                cont_net = (p0 - p1).magnitude + (p2 - p1).magnitude + (p3 - p2).magnitude;
                app_arc_length = (cont_net + chord) / 2;

                var points = Bezier.getSegmentPoints(p0, p1, p2, p3, b.normalDir, normalDir, (int)(app_arc_length / distanceBetweenPoints));
                points.Add(new Bezier.PathPoint(p3, b.normalDir));
                //points.Add(new Bezier.PathPoint(p3 + HandleB.position * 0.1f, b.normalDir));
                int remoteB = 0;
                for (; remoteB < b.Branches.Count; remoteB++)
                {
                    if (b.Branches[i] == this)
                        break;
                } 
                if (remoteB < b.Branches.Count)
                {
                    //b.inBetweenBranches[remoteB] = (points);
                    if (b.InBetweenBranches == null)
                        b.InBetweenBranches = new Bezier.PathPoint[b.Branches.Count][];
                    b.InBetweenBranches[remoteB] = points.ToArray();
                }
            }

        }
        /// <summary>
        /// calculate in between opoints between waypoint and its brances.
        /// called from the waypoint when changed state
        /// </summary>
        public void RecalculateInBetween()
        {
            Vector3 p0, p1, p2, p3;
            p0 = GetPosition();
            p1 = HandleB.position;

            float chord, cont_net, app_arc_length;
            if (Next != null) {
                p2 = Next.HandleA.position;
                p3 = Next.GetPosition();
                chord = (p3 - p0).magnitude;
                cont_net = (p0 - p1).magnitude + (p2 - p1).magnitude + (p3 - p2).magnitude;
                app_arc_length = (cont_net + chord) / 2;
                inBetweenPoints = Bezier.getSegmentPoints(p0,p1,p2,p3, normalDir, Next.normalDir, (int)(app_arc_length/distanceBetweenPoints));
                //inBetweenPoints.Add(new Bezier.PathPoint(p3, Next.normalDir));
                //inBetweenBranches = new List<List<Bezier.PathPoint>>();
            }
            
        }
        
        /// <summary>
        /// change position of the waypoint
        /// better choose this if you need to change posiiotn from code
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            OnStateChanged();
        }
        public Vector3 GetPosition() { return transform.position; }
        /// <summary>
        /// recalculate the direction of the normal based on the direction of the next and the right
        /// </summary>
        public void recalculateNormal()
        {
            Vector3 dir = getForwardVec();
            Vector3 p0, p1, p2, p3;
            if (Next != null)
            {
                p0 = GetPosition();
                p1 = HandleB.position;
                p2 = Next.GetPosition();
                p3 = Next.HandleA.position;
                Vector3 r = Bezier.GetCurveRadius(p0, p1, p2, p3, 0);
                Vector3 proj = Vector3.Cross(getForwardVec(),r);
                normalDir = -proj.normalized;
            }
            else if (previous != null)
            {
                p0 = GetPosition();
                p1 = HandleA.position;
                p3 = previous.GetPosition();
                p2 = previous.HandleB.position;
                Vector3 r = Bezier.GetCurveRadius(p0, p1, p2, p3, 0);
                Vector3 proj = Vector3.Cross(getForwardVec(), r);
                normalDir = proj;
            }

            
        }
        /// <summary>
        /// change the waypoint to flat yawpoint no Y axis
        /// </summary>
        /// <param name="flat"></param>
        public void SetFlat(bool flat)
        {
            if (flat) { 
                localYoffset = transform.localPosition.y;
                HandleAoffset = HandleA.localPosition.y;
                HandleBoffset = HandleB.localPosition.y;
                transform.localPosition = new Vector3(transform.localPosition.x,0, transform.localPosition.z);
                HandleA.localPosition = new Vector3(HandleA.localPosition.x, 0, HandleA.localPosition.z);
                HandleB.localPosition = new Vector3(HandleB.localPosition.x, 0, HandleB.localPosition.z);

                Vector3 dir = getForwardVec();
                Vector3 proj = Vector3.ProjectOnPlane(normalDir, dir).normalized;
                normalDir = proj;
                


            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x, localYoffset, transform.localPosition.z);
                HandleA.localPosition = new Vector3(HandleA.localPosition.x, HandleAoffset, HandleA.localPosition.z);
                HandleB.localPosition = new Vector3(HandleB.localPosition.x, HandleBoffset, HandleB.localPosition.z);
                Vector3 dir = getForwardVec();
                Vector3 proj = Vector3.ProjectOnPlane(normalDir, dir).normalized;
                normalDir = proj;

            }
        }
        public void saveYOffset()
        {
            localYoffset = transform.localPosition.y;
            HandleAoffset = HandleA.localPosition.y;
            HandleBoffset = HandleB.localPosition.y;
        }
        /// <summary>
        /// removes branch from loop
        /// </summary>
        /// <param name="branch"></param>
        public void RemoveBranch(Waypoint branch)
        {
            branch.ReverseBranch.Remove(this);
            Branches.Remove(branch);
            recalculateAll();

        }
        /// <summary>
        /// connect waypoint as a branch
        /// </summary>
        /// <param name="branch"></param>
        public void AddBranch(Waypoint branch)
        {

            if(parent==branch.parent)
            {
                Debug.Log("can't link two waypoints belonging to the same loop.");
                return;
            }
            if (Branches == null) Branches = new List<Waypoint>();
            if (!Branches.Contains(branch))
            {
                Branches.Add(branch);
                parent.exits.Add(this);
                branch.parent.entrances.Add(branch);
                if (branch.ReverseBranch == null) branch.ReverseBranch = new List<Waypoint>();
                branch.ReverseBranch.Add(this);
            }
            else
                Debug.Log("branch already exits");
            RecalculateInBranches();
        }
        /// <summary>
        /// set up handles at waypoint creation
        /// </summary>
        public void CreateHandles()
        {
            if (HandleA == null)
            {
                HandleA = new GameObject("HandleA").transform;
                HandleA.parent = transform;
                HandleA.transform.position = transform.position - transform.forward;
            }
            if (HandleB == null)
            {
                HandleB = new GameObject("HandleB").transform;
                HandleB.parent = transform;

                HandleB.transform.position = transform.position + transform.forward;
            }
        }

       
#if UNITY_EDITOR
        /// <summary>
        /// geenerate unique id based on the position of the waypoint in world space
        /// </summary>
        public void RecalculateID()
        {
            var relativeposition = (transform.position - WaypointSystem.Instance.transform.position);
            _id = (int)(relativeposition.x + WaypointSystem.Instance.WorldSize * relativeposition.z);
        }
#endif
        public Waypoint GetNextWaypoint()
        {
            return Next;
        }

        /// <summary>
        /// update the handle based on the other handle state
        /// this function works on lockHandle mode to ensure the handles are in the same line
        /// </summary>
        /// <param name="updateA">true if you want to update handleA, else update handleB</param>
        public void UpdateHandle( bool updateA)
        {
            if (!(HandleA && HandleB)) return;
            var point = updateA ? HandleB.transform : HandleA.transform;
            var otherpoint = updateA ? HandleA.transform : HandleB.transform;
            var distance = (otherpoint.position - transform.position).magnitude;
            var dir = -(point.position - transform.position).normalized;
            otherpoint.position = dir * distance + transform.position;
        }

        private void OnDrawGizmos()
        {
            var color = normalColor;
            Gizmos.color = color;
            
            if(Next && DrawLink)
                Gizmos.DrawLine(transform.position, Next.transform.position);
            
            if (drawinbetween && GizmoMode!=0)
            {
                
                if (inBetweenPoints != null)
                {
                    for (int i = 0; i < inBetweenPoints.Count; i++)
                    {
                        var p = inBetweenPoints[i];
                        if (GizmoMode == 1)
                        {
                            Gizmos.DrawSphere(p.pos, 0.1f);
                        }else if(GizmoMode==2)
                        {
                            if (i!= inBetweenPoints.Count - 1)
                            {
                                Gizmos.DrawLine(p.pos, inBetweenPoints[i + 1].pos);
                            }
                        }else if (GizmoMode == 3) { 
                            Gizmos.DrawLine(p.pos, p.pos + p.normal);
                            if (i != inBetweenPoints.Count - 1)
                            {
                                Gizmos.DrawLine(p.pos, inBetweenPoints[i + 1].pos);
                            }
                        }
                    }
                }
                Gizmos.color = color + new Color(50, 0, 0);
                if (InBetweenBranches != null)
                {
                    for (int i = 0; i < InBetweenBranches.Length; i++)
                    {
                        for (int j = 0; j < InBetweenBranches[i].Length; j++)
                        {
                            var p = InBetweenBranches[i][j]; if (GizmoMode == 1)
                            {
                                Gizmos.DrawSphere(p.pos, 0.1f);
                            }
                            else if (GizmoMode == 2)
                            {
                                if (j < InBetweenBranches[i].Length - 1)
                                {
                                    Gizmos.DrawLine(p.pos, InBetweenBranches[i][j+1].pos);
                                }
                            }
                            else if (GizmoMode == 3)
                            {
                                Gizmos.DrawLine(p.pos, p.pos + p.normal);
                                if (j < InBetweenBranches[i].Length - 1)
                                {
                                    Gizmos.DrawLine(p.pos, InBetweenBranches[i][j + 1].pos);
                                }
                            }
                        }
                    }
                }
            }
        }
        public void DrawHandle(Transform h)
        {
            Gizmos.color = Color.cyan;
            if (h == null) return;
            Gizmos.DrawWireSphere(h.position, gizmoSize * 0.5f);

            Gizmos.DrawLine(transform.position, h.position);
            
        }

        public void OnBeforeSerialize()
        {
            json = "";
            if (InBetweenBranches != null) { 
                SerializableListWrapper s = new SerializableListWrapper();
                s.nestedList = new SerializableListWrapperL2[InBetweenBranches.Length];
                for (int i = 0; i < s.nestedList.Length; i++)
                {
                    var x = InBetweenBranches[i];
                    s.nestedList[i] = new SerializableListWrapperL2();
                    s.nestedList[i].nestedList = x;


                }
                json = JsonUtility.ToJson(s);
               
            }
        }

        public void OnAfterDeserialize()
        {
            if (json != "") { 
                var s = JsonUtility.FromJson<SerializableListWrapper>(json);
                if (s != null) { 
                    InBetweenBranches = new Bezier.PathPoint[s.nestedList.Length][];
                    for (int i = 0; i < s.nestedList.Length; i++)
                    {
                        InBetweenBranches[i] = s.nestedList[i].nestedList;
                    }
                }
            }
        }
    }
    [Serializable]
    public class SerializableListWrapper 
    {
        public SerializableListWrapperL2[] nestedList;
    }
    [Serializable]
    public class SerializableListWrapperL2
    {
        public Bezier.PathPoint[] nestedList;
    }
}
