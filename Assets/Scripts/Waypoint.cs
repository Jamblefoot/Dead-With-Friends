using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public Vector3 plane = Vector3.zero;

    public Waypoint[] connections;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        Gizmos.DrawSphere(pos, 0.5f);
        for(int i = 0; i < connections.Length; i++)
        {
            Vector3 otherPos = connections[i].transform.position;
            Gizmos.DrawLine(pos, otherPos);
            Vector3 dir = otherPos - pos;
            Vector3 midPoint = pos + dir * 0.5f;
            Vector3 cross = Vector3.Cross(dir.normalized, Vector3.up);
            Gizmos.DrawLine(midPoint, midPoint - (dir).normalized + cross);
            Gizmos.DrawLine(midPoint, midPoint - (dir).normalized - cross);
        }
    }
}
