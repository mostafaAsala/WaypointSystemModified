using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace ASWS { 

    public class WaypointLoop : MonoBehaviour
    {
        [Tooltip("unique ID for the loop based on position")]
        private int _id;
        [Tooltip("waypoints that construct the loop")]
        public List<Waypoint> waypoints;
        [Tooltip("the waypoint system that contains the loop")]
        public WaypointSystem parent;
        [HideInInspector,Tooltip("link the last point to the start point")]
        public bool isClosedLoop;
        [Tooltip("convert between 2d flat loop or 3d loop\n falt loop has all points in the same plane can be rotated by rotating the loop")]
        private bool is2d = false;
        [Tooltip("mark the branch end waypoints as an entrance to the loop")]
        public List<Waypoint> entrances;
        [Tooltip("mark the branch source waypoints as an exit to the loop")]
        public List<Waypoint> exits;
        public bool autoSet = false;
        /// <summary>
        /// switch between 2d and 3d loops
        /// </summary>
        public bool Is2d
        {
            get { return is2d; }
            set
            {

                if (is2d == value) return;
                is2d = value;
                for (int i = 0; i < waypoints.Count; i++)
                {
                    waypoints[i].SetFlat(value);
                    
                }
                updateLoopPoints();
            }

        }
        /// <summary>
        /// save offsets of the flat plane position
        /// </summary>
        public void SaveWaypointsYOffset()
        {
            for(int i = 0; i < waypoints.Count; i++)
            {
                waypoints[i].saveYOffset();
            }
        }

        /// <summary>
        /// link the last point to the start point
        /// </summary>
        public bool IsClosedLoop
        {
            get
            {
                return isClosedLoop;
            }
            set
            {
                if (isClosedLoop != value)
                {
                    isClosedLoop = value;
                    var last = waypoints.Count - 1;
                    if (isClosedLoop)
                    {
                        waypoints[0].CreateHandles();
                        waypoints[last].CreateHandles();
                        waypoints[0].UpdateHandle( true);
                        waypoints[last].UpdateHandle( false);
                        waypoints[last].HandleB.position = waypoints[last].transform.position * 2 - waypoints[last].HandleA.position;
                        waypoints[0].HandleA.position = waypoints[0].transform.position * 2 - waypoints[0].HandleB.transform.position;
                        waypoints[last].Next = waypoints[0];
                        waypoints[0].previous = waypoints[last];

                    }
                    else
                    {
                        
                        waypoints[last].Next = null;
                        waypoints[0].previous = null;
                    }
                }
            }
        }

        public int id { get { return _id; } }
#if UNITY_EDITOR
       

#endif
        /// <summary>
        /// create a loop from it's children
        /// </summary>
        public void ScanLoop()
        {
            waypoints = new List<Waypoint>(GetComponentsInChildren<Waypoint>());
            for(int i = 0; i < waypoints.Count; i++)
            {
                waypoints[i].parent = this;
            }
        }
        /// <summary>
        /// remove all waypoints in the loop
        /// </summary>
        public void ClearWaypoints()
        {
            int count = waypoints.Count;
            for (int i = 0; i < count; i++)
            {
                Destroy(waypoints[0].gameObject);
                waypoints.RemoveAt(0);
            }
        }
        /// <summary>
        /// adding new waypoint as a last point in loop
        /// </summary>
        /// <param name="position">position of the waypoint</param>
        /// <returns>the created waypoint</returns>
        public Waypoint AddWaypointAttEnd(Vector3 position)
        {
            
            if (waypoints == null)
                waypoints = new List<Waypoint>();
            Waypoint newPoint = new GameObject("Waypoint" + waypoints.Count, typeof(Waypoint)).GetComponent<Waypoint>();
            newPoint.transform.parent = transform;
            newPoint.CreateHandles();
            if (waypoints.Count > 0)
            {
                Waypoint LastWaypoint = waypoints[waypoints.Count - 1];
                LastWaypoint.Next = newPoint;
                newPoint.previous = LastWaypoint;

                LastWaypoint.HandleB.position = LastWaypoint.transform.position * 2 - LastWaypoint.HandleA.transform.position;
                newPoint.transform.position = position;
                newPoint.HandleA.transform.position = (position + LastWaypoint.HandleB.position) * 0.5f;
                newPoint.UpdateHandle( false);

                if (isClosedLoop) {
                    waypoints[0].previous = newPoint;
                    newPoint.Next = waypoints[0];
                }
                LastWaypoint.RecalculateInBetween();
            }
            else
            newPoint.transform.position = position;
            newPoint.parent = this;
            waypoints.Add(newPoint);
            newPoint.normalDir = Vector3.ProjectOnPlane(Vector3.up, (newPoint.HandleA.position - newPoint.transform.position).normalized).normalized;
            newPoint.RecalculateInBetween();
            
            return newPoint;
        }

        /// <summary>
        /// get all point by sequence from start point to end
        /// </summary>
        /// <param name="Beginindex"></param>
        /// <param name="LastIndex">positive if index from begining
        /// ,and negative if you want the index from last one</param>
        /// <returns>list of positions from start position to end\nif begin is larger than end return empty list, if gegin equal the end and closed loop return all points else return null</returns>
        public List<Vector3> GetPathPointsFromPoint(int Beginindex, int LastIndex)
        {
            if (LastIndex < 0)
            {
                LastIndex = waypoints.Count + LastIndex % waypoints.Count;
            }
            List<Vector3> path = new List<Vector3>();
            if (Beginindex == LastIndex && !isClosedLoop) return path;
            Waypoint st = waypoints[Beginindex];
            Waypoint ls = waypoints[LastIndex];

            path.Add(st.transform.position);
            path.Add(st.HandleB.position);
            st = st.Next;
            int i = 0;
            while (st != ls)
            {
                i++;
                if (i > waypoints.Count) return null;
                path.Add(st.HandleA.position);
                path.Add(st.transform.position);
                path.Add(st.HandleB.position);

                st = st.Next;

            }
            path.Add(st.HandleA.position);
            path.Add(st.transform.position);
            Debug.Log(st.name);
            return path;
        }
        /// <summary>
        /// gets the waypoint list for path from point at Beginindex to point at LastIndex
        /// </summary>
        /// <param name="Beginindex"></param>
        /// <param name="LastIndex"></param>
        /// <returns></returns>
        public List<Waypoint> GetPathWayPoints(int Beginindex, int LastIndex)
        {
            if (Beginindex >= waypoints.Count || LastIndex >= waypoints.Count || Beginindex<0) return null;
            if (LastIndex < 0)
            {
                LastIndex = waypoints.Count + LastIndex % waypoints.Count;
                Debug.Log(LastIndex);
            }
            List<Waypoint> path = new List<Waypoint>();
            if (Beginindex == LastIndex && !isClosedLoop) return path;
            Waypoint st = waypoints[Beginindex];
            Waypoint ls = waypoints[LastIndex];

            path.Add(st);
            st = st.Next;
            int i = 0;
            
            while (st != ls)
            {
                i++;
                if (i > waypoints.Count) return null;
                path.Add(st);

                st = st.Next;

            }
            path.Add(st);
            return path;
        }

        /// <summary>
        /// create new handles, new positions for waypoints, update new connections from last point to first
        /// </summary>
        public void toggleLoop()
        {
            isClosedLoop = !isClosedLoop;
            var last = waypoints.Count - 1;
            if (isClosedLoop)
            {
                waypoints[0].CreateHandles();
                waypoints[last].CreateHandles();
                waypoints[0].UpdateHandle( true);
                waypoints[last].UpdateHandle( false);
                waypoints[last].HandleB.position = waypoints[last].transform.position * 2 - waypoints[last].HandleA.position;
                waypoints[0].HandleA.position = waypoints[0].transform.position * 2 - waypoints[0].HandleB.transform.position;
                waypoints[last].Next = waypoints[0];
                waypoints[0].previous = waypoints[last];
                
            }
            else
            {
                waypoints[last].HandleB = null;
                waypoints[0].HandleA = null;
                waypoints[last].Next = null;
                waypoints[0].previous = null;
            }
        }
        /// <summary>
        /// make loop postion in the center of the waypoints
        /// </summary>
        public void RepositionLoopOrigin()
        {
            Vector3 mean =Vector3.zero;
            for(int i = 0; i < waypoints.Count; i++)
            {
                mean += waypoints[i].transform.localPosition;
            }
            mean = mean / waypoints.Count;
            for (int i = 0; i < waypoints.Count; i++)
            {
                waypoints[i].transform.localPosition= waypoints[i].transform.localPosition-mean;
            }
            transform.position += mean;
           
        }
        /// <summary>
        /// automatic setup of the loop points , set up handles, normals, branches, inbetween points
        /// </summary>
        public void automaticSetup()
        {

            for(int i = 0; i < waypoints.Count; i++)
            {
                int i1 = (waypoints.Count + i - 1) % waypoints.Count;
                int i2 = (i + 1) % waypoints.Count;
                if (i1 == i || i2 == i) return;
                var tangent =( (waypoints[i1].transform.position - waypoints[i].transform.position).normalized - ( waypoints[i2].transform.position - waypoints[i].transform.position).normalized).normalized;
                var dist1 = Vector3.Distance(waypoints[i1].transform.position, waypoints[i].transform.position)/2;
                if(waypoints[i].HandleA)
                waypoints[i].HandleA.position = waypoints[i].transform.position + dist1 * tangent;

                var dist2 = Vector3.Distance(waypoints[i2].transform.position, waypoints[i].transform.position)/2;
                if(waypoints[i].HandleB)
                waypoints[i].HandleB.position = waypoints[i].transform.position - dist2 * tangent;
                waypoints[i].recalculateNormal();
            }
            for (int i = 0; i < waypoints.Count; i++)
            {
                waypoints[i].RecalculateInBetween();
                waypoints[i].RecalculateInBranches();
                waypoints[i].RecalculateReverseBranches();
            }


            }
        /// <summary>
        /// update loop waypoints, position and normals.
        /// used whenever you change anything regarding the loop
        /// </summary>
        public void updateLoopPoints()
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                waypoints[i].OnStateChanged();
            }
        }
        /// <summary>
        /// removes waypoint from loop
        /// </summary>
        /// <param name="index"></param>
        public void RemovePoint(int index)
        {
            if (index !< waypoints.Count) return;
            var point = waypoints[index];
            RemovePoint(point);

        }
        /// <summary>
        /// used when you don't know witch waypoint is near the object
        /// </summary>
        /// <param name="point">the position of the object</param>
        /// <param name="minsnap">the max distance between the point and the waypoint to connect</param>
        /// <returns>the nearst waypoint, null if there is no waypoints in range</returns>
        public Waypoint GetClosestWaypoint(Vector3 point,float minsnap=1f)
        {

            float mindist = float.MaxValue;
            Waypoint Cpoint=null;
            for (int i = 0; i < waypoints.Count; i++)
            {
                float dist = Vector3.Distance(point, waypoints[i].transform.position);
                if ( dist<mindist)
                {
                    mindist = dist;
                    Cpoint = waypoints[i];
                }
            }

            if (mindist <= minsnap)
                return Cpoint;
            return null;

        }
        /// <summary>
        /// remove waypoint from loop
        /// </summary>
        /// <param name="point"></param>
        public void RemovePoint(Waypoint point)
        {
            var prev = point.previous;
            if(prev)
                prev.Next = point.Next;
            var next = point.Next;
            if(next)
                next.previous = prev;
#if UNITY_EDITOR
            DestroyImmediate(point.gameObject);
#else
            Destroy(point.gameObject);
#endif
            waypoints.Remove(point);
        }
        /// <summary>
        ///  add new waypointat position in specific index in loop 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="index"></param>
        public void AddWaypointAtIndex(Vector3 position, int index)
        {
            if (index < waypoints.Count)
                AddWaypoint(position, index);


        }
        /// <summary>
        /// add waypoint after another waypoint in loop
        /// </summary>
        /// <param name="position"></param>
        /// <param name="point"></param>
        public void AddWaypointAfter(Vector3 position, Waypoint point)
        {

            int index = waypoints.IndexOf(point);
            AddWaypoint(position, index);
        }

        /// <summary>
        /// add new waypointat position in specific index in loop 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Waypoint AddWaypoint(Vector3 position , int index)
        {
            index = (index ) % waypoints.Count;
            if (waypoints == null)
                waypoints = new List<Waypoint>();
            Waypoint newPoint = new GameObject("Waypoint" + waypoints.Count, typeof(Waypoint)).GetComponent<Waypoint>();
            newPoint.transform.parent = transform;
            newPoint.CreateHandles();
            if (waypoints.Count > 0)
            {
                Waypoint LastWaypoint = waypoints[index];
                newPoint.Next = LastWaypoint.Next;
                LastWaypoint.Next = newPoint;
                newPoint.previous = LastWaypoint;
                if (newPoint.Next)
                    newPoint.Next.previous = newPoint;
                LastWaypoint.HandleB.position = LastWaypoint.transform.position * 2 - LastWaypoint.HandleA.transform.position;
                newPoint.transform.position = position;
                newPoint.HandleA.transform.position = (position + LastWaypoint.HandleB.position) * 0.5f;
                newPoint.UpdateHandle( false);



                LastWaypoint.RecalculateInBetween();
            }
            else
                newPoint.transform.position = position;

            waypoints.Insert(index+1,newPoint);

            newPoint.parent = this;

            newPoint.normalDir =Vector3.ProjectOnPlane(Vector3.up, (newPoint.HandleA.position - newPoint.transform.position).normalized).normalized;
            newPoint.RecalculateInBetween();
            return newPoint;
        }
        /// <summary>
        /// set up loop inside a waypointsystem
        /// </summary>
        /// <param name="par"></param>
        public void Setup(WaypointSystem par)
        {
            waypoints = new List<Waypoint>();
            if(par)
            parent = par;
            entrances = new List<Waypoint>();
            exits = new List<Waypoint>();
        }
        private void OnDrawGizmos()
        {
            for(int i = 0; i < waypoints.Count; i++)
            {
                if (i == 0 || i == waypoints.Count - 1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(waypoints[i].transform.position, 0.3f);
                }
                else { 
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(waypoints[i].transform.position, 0.3f);
                }
            }
        }
    }

   

}