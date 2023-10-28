using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace ASWS { 
    public class CreateWaypointSystemWindow : EditorWindow
    {

        [MenuItem("Window/Create Waypoint System")]
        public static void ShowWindow()
        {
            GameObject waypointObject = new GameObject("WaypointSystem");
            waypointObject.AddComponent<WaypointSystem>();
        }
        

    }
}
