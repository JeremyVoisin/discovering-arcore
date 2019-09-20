using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class HostMapTile : MapTile
{
    /// <summary>
    /// Rebuilds the mesh and sends it to all player
    /// </summary>
    /// <param name="mesh">The mesh to update</param>
    /// <param name="verts">The new vertices configuration</param>
	protected override void RebuildMesh(Mesh mesh, List<Vector3> verts)
	{
        base.RebuildMesh(mesh, verts);
        SendToNetwork();
	}

    /// <summary>
    /// When a map tile is rebuild, send it to all clients so each player has the same map as the host
    /// </summary>
	public void SendToNetwork()
	{
		var mesh = GetComponent<MeshFilter>().mesh;

		NetworkIdentity ide = GetComponent<NetworkIdentity>();

		UpdateHostMessage message = new UpdateHostMessage { list = mesh.vertices, identifier = gameObject.name, texture = ImageUrl };

		NetworkServer.SendToAll(CloudAnchorController.kMessageId, message);
	}

}
