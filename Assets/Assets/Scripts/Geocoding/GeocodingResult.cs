using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GeoAddress
{
    public string adminDistrict;
    public string countryRegion;
    public string formattedAddress;
}

[Serializable]
public class GeoPoint
{
    public string type;
    public List<double> coordinates;
}

[Serializable]
public class GeocodePoint
{
    public string type;
    public List<double> coordinates;
    public string calculationMethod;
    public List<string> usageTypes;
}

[Serializable]
public class GeoResource
{
    public string __type;
    public List<double> bbox;
    public string name;
    public GeoPoint point;
    public GeoAddress address;
    public string confidence;
    public string entityType;
    public List<GeocodePoint> geocodePoints;
    public List<string> matchCodes;

}

[Serializable]
public class ResourceSet
{
    public int estimatedTotal;
    public List<GeoResource> resources;
}

[Serializable]
public class GeocodingResult
{
    public string authenticationResultCode;
    public string brandLogoUri;
    public string copyright;
    public int statusCode;
    public string statusDescription;
    public string traceId;
    public List<ResourceSet> resourceSets;
}
