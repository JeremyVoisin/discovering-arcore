using System;
using System.Collections.Generic;

/// <summary>
/// A representation of an elevation corresponding to a couple of coordinates
/// </summary>
[Serializable]
public class Resource
{
    public int elevation;
    public float longitude;
    public float latitude;

    public override string ToString()
    {
        return "latitude: " + latitude + " longitude: " + longitude + "elevation: " + elevation;
    }
}

/// <summary>
/// A list of representations of elevations corresponding to a couple of coordinates
/// </summary>
[Serializable]
public class ElevationResult
{
    public ElevationResult()
    {
        results = new List<Resource>();
    }
    public List<Resource> results;
}
