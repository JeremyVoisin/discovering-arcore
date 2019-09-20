using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A network message to notify all other clients of a scale change
/// </summary>
public class UpdateScaleMessage : MessageBase
{
    public Vector3 newScale;
}