using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CloudAnchorClientController : CloudAnchorController
{
    public ARRaycastManager m_RaycastManager;
    public ARAnchorManager m_AnchorManager;
    private ARCloudAnchor _cloudAnchor;
    
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
        else if (m_CurrentMode == ApplicationMode.Waiting)
        {
            CloudAnchorState cloudAnchorState = _cloudAnchor.cloudAnchorState;
            if (cloudAnchorState == CloudAnchorState.Success)
            {
                Debug.LogError($"Client could not resolve Cloud Anchor {m_CloudAnchorId}");

                OnAnchorResolved(false,$"Client could not resolve Cloud Anchor {m_CloudAnchorId}");
                return;
            }
            
            NetworkManagerController.m_Manager.client.Send(kSyncRequestId, new UpdateHostMessage());
            m_CurrentMode = ApplicationMode.Ready;

            SetWorldOrigin(_cloudAnchor.transform);
            ((ClientMapBuilder)mapBuilder).SetSelectedPlane(m_AnchorManager.AddAnchor(_cloudAnchor.pose));
            ((ClientMapBuilder)mapBuilder).ShowMap();

            OnAnchorResolved(true, $"Resolved Cloud Anchor {m_CloudAnchorId}");
            mapBuilder.transform.position = new Vector3(0, 0, 0);
            _OnResolved(_cloudAnchor.transform);
        }
    }

    /// <summary>
    /// Resolves an anchor id and instantiates an Anchor prefab on it.
    /// </summary>
    /// <param name="cloudAnchorId">Cloud anchor id to be resolved.</param>
    private void _ResolveAnchorFromId(string cloudAnchorId)
    {
        // If device is not tracking, let's wait to try to resolve the anchor.
        if (ARSession.state != ARSessionState.SessionTracking)
        {
            return;
        }
        m_CurrentMode = ApplicationMode.Resolving;
        OnAnchorInstantiated();

        NetworkManagerController.m_Manager.client.RegisterHandler(kMessageId, ReceiveMessage);
        NetworkManagerController.m_Manager.client.RegisterHandler(kScaleId, ReceiveScale);
        NetworkManagerController.m_Manager.client.RegisterHandler(kRotationId, ReceiveRotation);

        _cloudAnchor = m_AnchorManager.ResolveCloudAnchorId(m_CloudAnchorId);
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
