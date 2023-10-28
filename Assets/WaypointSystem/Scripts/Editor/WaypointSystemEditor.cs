
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
#if UNITY_EDITOR
namespace ASWS {
    [CustomEditor(typeof(WaypointSystem))]
    public class WaypointSystemEditor : Editor
    {

        WaypointSystem self;

        public SerializedProperty GizmoType { get; private set; }

        public bool create = false, delete = false;
        int selectedSegment = -1;
        int id = -1;
        public float minDistanceToPoint = 0.5f;
        public float minDistanceToSegment = 1f;
        public Color waypointColor = Color.red;
        public Color FirstWaypointColor = Color.cyan;
        public Color NextHandleColor = new Color(0.6f, 0.9f, 0.2f, 1);
        public Color PrevHandleColor = new Color(0.6f, 0.2f, 0.9f, 1);

        public Color ControlpointColor = Color.green;
        public Color SegmentColor = Color.blue;
        public Color selectedSegmentColor = new Color(139 / 255, 128 / 255, 0, 1);
        public float handleToolSize = 1;
        public int SelectedLoop = -1;
        public WaypointLoop selectedLoop;
        public Waypoint SelectedWaypoint;
        bool selectedloopFold = false;
        private float distancePointAhead = 15f;
        private bool loopColors_open;
        private bool loop_transformEdit;
        private bool enableBranchEdit = false;
        private bool selectedWaypointdata = false;
        private bool foldLoops;
        private bool waypointListFold;
        private Vector3 lastPos;
        private bool resourcePreserve = false;

        /// <summary>
        /// select Branch
        /// </summary>
        private bool drawingWire = false;
        private float wireDuration = 1.0f; // Adjust this to set the duration in seconds
        private float startTime;
        private Waypoint SB1, SB2;
        private void OnSceneGUI()
        {
            
            if (self.transform.position != lastPos)
            {
                foreach(var loop in self.loops)
                {
                    loop.updateLoopPoints();
                }

                lastPos = self.transform.position;
            }
            /*
            if (SelectedWaypoint!=null&& SelectedWaypoint.Next!=null)
            { 
                Vector3 r = Bezier.GetCurveRadius(SelectedWaypoint.transform.position, SelectedWaypoint.HandleB.position, SelectedWaypoint.Next.HandleA.position, SelectedWaypoint.Next.transform.position, 0);
                Handles.DrawLine(SelectedWaypoint.transform.position, SelectedWaypoint.transform.position + r);
                Vector3 r3 = r;
                r3.y = 0;
                Handles.DrawWireDisc(SelectedWaypoint.transform.position + r, Vector3.up, r3.magnitude);
            }*/
            checkKeyDown(KeyCode.B, ref create);

            id = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(id);
            Ray worldRay = new Ray(Vector3.zero, Vector3.zero);

            RaycastHit hitInfo = new RaycastHit();
            bool rayhit = false;
            //update raycast data only when event happen
            if (Event.current.type == EventType.MouseDown || Event.current.shift)
            {
                worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                rayhit = Physics.Raycast(worldRay, out hitInfo);
            }

            //focus on selected
            if (Event.current.type == EventType.KeyDown && Event.current.control)
            {
                if (Event.current.keyCode == KeyCode.W && SelectedWaypoint != null)
                {

                    SceneView.lastActiveSceneView.Frame(new Bounds(SelectedWaypoint.transform.position, Vector3.one * 10), false);
                }
                else if (Event.current.keyCode == KeyCode.Q && selectedLoop != null)
                {

                    SceneView.lastActiveSceneView.Frame(new Bounds(selectedLoop.transform.position, Vector3.one * 10), false);
                }
            }

            for (int i = 0; i < self.loops.Count; i++)
            {
                DrawLoopScene(self.loops[i], false, worldRay, hitInfo);
                DrawLoopOrigin(self.loops[i], Color.black, minDistanceToPoint * 1.5f);
            }
            // create/remove new button
            if (Event.current.shift)
            {
                //create/remove new loop/waypoint
                if (Event.current.type == EventType.MouseDown && !Event.current.control)
                {
                    //create 
                    if (Event.current.button == 0)
                    {
                        //create new loop if no loop selected
                        if (selectedLoop == null) {
                            Vector3 pos;
                            if (rayhit)
                                pos = hitInfo.point;
                            else
                                pos = worldRay.origin + worldRay.direction * distancePointAhead;
                            selectedLoop = self.AddLoop(pos);
                            Debug.Log("Creating Loop: " + selectedLoop.name);
                            EditorUtility.SetDirty(self);
                        }
                    }
                    //delete loop or waypoint
                    else if (Event.current.button == 1)
                    {
                        float distance = minDistanceToPoint;
                        int index = -1;
                        //get nearest loop
                        for (int i = 0; i < self.loops.Count; i++)
                        {
                            var dist = GetDistance(worldRay, self.loops[i].transform.position);
                            if (dist < distance)
                            {
                                index = i;
                                distance = dist;
                            }
                        }

                        if (index != -1)
                        {
                            var l = self.loops[index];
                            bool del = EditorUtility.DisplayDialog("delete loop", "are you sure you want to delete " + l.name + " and it's " + l.waypoints.Count + " waypoints?", "delete", "cancel");
                            if (del) {

                                self.loops.RemoveAt(index);
                                DestroyImmediate(l.gameObject);
                                EditorUtility.SetDirty(self);
                            }
                        }

                    }
                    //connecr branch
                } else if (SelectedWaypoint != null && Event.current.control)
                {
                    if (self.curveType == ConnectionType.bezier)
                        Handles.DrawBezier(SelectedWaypoint.transform.position, worldRay.origin, SelectedWaypoint.HandleB.position, worldRay.origin + worldRay.direction, Color.magenta, null, 4f);
                    else
                        Handles.DrawLine(SelectedWaypoint.transform.position, worldRay.origin, 3f);

                    HandleUtility.Repaint();
                    if (Event.current.type == EventType.MouseDown)
                    {
                        var w = GetClosetPoint(worldRay, minDistanceToPoint, out _, out _);
                        if (w != null)
                        {

                            SelectedWaypoint.AddBranch(w);
                            //self.AddBranch(SelectedWaypoint, w);
                            EditorUtility.SetDirty(self);
                        }
                    }

                }
            }

            /*
            foreach(var Branch in self.Branches)
            {
                if (Branch.Key == null)
                {
                    self.Branches.Remove(Branch.Key);
                    continue;
                }
                if(Branch.Value!=null) 
                foreach (var val in Branch.Value) 
                {
                    var start = val.source;
                    var end = val.destination;
                    if (start == null || end == null)
                    {
                        Branch.Value.Remove(val);
                        continue;

                    }
                    Handles.DrawBezier(start.transform.position, end.transform.position, start.HandleB.position, end.HandleA.position, Color.magenta, null, 4f);
                    
                    
                }
                
            }*/
            //self.DrawBranch();


            if (drawingWire)
            {
                // Draw your wire or any other visual representation here
                Handles.color = Color.red;
                Handles.SphereHandleCap(0, SB1.transform.position, Quaternion.identity, 1, EventType.Repaint);
                Handles.SphereHandleCap(0, SB2.transform.position, Quaternion.identity, 1, EventType.Repaint);

                float elapsedTime = Time.realtimeSinceStartup - startTime;
                if (elapsedTime >= wireDuration)
                {
                    drawingWire = false;
                    SceneView.RepaintAll();
                }
            }
        }

        
        public override void OnInspectorGUI()
        {

            self = (WaypointSystem)target;
            self.autoset = EditorGUILayout.Toggle("Auto Set Waypoints", self.autoset);
            self.freeMoveHandles = EditorGUILayout.Toggle("Use Free Handle", self.freeMoveHandles);
            enableBranchEdit = EditorGUILayout.Toggle("enable Branch Edit", enableBranchEdit);
            resourcePreserve = EditorGUILayout.Toggle(new GUIContent("low CPU mode", "by enable this:\n*the segment selection is disabled\n"), resourcePreserve);



            self.curveType = (ConnectionType)EditorGUILayout.EnumPopup(self.curveType);
            DebugColors();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            foldLoops = EditorGUILayout.Foldout(foldLoops, "loops");
            GUIStyle s = EditorStyles.iconButton;

            s.fixedWidth = 120;
            if (foldLoops)
            {
                for (int i = 0; i < self.loops.Count; i++)
                {
                    if (GUILayout.Button(self.loops[i].name, s, GUILayout.Width(60)))
                    {
                        selectedLoop = self.loops[i];
                        SelectedWaypoint = null;
                        SceneView.RepaintAll();
                    }
                }
            }

            EditorGUILayout.EndVertical();


            if (GUILayout.Button("Scan Loops"))
            {
                self.ScanLoops();
            }
            if (GUILayout.Button("clear branches"))
            {
                for (int i = 0; i < self.loops.Count; i++)
                {
                    var l = self.loops[i];
                    for (int j = 0; j < l.waypoints.Count; j++)
                    {
                        var w = l.waypoints[j];
                        w.Branches = new List<Waypoint>();
                        w.enterance = false;
                        w.exit = false;
                    }
                    l.entrances = new List<Waypoint>();
                    l.exits = new List<Waypoint>();
                }
            }
            if (GUILayout.Button("Compile graph"))
            {
                self.CreateGraph();
            }


            if (selectedLoop != null)
                LoopInsprctorGUI(selectedLoop);
            if (SelectedWaypoint != null)
                WaypointInspector(SelectedWaypoint);


        }
        public void WaypointInspector(Waypoint point)
        {
            selectedWaypointdata = EditorGUILayout.Foldout(selectedWaypointdata, "selected Waypoint: " + point.name);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (selectedWaypointdata)
            {
                SerializedObject serializedObject = new SerializedObject(point);
                SerializedProperty parent = serializedObject.FindProperty("parent");
                EditorGUILayout.Vector3Field("Position :", point.transform.localPosition);
                EditorGUILayout.Vector3Field("HandleA :", point.HandleA.localPosition);
                EditorGUILayout.Vector3Field("HandleB :", point.HandleB.localPosition);

                EditorGUILayout.PropertyField(parent, new GUIContent("parent"));
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                if (point.Next)
                    if (GUILayout.Button("next:" + point.Next.name))
                    {
                        SelectedWaypoint = point.Next;
                        SceneView.RepaintAll();
                    }
                if (point.previous)
                    if (GUILayout.Button("Prev:" + point.previous.name))
                    {
                        SelectedWaypoint = point.previous;
                        SceneView.RepaintAll();
                    }
                EditorGUILayout.EndHorizontal();


                var brances = point.Branches;
                if (brances != null)
                {

                    EditorGUILayout.LabelField("Branches:");
                    foreach (var b in brances)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(b.ToString());
                        if (GUILayout.Button("check"))
                        {
                            SB1 = b;
                            SB2 = point;
                            drawingWire = true;
                            startTime = Time.realtimeSinceStartup;
                            SceneView.RepaintAll();

                        }
                        if (GUILayout.Button("delete"))
                        {
                            point.RemoveBranch(b);
                            break;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        public void OnEnable()
        {
            
            self = (WaypointSystem)target;
            GizmoType = serializedObject.FindProperty("GizmoMode");
            self.ScanLoops();
            for (int i = 0; i < self.loops.Count; i++)
            {
                self.loops[i].gameObject.hideFlags |= HideFlags.HideInHierarchy;
                self.loops[i].updateLoopPoints();
                if (!self.loops[i].Is2d)
                {
                    self.loops[i].SaveWaypointsYOffset();
                }
                //self.loops[i].gameObject.hideFlags &= ~HideFlags.HideInHierarchy;
                /*
                foreach (var point in self.loops[i].waypoints)
                { 
                    Vector3 dir = point.getForwardVec();
                    Vector3 norm = Vector3.Cross(Vector3.right, dir).normalized;
                    Vector3 proj = Vector3.ProjectOnPlane(point.normalDir, dir).normalized;
                    var q = Quaternion.AngleAxis(Vector3.SignedAngle(norm, proj, dir), dir);
                    var p = point.transform.position;
                    Debug.Log(p);
                    Debug.Log(q);
                    q = Handles.Disc(q,p, dir, 3, false, 0);

                    point.normalDir = q * norm;
                    point.OnStateChanged();
                }  
                */
            }

            

        }
        HideFlags HideFlagsButton(string aTitle, HideFlags aFlags, HideFlags aValue)
        {
            if (GUILayout.Toggle((aFlags & aValue) > 0, aTitle, "Button"))
                aFlags |= aValue;
            else
                aFlags &= ~aValue;
            return aFlags;
        }

        public void PosHandle()
        {

        }


        public void ClosestCurveinLoop(WaypointLoop loop, Ray ray)
        {
            if (loop == null) return;
            float distance = minDistanceToSegment;
            selectedSegment = -1;
            for (int i = 0; i < loop.waypoints.Count; i++)
            {
                var w1 = loop.waypoints[i];
                Waypoint w2;
                if (i == loop.waypoints.Count - 1)
                    w2 = loop.waypoints[0];
                else
                    w2 = loop.waypoints[i + 1];
                float dis = ClosestPointinCurve(w1.transform.position, w1.HandleB.position, w2.HandleA.position, w2.transform.position, ray);
                if (dis < distance)
                {
                    selectedSegment = i;
                    distance = dis;
                }

            }

            HandleUtility.Repaint();
        }

        public float ClosestPointinCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Ray ray)
        {
            float ControlLength = Vector3.Distance(p1, p2) + Vector3.Distance(p2, p3) + Vector3.Distance(p3, p4);
            float estimatedCurveLength = Vector3.Distance(p1, p4) + ControlLength / 2;
            int divitions = Mathf.CeilToInt(estimatedCurveLength);
            float dis = minDistanceToSegment;
            var pnts = Handles.MakeBezierPoints(p1, p4, p2, p3, divitions);

            for (int i = 0; i < pnts.Length - 1; i++)
            {
                Vector3 i1, i2;

                (i1, i2) = AslaMath.LineDistance(ray.origin, ray.direction, pnts[i], pnts[i + 1] - pnts[i]);

                if ((i2 - pnts[i]).magnitude < (pnts[i + 1] - pnts[i]).magnitude)
                {
                    var d = Vector3.Distance(i1, i2);

                    if (d < dis) {
                        dis = d;
                    }
                }
            }
            return dis;
        }



        public void DrawLoopScene(WaypointLoop loop, bool Selected, Ray worldRay, RaycastHit hitInfo)
        {
            //Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // RaycastHit hitInfo;
            //bool rayhit = Physics.Raycast(worldRay, out hitInfo);
            if (Event.current.control)
            {
                //selec loop
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        if (GetDistance(worldRay, loop.transform.position) < minDistanceToPoint)
                        {
                            selectedLoop = loop;
                        }


                    }
                    else if (Event.current.button == 1)
                    {
                        /*if(GetDistance(worldRay, loop.transform.position) < minDistanceToPoint){
                            self.loops.Remove(loop);
                            DestroyImmediate(loop.gameObject);
                            return;
                        }*/
                    }
                }
            }




            //selecting segment
            if (Event.current.shift && resourcePreserve == false)
            {
                ClosestCurveinLoop(selectedLoop, worldRay);
            }
            else
                selectedSegment = -1;


            if (loop == selectedLoop) {

                //GetNearestPointToSegment(loop,hitInfo.point);
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.control && !Event.current.shift)
                    {
                        if (Event.current.button == 0) {
                            int index = GetClosetPointInloop(loop, worldRay, 1);
                            if (index != -1)
                            {
                                /*if(SelectedWaypoint!=null && SelectedWaypoint.parent != loop)
                                {
                                    self.AddBranch(SelectedWaypoint, loop.waypoints[index]);
                                }*/
                                SelectedWaypoint = loop.waypoints[index];
                            }
                        }
                        else if (Event.current.button == 1)
                        {
                            SelectedWaypoint = null;
                            selectedLoop = null;
                        }
                    }
                    Vector3 hitPoint = hitInfo.point;
                    if (hitInfo.collider == null)
                        hitPoint = worldRay.origin + worldRay.direction * distancePointAhead;


                    if (loop.Is2d)
                    {
                        Vector3 dir = loop.transform.InverseTransformDirection(worldRay.direction);
                        Vector3 point = loop.transform.InverseTransformPoint(worldRay.origin);
                        dir = dir * point.y / dir.y;
                        point = point - dir;
                        hitPoint = loop.transform.TransformPoint(point);
                    }


                    if (Event.current.shift && !Event.current.control)
                        if (Event.current.button == 0)
                        {

                            if (selectedSegment != -1)
                            {

                                SelectedWaypoint = loop.AddWaypoint(hitPoint, selectedSegment);
                                EditorUtility.SetDirty(self);
                                if (self.autoset || loop.autoSet)
                                    loop.automaticSetup();
                                self.GraphUpdated = false;
                            }
                            else
                            {
                                SelectedWaypoint = loop.AddWaypointAttEnd(hitPoint);
                                self.GraphUpdated = false;
                                EditorUtility.SetDirty(self);
                                self.GraphUpdated = false;
                            }
                        }
                        else if (Event.current.button == 1)
                        {

                            int index = GetClosetPointInloop(loop, worldRay, 1);
                            if (index != -1)
                            {
                                Waypoint w = loop.waypoints[index];//= loop.GetClosestWaypoint(hitInfo.point, minDistanceToPoint);
                                if (w != null)
                                    EditorUtility.SetDirty(self);
                                loop.RemovePoint(w);
                                self.GraphUpdated = false;
                            }

                        }


                }
            }
            if (loop != selectedLoop && resourcePreserve) return;
            {
                DrawLoopWaypoints(loop);
                if (self.curveType == ConnectionType.bezier)
                    DrawBezier(loop);
                else
                    DrawLines(loop);
            }


        }

        private void OnDisable()
        {

        }

        public void SelectLoop(Ray r)
        {

        }

        public void checkKeyDown(KeyCode key, ref bool value)
        {
            Event e = Event.current;
            // If statements are left separate in case
            // you intend to utilize more key/mouse buttons
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == key)
                {

                    value = true;
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == key)
                {
                    value = false;
                }
            }

        }
        public void GetNearestPointToSegment(WaypointLoop loop, Vector3 position)
        {
            if (Event.current.type == EventType.MouseMove)
            {
                int newselectedSeg = -1;
                float minDestToSeg = minDistanceToSegment;
                for (int i = 0; i < loop.waypoints.Count - 1; i++)
                {
                    int next = (i + 1);
                    float dst = HandleUtility.DistancePointBezier(position, loop.waypoints[i].transform.position, loop.waypoints[next].transform.position, loop.waypoints[i].HandleB.position, loop.waypoints[next].HandleA.position);
                    if (dst < minDestToSeg)
                    {
                        minDestToSeg = dst;
                        newselectedSeg = i;
                    }

                }
                if (loop.isClosedLoop)
                {
                    float dst = HandleUtility.DistancePointBezier(position, loop.waypoints[loop.waypoints.Count - 1].transform.position, loop.waypoints[0].transform.position, loop.waypoints[loop.waypoints.Count - 1].HandleB.position, loop.waypoints[0].HandleA.position);
                    if (dst < minDestToSeg)
                    {
                        minDestToSeg = dst;
                        newselectedSeg = loop.waypoints.Count - 1;
                    }
                }
                if (newselectedSeg != selectedSegment)
                {
                    selectedSegment = newselectedSeg;
                    HandleUtility.Repaint();
                }
            }

        }

        public void DrawLoopWaypoints(WaypointLoop loop)
        {
            if (loop.waypoints != null && loop.waypoints.Count > 0)
            {
                DrawWiredSphereHandle(loop.waypoints[0].transform.position, minDistanceToPoint + 0.1f, FirstWaypointColor);
                for (int i = 0; i < loop.waypoints.Count; i++)
                {
                    var point = loop.waypoints[i];

                    DrawPoint(point, waypointColor, minDistanceToPoint, loop.Is2d, loop.transform.position.y);

                }
            }
        }
        public void DrawLoopOrigin(WaypointLoop loop, Color color, float size = 0.5f)
        {



            Handles.color = color;

            Vector3 pos = Handles.FreeMoveHandle(loop.transform.position, Quaternion.identity, size, Vector3.zero, Handles.SphereHandleCap);
            if (loop == selectedLoop)
                pos = DrawCustomArrowHandleCap(loop.transform, size * 3 * handleToolSize);

            if (loop.transform.position != pos)
            {
                loop.transform.position = pos;
                EditorUtility.SetDirty(self);
                loop.updateLoopPoints();
                //Undo.RecordObject(self, "move Point");
                Undo.RegisterChildrenOrderUndo(self, "move loop");

            }
        }

        public Vector3 DrawCustomArrowHandleCap(Transform transform, float size)
        {

            return DrawCustomArrowHandleCap(transform.position, transform, size);
        }
        public Vector3 DrawCustomArrowHandleCap(Vector3 position, Transform transform, float size)
        {


            Handles.color = Handles.xAxisColor;
            Vector3 xPos = Handles.Slider(position, transform.right, size, Handles.ArrowHandleCap, 0);
            if (xPos.x != position.x)
                position.x = xPos.x;

            Vector3 offsetx = new Vector3(0, size * 0.1f, size * 0.1f);
            Vector3 PUposx = Handles.Slider2D(position + offsetx, transform.right, transform.up, transform.forward, size * 0.1f, Handles.RectangleHandleCap, 0);
            if (PUposx != position + offsetx)
                position = PUposx - offsetx;


            Handles.color = Handles.yAxisColor;
            Vector3 yPos = Handles.Slider(position, transform.up, size, Handles.ArrowHandleCap, 0);
            if (yPos.y != position.y)
                position.y = yPos.y;


            Vector3 offsety = new Vector3(size * 0.1f, 0, size * 0.1f);
            Vector3 PUposy = Handles.Slider2D(position + offsety, transform.up, transform.forward, transform.right, size * 0.1f, Handles.RectangleHandleCap, 0);
            if (PUposy != position + offsety)
                position = PUposy - offsety;


            Handles.color = Handles.zAxisColor;
            Vector3 zPos = Handles.Slider(position, transform.forward, size, Handles.ArrowHandleCap, 0);
            if (zPos.z != position.z)
                position.z = zPos.z;


            Vector3 offsetz = new Vector3(size * 0.1f, size * 0.1f, 0);
            Vector3 PUposz = Handles.Slider2D(position + offsetz, transform.forward, transform.right, transform.up, size * 0.1f, Handles.RectangleHandleCap, 0);
            if (PUposz != position + offsetz)
                position = PUposz - offsetz;

            return position;
        }


        
        public void DrawPoint(Waypoint point, Color color, float size = 0.5f,bool flat = false,float yOffset=0)
        {


        
            Vector3 pos = point.transform.position;
            if(self.freeMoveHandles)
                pos = Handles.FreeMoveHandle(point.transform.position, Quaternion.identity, size, Vector3.zero, Handles.SphereHandleCap);
            else
                DrawWiredSphereHandle(pos,size, color);
            
            if (SelectedWaypoint == point)
            {

                
                if (Tools.current == Tool.Move)
                    pos = DrawCustomArrowHandleCap(point.transform, size * 3* handleToolSize);
                else if (Tools.current == Tool.Rotate)
                {
                    Vector3 dir = point.getForwardVec();
                    Vector3 norm = Vector3.Cross( Vector3.right, dir).normalized;
                    Vector3 proj = Vector3.ProjectOnPlane(point.normalDir, dir).normalized;

                    var q = Quaternion.AngleAxis(Vector3.SignedAngle(norm,proj,dir),dir);
                    
                    var newQ = Handles.Disc(q, pos, dir, 3, false, 0);
                    if (q.eulerAngles != newQ.eulerAngles) { 
                        point.normalDir = newQ * norm;
                        q = newQ;
                        point.OnStateChanged();
                        self.OnSystemchanged();
                        EditorUtility.SetDirty(point);
                        Undo.RegisterChildrenOrderUndo(self, "move Waypoint");

                    }

                }

               
            }
            Handles.color = Color.cyan;
            Handles.DrawLine(pos, pos + point.normalDir * 3);
            if (point.transform.position != pos)
            {
                
                if (flat) pos.y = yOffset; 
                point.transform.position = pos;
                point.OnStateChanged();
                self.OnSystemchanged();
                EditorUtility.SetDirty(point);
                Undo.RegisterChildrenOrderUndo(self, "move Waypoint");

            }
            if (point.Branches!=null)
                for(int i = 0; i < point.Branches.Count; i++)
                {
                    if (point.Branches[i] == null) point.Branches.RemoveAt(i);
                    else
                    {
                        Handles.color = Color.blue;
                        Handles.DrawWireCube(point.transform.position, Vector3.one );
                        Handles.color = Color.magenta;
                        if (self.curveType == ConnectionType.bezier)
                        {
                            Handles.DrawBezier(point.transform.position, point.Branches[i].transform.position, point.HandleB.position, point.Branches[i].HandleA.position, Color.magenta, null, 4f);
                            
                        }
                        else
                            Handles.DrawLine(point.transform.position, point.Branches[i].transform.position, 3f);
                    }
                }


            //handle A
            Handles.color = NextHandleColor;
            Vector3 handleA = point.HandleA.transform.position;
            if (self.freeMoveHandles)
                handleA = Handles.FreeMoveHandle(point.HandleA.transform.position, Quaternion.identity, size * 0.7f, Vector3.zero, Handles.SphereHandleCap);
            else
                DrawWiredSphereHandle(handleA, size*0.7f, NextHandleColor);
            if (SelectedWaypoint == point)
                handleA = DrawCustomArrowHandleCap(point.HandleA.transform, size*3* handleToolSize);
            if (point.HandleA.transform.position != handleA)
            {
                
                if (flat) handleA.y = yOffset;
                point.HandleA.transform.position = handleA;
                point.UpdateHandle( false);
                Undo.RegisterChildrenOrderUndo(self, "move handleA");

                Vector3 dir = point.getForwardVec();
                var norm = Vector3.Cross(Vector3.right, dir).normalized;
                
                point.OnStateChanged();
                self.OnSystemchanged();
                EditorUtility.SetDirty(point);

            }
            Handles.DrawLine(point.transform.position, point.HandleA.transform.position);

            //handle B
            Handles.color = new Color(0.5f, 0.1f, 0.9f, 1);
            Vector3 handleB = point.HandleB.transform.position;
            if (self.freeMoveHandles)
                handleB = Handles.FreeMoveHandle(point.HandleB.transform.position, Quaternion.identity, size * 0.7f, Vector3.zero, Handles.SphereHandleCap);
            else
                DrawWiredSphereHandle(handleB, size*0.7f, PrevHandleColor);
            if (SelectedWaypoint == point)
                handleB = DrawCustomArrowHandleCap(point.HandleB.transform, size*3* handleToolSize  );
            if (point.HandleB.transform.position != handleB)
            {
                if (flat) handleB.y = yOffset;
                point.HandleB.transform.position = handleB;
                point.UpdateHandle( true);
                Undo.RegisterChildrenOrderUndo(self, "move handleB");

                Vector3 dir = point.getForwardVec();
                var norm = Vector3.Cross(Vector3.right, dir).normalized;
                point.OnStateChanged();
                self.OnSystemchanged();
                EditorUtility.SetDirty(point);

            }
            Handles.DrawLine(point.transform.position, point.HandleB.transform.position);


            //if it's handle



        }

        public void DrawWiredSphereHandle(Vector3 position , float size,Color color)
        {
            Handles.color = color;
            Handles.DrawWireDisc(position, Vector3.up, size);
            Handles.DrawWireDisc(position, Vector3.forward, size);
            Handles.DrawWireDisc(position, Vector3.right, size);
        }

        public void DrawLines(WaypointLoop loop)
        {

            if (loop.waypoints.Count > 1)
            {
                for (int i = 0; i < loop.waypoints.Count - 1; i++)
                {
                    var point1 = loop.waypoints[i];

                    var point2 = loop.waypoints[(i + 1) % loop.waypoints.Count];
                    Color c = (selectedSegment == i && loop == selectedLoop) ? selectedSegmentColor : SegmentColor;
                    Handles.color = c;
                    Handles.DrawLine(point1.transform.position, point2.transform.position, 3f);
                }
                if (loop.isClosedLoop)
                {
                    Color c = (selectedSegment == loop.waypoints.Count - 1) ? selectedSegmentColor : SegmentColor;
                    Handles.color = c;
                    Handles.DrawLine(loop.waypoints[0].transform.position, loop.waypoints[loop.waypoints.Count - 1].transform.position, 3f);

                }

            }
        }
        public void DrawBezier(WaypointLoop loop)
        {

            if (loop.waypoints.Count > 1)
            {
                for (int i = 0; i < loop.waypoints.Count - 1; i++)
                {
                    var point1 = loop.waypoints[i];

                    var point2 = loop.waypoints[(i + 1) % loop.waypoints.Count];
                    Color c = (selectedSegment == i && loop==selectedLoop) ? selectedSegmentColor : SegmentColor;
                    
                    Handles.DrawBezier(point1.transform.position, point2.transform.position, point1.HandleB.position, point2.HandleA.position, c, null, 5f);
                }
                if (loop.isClosedLoop)
                {
                    Color c = (selectedSegment == loop.waypoints.Count - 1) ? selectedSegmentColor : SegmentColor;
                    Handles.DrawBezier(loop.waypoints[0].transform.position, loop.waypoints[loop.waypoints.Count - 1].transform.position, loop.waypoints[0].HandleA.position, loop.waypoints[loop.waypoints.Count - 1].HandleB.position, c, null, 5f);

                }

            }
        }

        public void DebugColors()
        {

            loopColors_open = EditorGUILayout.Foldout(loopColors_open, "Debug Parameters");
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (loopColors_open)
            {
                EditorGUI.BeginChangeCheck();
                waypointColor = EditorGUILayout.ColorField("waypoint Color", waypointColor);
                FirstWaypointColor = EditorGUILayout.ColorField("First waypoint Color", FirstWaypointColor);

                NextHandleColor = EditorGUILayout.ColorField("next Handle Color", NextHandleColor);

                PrevHandleColor = EditorGUILayout.ColorField("previous handle Color", PrevHandleColor);
                minDistanceToPoint = EditorGUILayout.Slider("waypoint size", minDistanceToPoint, 0, 2);
                handleToolSize = EditorGUILayout.Slider("waypoint size", handleToolSize, 1, 5);
                ControlpointColor = EditorGUILayout.ColorField("Control point Color", ControlpointColor);
                SegmentColor = EditorGUILayout.ColorField("Segment Color", SegmentColor);

                selectedSegmentColor = EditorGUILayout.ColorField("selected Segment Color", selectedSegmentColor);


                serializedObject.Update();
                //update gizmo type
                EditorGUILayout.PropertyField(GizmoType, new GUIContent("Int Variable"));
                string[] options = new string[] { "no Gizmo", "only point", "only line","with normal" };
                int selectedIndex = Mathf.Clamp(GizmoType.intValue, 0, options.Length - 1);
                selectedIndex = EditorGUILayout.Popup("Gizmo Type", selectedIndex, options);

                
                
                if (GizmoType.intValue != selectedIndex)
                {
                    GizmoType.intValue = selectedIndex;

                    serializedObject.ApplyModifiedProperties();
                    self.updateGizmoType();
                }


                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
                minDistanceToSegment = EditorGUILayout.Slider("select segment distance", minDistanceToSegment, 0, 1);

            }
            EditorGUILayout.EndVertical();
        }
        public void LoopInsprctorGUI(WaypointLoop loop)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            selectedloopFold = EditorGUILayout.Foldout(selectedloopFold,"Selected Loop: " + loop.name);
            EditorGUI.indentLevel++;
            if (selectedloopFold)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                loop_transformEdit = EditorGUILayout.Foldout(loop_transformEdit, "Edit Transform");
                if (loop_transformEdit)
                {
                    loop.transform.localPosition = EditorGUILayout.Vector3Field("Position", loop.transform.localPosition);
                    loop.transform.localEulerAngles = EditorGUILayout.Vector3Field("Rotation", loop.transform.localEulerAngles);
                    loop.transform.localScale = EditorGUILayout.Vector3Field("Scale", loop.transform.localScale);
                }

                waypointListFold = EditorGUILayout.Foldout(waypointListFold, "Waypoints");
                
                if (waypointListFold)
                {
                    var s = EditorStyles.iconButton;
                    s.fixedWidth = 120;
                    
                    for (int i = 0; i < loop.waypoints.Count; i++)
                    {var r = EditorGUILayout.GetControlRect();
                        r.x+= (EditorGUI.indentLevel+1)*10;
                        if (GUI.Button(r,loop.waypoints[i].name , s))
                        {
                            SelectedWaypoint = loop.waypoints[i];
                            
                        }
                    }

                    
                }

                EditorGUILayout.EndVertical();
                loop.IsClosedLoop = EditorGUILayout.Toggle("Closed",loop.IsClosedLoop);
                loop.autoSet = EditorGUILayout.Toggle("AutoSet Control point",loop.autoSet);
                EditorGUI.BeginChangeCheck();
                bool temp2d = EditorGUILayout.Toggle("is 2d",loop.Is2d);
                if (EditorGUI.EndChangeCheck())
                {
                    loop.Is2d = temp2d;
                    
                }
                
                /*
                loopColors_open = EditorGUILayout.Foldout(loopColors_open, "Loop Colors");
                if (loopColors_open) { 
                EditorGUI.BeginChangeCheck();
                waypointColor = EditorGUILayout.ColorField("waypoint Color", waypointColor);
                    FirstWaypointColor = EditorGUILayout.ColorField("First waypoint Color", FirstWaypointColor);

                    NextHandleColor = EditorGUILayout.ColorField("next Handle Color", NextHandleColor);

                    PrevHandleColor = EditorGUILayout.ColorField("previous handle Color", PrevHandleColor);
                    minDistanceToPoint = EditorGUILayout.Slider("waypoint size", minDistanceToPoint, 0, 2);
                ControlpointColor = EditorGUILayout.ColorField("Control point Color", ControlpointColor);
                SegmentColor = EditorGUILayout.ColorField("Segment Color", SegmentColor);

                selectedSegmentColor = EditorGUILayout.ColorField("selected Segment Color", selectedSegmentColor);
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                } 
                minDistanceToSegment = EditorGUILayout.Slider("select segment distance", minDistanceToSegment, 0, 1);
                }*/
                
                
                
                if (GUILayout.Button(new GUIContent("Scan waypoints","Get all children waypoints in hirarchey")))
                {
                    self.GraphUpdated = false;
                    loop.ScanLoop();
                    EditorUtility.SetDirty(self);
                }
                if (GUILayout.Button(new GUIContent("remove waypoints","delete all children waypoints from hirarchey")))
                {
                    ClearWaypointsInLoop(loop);
                    EditorUtility.SetDirty(self);
                }
                if (GUILayout.Button(new GUIContent("Auto Setup","recalculate the handles position for more smooth path")))
                {
                    loop.automaticSetup();
                }
                if (GUILayout.Button(new GUIContent("reposition Loop","put the position of the loop in the center of loop")))
                {
                    loop.RepositionLoopOrigin();
                    EditorUtility.SetDirty(self);
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }


        public void ClearWaypointsInLoop(WaypointLoop loop)
        {
            int count = loop.waypoints.Count;
            for (int i = 0; i < count; i++)
            {
                DestroyImmediate(loop.waypoints[0].gameObject);
                loop.waypoints.RemoveAt(0);
            }
        }
        public float GetDistance(Ray worldRay, Vector3 point)
        {
            var proj = Vector3.Project(point - worldRay.origin, worldRay.direction);
            var lastpoint = worldRay.origin + proj;
            return Vector3.Distance(lastpoint, point);
        }


        public Waypoint GetClosetPoint(Ray ray, float distanceSnap, out WaypointLoop loop, out int index)
        {
            index = -1;
            loop = null;
            int indexinloop = -1;
            Waypoint point = null;
            float distance = distanceSnap;
            for (int i = 0; i < self.loops.Count; i++)
            {
                
                    GetClosetPointInloop(self.loops[i], ray, distanceSnap, ref point, ref distance, out indexinloop);
                    if (distance < distanceSnap)
                    {
                        loop = self.loops[i];
                        index = indexinloop;
                        distanceSnap = distance;
                    }
                
            }
            return point;
        }


        public static (Vector3 , Vector3) LineDistance(Vector3 p1,Vector3 dir1,Vector3 p2,Vector3 dir2)
        {
            dir2 = dir2.normalized;
            dir1 = dir1.normalized;

            // i1 = (p1 + n *dir1.x)
            // i2 = (p2 + m *dir2.x)
            
            
            
            //dir1.x *( (p2.x + n *dir2.x)-(p1.x + m *dir1.x) ) +dir1.y *( (p2.y + n *dir2.y)-(p1.y + m *dir1.y) ) +dir1.z *( (p2.z + n *dir2.z)-(p1.z + m *dir1.z) ) = 0
            //dir2.x *( (p2.x + n *dir2.x)-(p1.x + m *dir1.x) ) +dir2.y *( (p2.y + n *dir2.y)-(p1.y + m *dir1.y) ) +dir2.z *( (p2.z + n *dir2.z)-(p1.z + m *dir1.z) ) = 0 
            // m=? , n=?

            //     eq1
            // n * (dir1.x*dir2.x  + dir1.y*dir2.y + dir1.z*dir2.y) - m *(dir1.x*dir1.x + dir1.y*dir1.y + dir1.z*dir1.z) + dot(dir1 , p2-p1 ) = 0
            //dir1.x (p2.x-p1.x) +  dir1.y (p2.y-p1.y) +  dir1.y (p2.y-p1.y) = dot(dir1 , p2-p1 )
            //(dir1.x*dir1.x + dir1.y*dir1.y + dir1.z*dir1.z) = 1
            //(dir1.x*dir2.x  + dir1.y*dir2.y + dir1.z*dir2.y) = dot(dir1,dir2)
            // n * dot(dir1,dir2) - m + dot(dir1 , p2-p1 ) = 0


            //     eq2
            // n * (dir2.x*dir2.x  + dir2.y*dir2.y + dir2.z*dir2.y) - m *(dir2.x*dir1.x + dir2.y*dir1.y + dir2.z*dir1.z) + dot(dir2 , p2-p1 ) = 0
            //dir1.x (p2.x-p1.x) +  dir1.y (p2.y-p1.y) +  dir1.y (p2.y-p1.y) = dot(dir1 , p2-p1 )
            //(dir1.x*dir1.x + dir1.y*dir1.y + dir1.z*dir1.z) = 1
            //(dir1.x*dir2.x  + dir1.y*dir2.y + dir1.z*dir2.y) = dot(dir1,dir2)
            // n  - m *dot(dir1,dir2)+ dot(dir2 , p2-p1 ) = 0

            // eq1 :  n - m /dot(dir1,dir2) + dot(dir1 , p2-p1 )/dot(dir1,dir2) = 0
            // eq2 :  n - m *dot(dir1,dir2) + dot(dir2 , p2-p1 ) = 0


            //s1: -m * ( dot(dir1,dir2)+ 1/ dot(dir1,dir2) ) + dot(dir2 , p2-p1 ) - dot(dir1 , p2-p1 )/dot(dir1,dir2) = 0
            //s1: m = ( dot(dir2 , p2-p1 ) - dot(dir1 , p2-p1 )/dot(dir1,dir2) ) / ( dot(dir1,dir2)+ 1/ dot(dir1,dir2) ) ;
            //s2: n =  m *dot(dir1,dir2) - dot(dir2 , p2-p1 )
            var deltap = p2 - p1;
            var deltaDir = dir1 - dir2;
            var DirDot = Vector3.Dot(dir1, dir2);
            var dir1delta = Vector3.Dot(dir1, deltap);
            var dir2delta = Vector3.Dot(dir2, deltap);

            var m = (dir2delta - dir1delta / DirDot) - dir1delta / DirDot;
            var n = m * DirDot - dir2delta;

            var i1 = p1 + m * dir1;
            var i2 = p2 + n * dir2;
            return (i1, i2);
            //( (p2.x + n *dir2.x)-(p1.x + m *dir1.x) ) *(dir1.x - dir2.x) +( (p2.y + n *dir2.y)-(p1.y + m *dir1.y) ) *(dir1.y - dir2.y) +( (p2.z + n *dir2.z)-(p1.z + m *dir1.z) ) *(dir1.z -dir2.z) = 0

            //( (p2.x + n *dir2.x)-(p1.x + m *dir1.x) ) *deltaDir.x +( (p2.y + n *dir2.y)-(p1.y + m *dir1.y) ) *deltaDir.y +( (p2.z + n *dir2.z)-(p1.z + m *dir1.z) ) *deltaDir.z = 0

        }

        
       
        public void GetClosetPointInloop(WaypointLoop loop, Ray ray, float distanceSnap, ref Waypoint point, ref float distance, out int index)
        {
            index = -1;
            for (int i = 0; i < loop.waypoints.Count; i++)
            {
                float d = GetDistance(ray, loop.waypoints[i].transform.position);
                if (d < distanceSnap)
                {
                    distanceSnap = d;
                    point = loop.waypoints[i];
                    distance = distanceSnap;
                    index = i;
                }
            }

        }
        public int GetClosetPointInloop(WaypointLoop loop, Ray ray, float distanceSnap)
        {
            int index = -1;
            for (int i = 0; i < loop.waypoints.Count; i++)
            {
                float d = GetDistance(ray, loop.waypoints[i].transform.position);
                if (d < distanceSnap)
                {
                    distanceSnap = d;
                    
                    index = i;
                }
            }
            return index;
        }
    }
    }
#endif