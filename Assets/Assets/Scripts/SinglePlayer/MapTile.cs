
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net;
using System;

public class MapTile : DynamicallyTexturedMesh
{

    protected TileInfo _tileData;

    private static float minElevation = float.PositiveInfinity;
    private static float effectiveElevation = float.PositiveInfinity;

    /// <summary>
    /// The lower elevation through all vertices
    /// </summary>
    private static float BaseElevation
    {
        set
        {
            if (value < minElevation)
                minElevation = value;
        }
    }

    private static List<Task> tasks = new List<Task>();

    /// <summary>
    /// When all tiles are loaded, search for the lowest point in each vertices to use it as the 0 over the y-axis
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns></returns>
    public static async Task OnEndLoading(List<MapTile> tiles)
    {
        await Task.WhenAll(tasks);
        foreach (MapTile tile in tiles)
        {
            if (float.IsPositiveInfinity(effectiveElevation))
            {
                tile.transform.Translate(0, -minElevation, 0);
            }
        }
        tasks.Clear();
        effectiveElevation = minElevation;
        minElevation = float.PositiveInfinity;
    }

    /// <summary>
    /// Set current tiles info and starts loading tile image and elevations
    /// </summary>
    /// <param name="tiledata">Current tile infos</param>
    public void SetTileData(TileInfo tiledata)
    {
        if (tiledata != null && !tiledata.Equals(TileData))
        {
            TileData = tiledata;
            GetTextureAsync();
            tasks.Add(DownloadElevationDataFromWebAsync());
        }
    }

    /// <summary>
    /// _tileData accessors
    /// </summary>
    public TileInfo TileData
    {
        get { return _tileData; }
        set
        {
            _tileData = value;
            ImageUrl = string.Format(Config.MapTilerUrl + Config.MapTilerAPIKey, _tileData.ZoomLevel, _tileData.X, _tileData.Y);
        }
    }

    /// <summary>
    /// Asynchronously calls OpenElevation to get each vertex's height
    /// </summary>
    /// <returns></returns>
    protected async Task DownloadElevationDataFromWebAsync()
    {
        ElevationRequest request = _tileData.GetPointsLocations(GetComponent<MeshFilter>().mesh.vertexCount);
        ElevationResult elevationResult = await OpenElevationAPI.RequestElevation(request);
        if (elevationResult != null)
        {
            ApplyElevationData(elevationResult);
        }

    }

    /// <summary>
    /// This method is called after an OpenElevation call. An OpenElevation response contains 121 elevations, corresponding to each tiles' vertices.
    /// </summary>
    /// <param name="elevationData">OpenElevation's answer</param>
    private void ApplyElevationData(ElevationResult elevationData)
    {
        var threeDScale = _tileData.ScaleFactor;

        var resource = elevationData.results;

        var verts = new List<Vector3>();
        var mesh = GetComponent<MeshFilter>().mesh;
        float localMinElevation = float.PositiveInfinity;

        for (var i = 0; i < mesh.vertexCount; i++)
        {
            var newPos = mesh.vertices[i];
            newPos.y = resource[i].elevation / threeDScale;
            verts.Add(newPos);
            if (newPos.y < localMinElevation)
                localMinElevation = newPos.y;
        }
        BaseElevation = localMinElevation;

        RebuildMesh(mesh, verts);
    }

    /// <summary>
    /// Apply a new vertices configuration and rebuild the mesh.
    /// </summary>
    /// <param name="mesh">The mesg filter</param>
    /// <param name="verts">The new vertices configuration</param>
    protected virtual void RebuildMesh(Mesh mesh, List<Vector3> verts)
    {
        mesh.SetVertices(verts);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        DestroyImmediate(gameObject.GetComponent<MeshCollider>());
        var meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    
}
