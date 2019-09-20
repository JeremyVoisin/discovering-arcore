
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using UnityEngine;
using UnityEngine.Networking;

public class CloudAnchorClientController : CloudAnchorController
{
    /// <summary>
    /// The Unity Start() method.
    /// </summary>
    public override void OnStart()
    {
        base.OnStart();
        NetworkManagerController.m_Manager.client.RegisterHandler(kReceivedCloudId, ReceivedCloudMessage);
        NetworkManagerController.m_Manager.client.RegisterHandler(kStartSessionId, ReceiveStartSession);
        m_CurrentMode = ApplicationMode.Waiting;
    }

    /// <summary>
    /// The Unity Start() method.
    /// </summary>
    public override void OnUpdate()
    {
        base.OnUpdate();
        if(m_CloudAnchorId != null && m_CurrentMode == ApplicationMode.Waiting)
            _ResolveAnchorFromId(m_CloudAnchorId);
    }

    /// <summary>
    /// Resolves an anchor id and instantiates an Anchor prefab on it.
    /// </summary>
    /// <param name="cloudAnchorId">Cloud anchor id to be resolved.</param>
    private void _ResolveAnchorFromId(string cloudAnchorId)
    {
        // If device is not tracking, let's wait to try to resolve the anchor.
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }
        m_CurrentMode = ApplicationMode.Resolving;
        OnAnchorInstantiated();

        NetworkManagerController.m_Manager.client.RegisterHandler(kMessageId, ReceiveMessage);
        NetworkManagerController.m_Manager.client.RegisterHandler(kScaleId, ReceiveScale);
        NetworkManagerController.m_Manager.client.RegisterHandler(kRotationId, ReceiveRotation);

        XPSession.ResolveCloudAnchor(m_CloudAnchorId).ThenAction((System.Action<CloudAnchorResult>)(result =>
        {
            if (result.Response != CloudServiceResponse.Success)
            {
                Debug.LogError(string.Format("Client could not resolve Cloud Anchor {0}: {1}",
                                             m_CloudAnchorId, result.Response));

                OnAnchorResolved(false, result.Response.ToString());
                return;
            }
            
            NetworkManagerController.m_Manager.client.Send(kSyncRequestId, new UpdateHostMessage());
            m_CurrentMode = ApplicationMode.Ready;

            SetWorldOrigin(result.Anchor.transform);
            ((ClientMapBuilder)mapBuilder).SetSelectedPlane(Session.CreateAnchor(new Pose(result.Anchor.transform.position, result.Anchor.transform.rotation)));
            ((ClientMapBuilder)mapBuilder).ShowMap();

            OnAnchorResolved(true, result.Response.ToString());
            mapBuilder.transform.position = new Vector3(0, 0, 0);
            _OnResolved(result.Anchor.transform);
        }));
    }

    /// <summary>
    /// Callback indicating that the Cloud Anchor was instantiated and the host request was made.
    /// </summary>
    public override void OnAnchorInstantiated()
    {
        _ShowAndroidToastMessage("Cloud Anchor added to session! Attempting to resolve anchor...");
    }

    /// <summary>
    /// Callback indicating that the Cloud Anchor was resolved.
    /// </summary>
    /// <param name="success">If set to <c>true</c> indicates the Cloud Anchor was resolved successfully.</param>
    /// <param name="response">The response string received.</param>
    public void OnAnchorResolved(bool success, string response)
    {
        if (success)
        {
            _ShowAndroidToastMessage("Cloud Anchor successfully resolved! Tap to place more objects.");
        }
        else
        {
            _ShowAndroidToastMessage("Cloud Anchor could not be resolved. Will attempt again. " + response);
        }
        
    }

    /// <summary>
    /// Callback invoked once the Cloud Anchor is resolved.
    /// </summary>
    /// <param name="anchorTransform">Transform of the resolved Cloud Anchor.</param>
    private void _OnResolved(Transform anchorTransform)
    {
        SetWorldOrigin(anchorTransform);
    }

    //Use this to receive the message from the Server on the Client's side
    public void ReceiveMessage(NetworkMessage networkMessage)
    {
        //Read the message that comes in
        UpdateHostMessage hostMessage = networkMessage.ReadMessage<UpdateHostMessage>();

        ClientMapTile tile = GameObject.Find(hostMessage.identifier).GetComponent<ClientMapTile>();
        if (tile != null)
        {
            tile.NetworkUpdate(hostMessage.list);
            tile.SetTex(hostMessage.texture);
        }
        ((ClientMapBuilder)mapBuilder).AddTile(tile);
    }

    //Use this to receive the message from the Server on the Client's side
    public void ReceiveRotation(NetworkMessage networkMessage)
    {
        //Read the message that comes in
        UpdateRotationMessage hostMessage = networkMessage.ReadMessage<UpdateRotationMessage>();

        mapBuilder.transform.rotation = hostMessage.rotation;
    }

    protected override void ProcessTouch(Touch touch)
    {
        TrackableHit hit;
        TrackableHitFlags raycastFilter =
            TrackableHitFlags.PlaneWithinPolygon;
        if (ARCoreWorldOriginHelper.Raycast(touch.position.x, touch.position.y,
                raycastFilter, out hit))
        {
            m_LastPlacedAnchor = hit.Trackable.CreateAnchor(hit.Pose);
        }

        if (_CanPlaceFlags())
        {
            ((ClientMapBuilder)mapBuilder).ProcessTouches();
        }
            
    }

    /// <summary>
    /// Indicates whether a flag can be placed.
    /// </summary>
    /// <returns><c>true</c>, if flag can be placed, <c>false</c> otherwise.</returns>
    protected override bool _CanPlaceFlags()
    {
        return m_CurrentMode == ApplicationMode.Ready && m_IsOriginPlaced;
    }

    public void ReceiveScale(NetworkMessage networkMessage)
    {
        UpdateScaleMessage hostMessage = networkMessage.ReadMessage<UpdateScaleMessage>();
        ((ClientMapBuilder)mapBuilder).SetScale(hostMessage.newScale);
    }

    /// <summary>
    /// If the player joins the server before the cloud anchor was hosted, this callback function is called to tell him when displaying the map is needed
    /// </summary>
    /// <param name="networkMessage">An empty network message</param>
    public void ReceiveStartSession(NetworkMessage networkMessage)
    {
        StartSessionMessage hostMessage = networkMessage.ReadMessage<StartSessionMessage>();
        ((ClientMapBuilder)mapBuilder).ShowMap();
    }

    /// <summary>
    /// This callback function is called when the host have sent the cloud anchor ID
    /// </summary>
    /// <param name="networkMessage">The network message containing a cloud anchor ID</param>
    public void ReceivedCloudMessage(NetworkMessage networkMessage)
    {
        StartSessionMessage hostMessage = networkMessage.ReadMessage<StartSessionMessage>();
        m_CloudAnchorId = hostMessage.cloudId;
    }
}
