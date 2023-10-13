using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
namespace ASWS
{   [CustomEditor(typeof(Waypoint))]
    public class WaypointEditor : Editor
    {
        private Waypoint self;
        public override void OnInspectorGUI()
        {
            self = (Waypoint)target;
            base.OnInspectorGUI();
            GUI.enabled = false;
            EditorGUILayout.FloatField("id", self.id);
            GUI.enabled = true;
            Waypoint.gizmoSize = EditorGUILayout.Slider(Waypoint.gizmoSize, 0, 1);
            if(GUILayout.Button("Recalculate id"))
            {
                self.RecalculateID();
            }
            if(GUILayout.Button("Lock Handle"))
            {
                LockHandle();
            }
            

                //self.transform.eulerAngles = new Vector3(self.transform.eulerAngles.x, self.angle, self.transform.eulerAngles.z);
        }


        public void LockHandle()
        {
            self.LockHandles = true;

        }

        
    }  
}
#endif
