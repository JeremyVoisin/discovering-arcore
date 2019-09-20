
//Create a class for the message you send to the Client
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A network message to notify all other clients of an elevation change
/// </summary>
public class UpdateHostMessage : MessageBase
{
    public Vector3[] list;
    public string texture;
    public string identifier;
}