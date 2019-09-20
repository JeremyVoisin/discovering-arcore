using System;
using UnityEngine;

public class TileInfo : IEquatable<TileInfo>
{
    public float MapTileSize { get; private set; }

    private const int MapPixelSize = 256;
    private const float rT = 6378.137f;


    public TileInfo(WorldCoordinate centerLocation, int zoom, float mapTileSize)
    {
        SetStandardValues(mapTileSize);
        //http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Tile_numbers_to_lon..2Flat._2
        var latrad = centerLocation.Lat * Mathf.Deg2Rad;
        var n = Math.Pow(2, zoom);
        X = (int)((centerLocation.Lon + 180.0) / 360.0 * n);
        Y = (int)((1.0 - Mathf.Log(Mathf.Tan(latrad) + 1 / Mathf.Cos(latrad)) / Mathf.PI) / 2.0 * n);
        ZoomLevel = zoom;
    }

    public TileInfo(int x, int y, int zoom, float mapTileSize)
    {
        SetStandardValues(mapTileSize);
        X = x;
        Y = y;
        ZoomLevel = zoom;
    }


    private void SetStandardValues(float mapTileSize)
    {
        MapTileSize = mapTileSize;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public int ZoomLevel { get; private set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as TileInfo);
    }

    public override string ToString()
    {
        return string.Format("X={0},Y={1},zoom={2}", X, Y, ZoomLevel);
    }

    //********************
    // IEquatable section
    //********************

    public bool Equals(TileInfo other)
    {
        if (other != null)
        {
            return X == other.X && Y == other.Y && ZoomLevel == other.ZoomLevel;
        }

        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = X;
            hashCode = (hashCode* 397) ^ Y;
            hashCode = (hashCode* 397) ^ ZoomLevel;
            return hashCode;
        }
    }

    /// <summary>
    /// OpenStreetMap Zoom Levels (see http://wiki.openstreetmap.org/wiki/Zoom_levels)
    /// </summary>
    private static readonly float[] _zoomScales =
    {
        156412f, 78206f, 39103f, 19551f, 9776f, 4888f, 2444f,
        1222f, 610.984f, 305.492f, 152.746f, 76.373f, 38.187f,
        19.093f, 9.547f, 4.773f, 2.387f, 1.193f, 0.596f, 0.298f
    };

    /// <summary>
    /// Current scale factor from ZoomLevel and TileSize
    /// </summary>
    public float ScaleFactor
    {
        get { return _zoomScales[ZoomLevel] * MapPixelSize; }
    }

    /// <summary>
    /// Calculates the real world coordinates of the north east point in tile
    /// </summary>
    /// <returns>The north east position</returns>
    public WorldCoordinate GetNorthEast()
    {
        return GetNorthWestLocation(X+1, Y, ZoomLevel);
    }


    /// <summary>
    /// Calculates the real world coordinates of the south west point in tile
    /// </summary>
    /// <returns>The south west position</returns>
    public WorldCoordinate GetSouthWest()
    {
        return GetNorthWestLocation(X, Y+1, ZoomLevel);
    }

    /// <summary>
    /// Calculates the real world coordinates of the north west point in tile (see http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#C.23)
    /// </summary>
    /// <param name="tileX">X position of the tile in local space</param>
    /// <param name="tileY">Y position of the tile in local space</param>
    /// <param name="zoomLevel">Zoom level</param>
    /// <returns>The real world coordinates of the north west point</returns>
    private WorldCoordinate GetNorthWestLocation(int tileX, int tileY, int zoomLevel)
    {
        var p = new WorldCoordinate();
        var n = Math.Pow(2.0, zoomLevel);
        p.Lon = (float)(tileX / n * 360.0 - 180.0);
        var latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * tileY / n)));
        p.Lat = (float) (latRad * 180.0 / Math.PI);
        return p;
    }

    /// <summary>
    /// Get an elevation request in real world coordinates for a vertex number in tile
    /// </summary>
    /// <param name="vertexNumber">Vertex number of the tile</param>
    /// <returns>An elevation request with real world coordinates from vertex number</returns>
    public ElevationRequest GetPointsLocations(int vertexNumber){
        var request = new ElevationRequest();
        var southWest = GetSouthWest();
        var northEast = GetNorthEast();
        double tileWidth = Math.Sqrt(vertexNumber);
        double latDistanceBetweenPoints = Math.Abs(Math.Abs(northEast.Lat) - Math.Abs(southWest.Lat))/(tileWidth - 1) ;
        double lngDistanceBetweenPoints = Math.Abs(Math.Abs(northEast.Lon) - Math.Abs(southWest.Lon)) / (tileWidth - 1);
        
        for (int i = 0; i < vertexNumber; i++){
            var lat = GetLatFromSouthWest(i, tileWidth, latDistanceBetweenPoints, southWest);
            var lng = GetLngFromSouthWest(i, tileWidth, lngDistanceBetweenPoints, southWest);
            Location vertexC = new Location();
            vertexC.latitude = (float)lat;
            vertexC.longitude = (float)lng;
            request.locations.Add(vertexC);
        }

        return request;
    }

    /// <summary>
    /// Used to split the tile in 11x11 real world coordinates matrix, starting from south west to north east.
    /// The latitude step between two points is Math.Abs(Math.Abs(northEast.Lat) - Math.Abs(southWest.Lat))/(tileWidth - 1).
    /// </summary>
    /// <param name="i">The index of the processed vertex</param>
    /// <param name="tileWidth">The edge size, in vertices number.</param>
    /// <param name="distanceBetweenPoints">Delta latitude between two vertices</param>
    /// <param name="precLat">Latitude of precedent vertex</param>
    /// <returns>The latitude for the processed vertex</returns>
    private double GetLatFromSouthWest(double i, double tileWidth, double distanceBetweenPoints, WorldCoordinate precLat){
        return precLat.Lat + (distanceBetweenPoints * Math.Floor(i / tileWidth));
    }

    /// <summary>
    /// Used to split the tile in 11x11 real world coordinates matrix, starting from south west to north east.
    /// The longitude step between two points is Math.Abs(Math.Abs(northEast.Lon) - Math.Abs(southWest.Lon)) / (tileWidth - 1).
    /// </summary>
    /// <param name="i">The index of the processed vertex</param>
    /// <param name="tileWidth">The edge size, in vertices number.</param>
    /// <param name="distanceBetweenPoints">Delta latitude between two vertices</param>
    /// <param name="precLng">Longitude of precedent vertex</param>
    /// <returns>The longitude for the processed vertex</returns>
    private double GetLngFromSouthWest(double i, double tileWidth, double distanceBetweenPoints, WorldCoordinate precLng){
        return precLng.Lon + (distanceBetweenPoints * (i % tileWidth));
    }
}
