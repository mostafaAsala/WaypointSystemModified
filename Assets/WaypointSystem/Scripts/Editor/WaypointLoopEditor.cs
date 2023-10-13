using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


#if UNITY_EDITOR
namespace ASWS
{
    
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WaypointLoop))]
    public class WaypointLoopEditor : Editor
    {
        WaypointLoop self;
        public bool CTRLPressed = false;
        bool create;
        bool delete;
        public bool autoset = false;
        int selectedSegment = -1;

        public float minDistanceToPoint = 1f;
        public float minDistanceToSegment = 0.1f;
        public Color waypointColor = Color.red;
        
        public Color ControlpointColor = Color.green;
        public Color SegmentColor = Color.blue;
        public Color selectedSegmentColor =new Color(139/255, 128/255, 0,1);
        
        

        private void OnSceneGUI()
        {
            SceneDraw();

        }

        public void SceneDraw()
        {
            int id = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(id);


            checkKeyDown(KeyCode.Keypad0, ref create);
            checkKeyDown(KeyCode.Keypad1, ref delete);
            Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hitInfo;
            bool rayhit = Physics.Raycast(worldRay, out hitInfo);
            GetNearestPointToSegment(hitInfo.point);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {


                if (rayhit)
                {
                    if (create)
                    {
                        if (selectedSegment != -1)
                        {
                            self.AddWaypoint(hitInfo.point, selectedSegment);
                            if (autoset)
                                self.automaticSetup();
                        }
                        else
                            self.AddWaypointAttEnd(hitInfo.point);
                        
                    }
                    else if (delete)
                    {

                        Waypoint w = self.GetClosestWaypoint(hitInfo.point, minDistanceToPoint);
                        if (w != null)
                            self.RemovePoint(w);
                    }
                }

            }
            DrawWaypoint();
            DrawBezier();

        }
        public void DrawBezier()
        {
            
            if (self.waypoints.Count > 1)
            {
                for (int i = 0; i < self.waypoints.Count-1 ; i++)
                {
                    var point1 = self.waypoints[i];

                    var point2 = self.waypoints[(i + 1)% self.waypoints.Count];
                    Color c = (selectedSegment == i ) ? selectedSegmentColor : SegmentColor;
                    Handles.DrawBezier(point1.transform.position, point2.transform.position, point1.HandleB.position, point2.HandleA.position, c, null, 5f);
                }
                if (self.isClosedLoop)
                {
                    Color c = (selectedSegment == self.waypoints.Count - 1) ? selectedSegmentColor : SegmentColor;
                    Handles.DrawBezier(self.waypoints[0].transform.position, self.waypoints[self.waypoints.Count-1].transform.position, self.waypoints[0].HandleA.position, self.waypoints[self.waypoints.Count - 1].HandleB.position, c, null, 5f);

                }

            }
        }

        public void DrawWaypoint()
        {
            for (int i = 0; i < self.waypoints.Count; i++)
            {
                var point = self.waypoints[i];
                DrawPoint(point.transform, waypointColor, minDistanceToPoint);
                
                if(point.HandleA)
                    DrawPoint(point.HandleA.transform, ControlpointColor, minDistanceToPoint *0.7f, point,false);
                if(point.HandleB)
                DrawPoint(point.HandleB.transform, ControlpointColor, minDistanceToPoint*0.7f, point,true);
                
                
            }
        }

        public void DrawPoint(Transform point, Color color, float size = 0.5f, Waypoint parent = null, bool handleB = false)
        {
            Handles.color = color;
            Vector3 pos = Handles.FreeMoveHandle(point.position, Quaternion.identity, size, Vector3.zero, Handles.SphereHandleCap);
            if (point.transform.position != pos)
            {
                Undo.RecordObject(point, "move Point");
                point.transform.position = pos;

                if (parent)
                {
                    //var otherpoint = parent.HandleA.transform.position == point.transform.position ? parent.HandleB : parent.HandleA;
                    parent.UpdateHandle(handleB);

                }

            }
            //if it's handle
            if (parent)
            {
                Handles.DrawLine(point.position, parent.transform.position);

            }
        }

        public void GetNearestPointToSegment(Vector3 position)
        {
            if(Event.current.type == EventType.MouseMove)
            {
                int newselectedSeg = -1;
                float minDestToSeg = minDistanceToSegment;
                for (int i = 0; i < self.waypoints.Count-1; i++)
                {
                    int next = (i + 1) ;
                    float dst = HandleUtility.DistancePointBezier(position, self.waypoints[i].transform.position, self.waypoints[next].transform.position, self.waypoints[i].HandleB.position, self.waypoints[next].HandleA.position);
                    if(dst< minDestToSeg)
                    {
                        minDestToSeg = dst;
                        newselectedSeg = i;
                    }

                }
                if (self.isClosedLoop)
                {
                    float dst = HandleUtility.DistancePointBezier(position, self.waypoints[self.waypoints.Count-1].transform.position, self.waypoints[0].transform.position, self.waypoints[self.waypoints.Count-1].HandleB.position, self.waypoints[0].HandleA.position);
                    if (dst < minDestToSeg)
                    {
                        minDestToSeg = dst;
                        newselectedSeg = self.waypoints.Count-1;
                    }
                }
                if (newselectedSeg != selectedSegment)
                {
                    selectedSegment = newselectedSeg;
                    HandleUtility.Repaint();
                }
            }

        }
        
        private void OnEnable()
        {
            self = (WaypointLoop)target;
            if(self.waypoints==null)
                self.waypoints = new List<Waypoint>();
                

        }
        public void checkKeyDown(KeyCode key,ref bool value)
        {
            Event e = Event.current;
           
            // If statements are left separate in case
            // you intend to utilize more key/mouse buttons
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == key)
                {
                    value =  true;
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == key)
                {
                    value =  false;
                }
            }
       
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            self.IsClosedLoop = GUILayout.Toggle(self.IsClosedLoop, "Closed");
            autoset = GUILayout.Toggle(autoset, "AutoSet Control point");
            EditorGUI.BeginChangeCheck();

            waypointColor = EditorGUILayout.ColorField("waypoint Color", waypointColor);
            minDistanceToPoint =  EditorGUILayout.Slider("waypoint size", minDistanceToPoint, 0, 2);
            ControlpointColor = EditorGUILayout.ColorField("Control point Color", ControlpointColor);
            SegmentColor = EditorGUILayout.ColorField("Segment Color", SegmentColor);
            selectedSegmentColor = EditorGUILayout.ColorField("selected Segment Color", selectedSegmentColor);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
            minDistanceToSegment = EditorGUILayout.Slider("select segment distance", minDistanceToSegment, 0, 1);

            if (GUILayout.Button("Add Waypoint At tEnd"))
            {
                self.AddWaypointAttEnd(self.transform.position);
            }
            if (GUILayout.Button("Scan waypoints"))
            {
                self.waypoints = new List<Waypoint>(self.GetComponentsInChildren<Waypoint>());
            }
            if (GUILayout.Button("remove waypoints"))
            {
                ClearWaypoints();
            }
            if (GUILayout.Button("Toggle Loop"))
            {
                self.toggleLoop();
            }
            if (GUILayout.Button("Auto Setup"))
            {
                self.automaticSetup();
            }
            if (GUILayout.Button("reposition Loop"))
            {
                self.RepositionLoopOrigin();
            }

            

        }
        

       
       
        public void ClearWaypoints()
        {
            int count = self.waypoints.Count;
            for (int i = 0; i < count; i++)
            {
                DestroyImmediate(self.waypoints[0].gameObject);
                self.waypoints.RemoveAt(0);
            }
        }

    } 
}

#endif