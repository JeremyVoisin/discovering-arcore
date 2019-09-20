using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// A location where to search the elevation
/// </summary>
[Serializable]
public class Location{
    public float latitude;
    public float longitude;

    public override string ToString()
    {
        return "latitude: " + latitude + " longitude: " + longitude;
    }
}

/// <summary>
/// A list of location where to search the elevation
/// </summary>
[Serializable]
public class ElevationRequest{
    public List<Location> locations;

    public ElevationRequest()
    {
        locations = new List<Location>();
    }
}
