//Create a class for the message you send to the Client
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A network message to notify all other clients of the session start
/// </summary>
public class StartSessionMessage : MessageBase
{
    public string cloudId;
}