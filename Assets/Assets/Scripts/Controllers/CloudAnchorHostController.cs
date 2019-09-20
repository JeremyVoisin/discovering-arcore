using GoogleARCore;
using GoogleARCore.CrossPlatform;
using UnityEngine;
using UnityEngine.Networking;

public class CloudAnchorHostController : CloudAnchorController
{
    /// <summary>
    /// The Unity Start() method.
    /// </summary>
    public override void OnStart()
    {
        base.OnStart();
        m_CurrentMode = ApplicationMode.Hosting;
    }

    /// <summary>
    /// Hosts the user placed cloud anchor and associates the resulting Id with this object.
    /// </summary>
    /// <param name="lastPlacedAnchor">The last placed anchor.</param>
    public void HostLastPlacedAnchor(Component lastPlacedAnchor)
    {
        NetworkServer.RegisterHandler(kSyncRequestId, SyncRequestMessage);
        NetworkServer.RegisterHandler(kScaleId, ReceiveScale);

        var anchor = (Anchor)lastPlacedAnchor;

        XPSession.CreateCloudAnchor(anchor).ThenAction(result =>
        {
            if (result.Response != CloudServiceResponse.Success)
            {
                Debug.LogError(string.Format("Failed to host Cloud Anchor: {0}", result.Response));

                OnAnchorHosted(false, result.Response.ToString());
                return;
            }
            m_CloudAnchorId = result.Anchor.CloudId;
            NetworkServer.RegisterHandler(kAskCloudId, AskCloudMessage);
            NetworkServer.SendToAll(kReceivedCloudId, new StartSessionMessage() { cloudId =  m_CloudAnchorId });

            OnAnchorHosted(true, result.Response.ToString());

        });

    }

    /// <summary>
    /// Instantiates the anchor object at the pose of the m_LastPlacedAnchor Anchor. This will host the Cloud
    /// Anchor.
    /// </summary>
    private void _InstantiateAnchor()
    {
        SetSelectedPlane();
        OnAnchorInstantiated();
        HostLastPlacedAnchor(m_LastPlacedAnchor);

        ((HostMapBuilder)mapBuilder).HostOnNetwork();
        mapBuilder.transform.position = new Vector3(0, 0, 0);
    }

    protected void SetSelectedPlane()
    {
        mapBuilder.SetSelectedPlane(m_LastPlacedAnchor);
    }

    //Use this to receive the message from the Server on the Client's side
    public void SyncRequestMessage(NetworkMessage networkMessage)
    {
        ((HostMapBuilder)mapBuilder).UpdateNetwork();
    }

    /// <summary>
    /// Callback indicating that the Cloud Anchor was instantiated and the host request was made.
    /// </summary>
    /// <param name="isHost">Indicates whether this player is the host.</param>
    public override void OnAnchorInstantiated()
    {
        _ShowAndroidToastMessage("Hosting Cloud Anchor...");
    }

    /// <summary>
    /// Callback indicating that the Cloud Anchor was hosted.
    /// </summary>
    /// <param name="success">If set to <c>true</c> indicates the Cloud Anchor was hosted successfully.</param>
    /// <param name="response">The response string received.</param>
    public void OnAnchorHosted(bool success, string response)
    {
        if (success)
        {
            m_CurrentMode = ApplicationMode.Ready;
            _ShowAndroidToastMessage("Cloud Anchor successfully hosted! Tap to place more flags.");
        }
        else
        {
            _ShowAndroidToastMessage("Cloud Anchor could not be hosted. " + response);
        }
    }

    /// <summary>
    /// This method manages user interaction
    /// </summary>
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

        if (m_LastPlacedAnchor != null)
        {
            if (_CanPlaceFlags())
            {
                mapBuilder.ProcessTouches();
            }
            else if (!m_IsOriginPlaced)
            {
                SetWorldOrigin(m_LastPlacedAnchor.transform);
                _InstantiateAnchor();
                OnAnchorInstantiated();
            }
        }
    }

    /// <summary>
    /// Indicates whether a flag can be placed.
    /// </summary>
    /// <returns><c>true</c>, if flag can be placed, <c>false</c> otherwise.</returns>
    protected override bool _CanPlaceFlags()
    {
        return m_CurrentMode == ApplicationMode.Ready;
    }

    public void ReceiveScale(NetworkMessage networkMessage)
    {
        UpdateScaleMessage hostMessage = networkMessage.ReadMessage<UpdateScaleMessage>();
        ((HostMapBuilder)mapBuilder).SetScale(hostMessage.newScale);
    }

    public void AskCloudMessage(NetworkMessage networkMessage)
    {
        NetworkServer.SendToAll(kReceivedCloudId, new StartSessionMessage() { cloudId = m_CloudAnchorId });
    }
    

}
