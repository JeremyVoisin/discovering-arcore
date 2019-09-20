using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientMapTile : MapTile
{
    /// <summary>
    /// When received from host, update a map tile
    /// </summary>
    /// <param name="verts">New vertices configuration</param>
    public void NetworkUpdate(Vector3[] verts)
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        RebuildMesh(mesh, new List<Vector3>(verts));
    }
}
