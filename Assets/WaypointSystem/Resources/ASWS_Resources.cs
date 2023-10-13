using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace ASWS
{
    [FilePath("Assets/WaypointSystem/Resources/ASWS_Resources.foo", FilePathAttribute.Location.PreferencesFolder)]
    [CreateAssetMenu(menuName="ASWS/Resources")]
    public class ASWS_Resources : ScriptableSingleton<ASWS_Resources>
    {
        
        public Mesh ArrowMesh;
    }

}
#endif