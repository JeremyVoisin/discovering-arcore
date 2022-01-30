using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARSubsystems;

public class CloudAnchorHostController : CloudAnchorController
{
    
    public ARRaycastManager m_RaycastManager;
    public ARAnchorManager m_AnchorManager;
    private ARCloudAnchor _cloudAnchor;
    private bool newlyPlacedAnchor = false;
    
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

        var anchor = (ARAnchor)lastPlacedAnchor;
        _cloudAnchor = m_AnchorManager.HostCloudAnchor(anchor, 1);
        newlyPlacedAnchor = true;
    }

    void Update()
    {
        base.Update();
        if (_cloudAnchor && newlyPlacedAnchor)
        {
            newlyPlacedAnchor = false;
            // Check the Cloud Anchor state.
            CloudAnchorState cloudAnchorState = _cloudAnchor.cloudAnchorState;
            if (cloudAnchorState == CloudAnchorState.Success)
            {
                m_CloudAnchorId = _cloudAnchor.cloudAnchorId;
                NetworkServer.RegisterHandler(kAskCloudId, AskCloudMessage);
                NetworkServer.SendToAll(kReceivedCloudId, new StartSessionMessage() { cloudId =  m_CloudAnchorId });

                OnAnchorHosted(true, "OK");
            }
            else if (cloudAnchorState == CloudAnchorState.TaskInProgress)
            {
                // Wait, not ready yet.
            }
            else
            {
                OnAnchorHosted(false, "Failed to host Cloud Anchor");
            }
        }
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
        var m_Hits = new List<ARRaycastHit>();
        var raycastFilter =
            TrackableType.PlaneWithinPolygon |
            TrackableType.FeaturePoint;

        if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
        {
            foreach (var hit in m_Hits)
            {
                m_LastPlacedAnchor = m_AnchorManager.AddAnchor(ARCoreWorldOriginHelper._WorldToAnchorPose(hit.pose));
            }
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
