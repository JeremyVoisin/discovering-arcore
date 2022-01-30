using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MapBuilder : MonoBehaviour
{
    public int ZoomLevel = 12;

    public float MapTileSize = 0.5f;

    public GameObject MapTilePrefab;
    public GameObject flag;

    public float MapSize = 12;

    private TileInfo _centerTile;

    protected List<MapTile> _mapTiles;

    protected ARAnchor anchor;
    private Pose pose;
    private float yOffset;
    public Camera firstPersonCamera;

    protected bool isCurrentlyTracking = true;

    protected float initialFingersDistance;
    protected Vector3 initialScale;

    /// <summary>
    /// The start method
    /// </summary>
    void Start()
    {
        _mapTiles = new List<MapTile>();
        gameObject.name = "Map";
    }

    /// <summary>
    /// Resets the main anchor 
    /// </summary>
    /// <param name="newAnchor">The new anchor</param>
    public void SetSelectedPlane(ARAnchor newAnchor)
    {
        // Create the anchor at that point.
        Destroy(anchor);
        isCurrentlyTracking = false;
        anchor = newAnchor;
        CreateAnchor();
    }

    /// <summary>
    /// Create an anchor and attaches the map builder to it
    /// </summary>
    void CreateAnchor()
    {

        // Attach the Mapbuilder to the anchor.
        transform.position = anchor.transform.position;
        transform.rotation = anchor.transform.rotation;
        transform.SetParent(anchor.transform);
        
        // Finally, render the map
        ShowMap();
    }

    /// <summary>
    /// The update method
    /// </summary>
    private void Update()
    {
        if (ARSession.state != ARSessionState.SessionTracking)
        {
            return;
        }

        if (!isCurrentlyTracking)
        {
            foreach (var detectedPlanes in FindObjectsOfType<ARPlaneMeshVisualizer>())
            {
                detectedPlanes.enabled = false;
                detectedPlanes.transform.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    /// <summary>
    /// Displays the map after loading each elevation and each tile
    /// </summary>
    /// <param name="Latitude">The center tile latitude, default is Mont Blanc's latitude</param>
    /// <param name="Longitude">The center tile latitude, default is Mont Blanc's longitude</param>
    public virtual void ShowMap(float Latitude = 45.832675f, float Longitude = 6.865166f)
    {
        _centerTile = new TileInfo(new WorldCoordinate { Lat = Latitude, Lon = Longitude },
            ZoomLevel, MapTileSize);
        LoadTiles();
    }

    /// <summary>
    /// Create a MapSize x MapSize map centered on local (0, 0, 0)
    /// </summary>
    protected virtual void LoadTiles()
    {
        var size = (int)(MapSize / 2);

        var tileIndex = 0;
        List<MapTile> tiles = new List<MapTile>();
        for (var x = -size; x <= size; x++)
        {
            for (var y = -size; y <= size; y++)
            {
                var tile = GetOrCreateTile(x, y, tileIndex++);
                tiles.Add(tile);
                tile.SetTileData(new TileInfo(_centerTile.X - x, _centerTile.Y + y, ZoomLevel, MapTileSize));
                tile.gameObject.name = string.Format("({0},{1})", x, y);
                tile.transform.SetParent(transform);
            }
        }

        // When the map finishes loading, we search for the lower vertex in each MapTile
        // and we use its position as the 0 on y-axis
        MapTile.OnEndLoading(tiles);

    }

    /// <summary>
    /// Creates a MapTile if it doesn't already exists, at position (x, 0, y) 
    /// </summary>
    /// <param name="x">The local x position</param>
    /// <param name="y">The local y position</param>
    /// <param name="i">The index of the processed tile</param>
    /// <returns></returns>
    protected MapTile GetOrCreateTile(int x, int y, int i)
    {
        if (_mapTiles.Any() && _mapTiles.Count > i)
        {
            return _mapTiles[i];
        }

        var mapTile = Instantiate(MapTilePrefab, anchor.transform);
        mapTile.transform.localPosition = new Vector3(MapTileSize * x, 0, MapTileSize * y);
        mapTile.transform.localRotation = Quaternion.identity;
        var tile = mapTile.GetComponent<MapTile>();
        _mapTiles.Add(tile);
        return tile;
    }

    /// <summary>
    /// This method manages user interaction
    /// </summary>
    public void ProcessTouches()
    {
        if (Input.touchCount == 1)
        {
            ProcessSingleTouch();
        }
        else if (Input.touchCount == 2)
        {
            ProcessTwoTouches();
        }
    }

    /// <summary>
    /// When the user taps its screen, if the main anchor is set, we try to add a flag,
    /// otherwise, if the raycast hits a detected plane, the main anchor is created at the hit point
    /// </summary>
    protected virtual void ProcessSingleTouch()
    {
        Touch touch;
        if ((touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        if (!isCurrentlyTracking)
        {
            Ray raycast = firstPersonCamera.ScreenPointToRay(touch.position);
            RaycastHit raycastHit;
            if (Physics.Raycast(raycast, out raycastHit))
            {
                try
                {
                    var obj = GameObject.Find(raycastHit.collider.name);

                    if (obj != null)
                    {
                        MapTile component = obj.GetComponent<MapTile>();
                        if (component != null)
                        {
                            var fl = Instantiate(flag, raycastHit.point, Quaternion.identity);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }
    }


    /// <summary>
    /// Apply a scale factor to the map
    /// </summary>
    /// <param name="scale">The scale factor to apply</param>
    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
        transform.position = new Vector3(0, 0, 0);
    }

    /// <summary>
    /// Manages when user is pinching to apply a scale factor
    /// </summary>
    protected virtual void ProcessTwoTouches()
    {
        Touch touch = Input.GetTouch(1);

        if (touch.phase == TouchPhase.Began)
        {
            initialFingersDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            initialScale = this.transform.localScale;
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            float currentFingerDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            float ScaleFactor = currentFingerDistance / initialFingersDistance;
            transform.localScale = initialScale * ScaleFactor;
        }

    }
}
 