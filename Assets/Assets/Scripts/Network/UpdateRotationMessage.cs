using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A network message to notify all other clients of a rotation change
/// </summary>
public class UpdateRotationMessage : MessageBase
{
    public Quaternion rotation;
}