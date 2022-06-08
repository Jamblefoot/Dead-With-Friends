using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointSorter : MonoBehaviour
{
    [SerializeField] bool looping;
    [SerializeField] int laneCount = 4;

    [SerializeField] bool run = false;

    [SerializeField] Waypoint[] waypoints;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnValidate()
    {

        if(run)
        {
            run = false;
            Populate();
            Run();
            
        }
    }

    void Populate()
    {
        waypoints = GetComponentsInChildren<Waypoint>();
    }

    void Run()
    {
        for(int i = 0; i < waypoints.Length; i++)
        {
            Waypoint[] connections = FindConnections(i, laneCount);
            waypoints[i].connections = connections;
        }
    }

    Waypoint[] FindConnections(int i, int lanes)
    {
        int lane = i % lanes;
        List<Waypoint> newConnects = new List<Waypoint>();
        if(i >= waypoints.Length - laneCount)
        {
            if(looping)
            {
                newConnects.Add(waypoints[lane]); //center
                if(lane >= 0 && lane < lanes - 1)
                    newConnects.Add(waypoints[lane + 1]); //righthand lane
                if(lane > 0)
                    newConnects.Add(waypoints[lane - 1]); //lefthand lane
            }
        }
        else
        {
            TryAddWaypoint(i + lanes, ref newConnects); //center
            if(lane < lanes - 1)
                TryAddWaypoint(i + lanes + 1, ref newConnects); //righthand lane
            if(lane > 0)
                TryAddWaypoint(i + lanes - 1, ref newConnects); //lefthand lane

        }

        return newConnects.ToArray();
    }

    void TryAddWaypoint(int index, ref List<Waypoint> wpList)
    {
        if(index > waypoints.Length || index < 0) return;
        
        if(!wpList.Contains(waypoints[index]))
            wpList.Add(waypoints[index]);
    }
}
