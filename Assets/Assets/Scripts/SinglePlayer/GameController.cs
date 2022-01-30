using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GameController : MonoBehaviour
{
    public MapBuilder mapBuilder;

    private float initialFingersDistance;
    private Vector3 initialScale;
    public ARSession Session;
   
    public ARAnchorManager m_AnchorManager;

    public ARRaycastManager m_RaycastManager;

    private bool isCurrentlyTracking = true;


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // The session status must be Tracking in order to access the Frame.
        if (ARSession.state != ARSessionState.SessionTracking)
        {
            int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
            return;
        }
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        ProcessTouches();
    }

    /// <summary>
    /// This method manages user interaction
    /// </summary>
    void ProcessTouches()
    {
        if (isCurrentlyTracking)
        {
            if (Input.touchCount == 1)
            {
                ProcessSingleTouch();
            }
        }
        else
        {
            mapBuilder.ProcessTouches();
        }
    }

    /// <summary>
    /// When the user taps its screen, if the main anchor is set, we try to add a flag,
    /// otherwise, if the raycast hits a detected plane, the main anchor is created at the hit point
    /// </summary>
    void ProcessSingleTouch()
    {
        Touch touch;
        if ((touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        var m_Hits = new List<ARRaycastHit>();
        var raycastFilter =
            TrackableType.PlaneWithinPolygon |
            TrackableType.FeaturePoint;
        
        if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
        {
            foreach (var hit in m_Hits)
            {
                var anchor = m_AnchorManager.AddAnchor(new Pose(hit.pose.position, Quaternion.identity));
                SetSelectedPlane(anchor);
                isCurrentlyTracking = false;
            }
        }        
    }
    
    

    /// <summary>
    /// After an anchor was created, set it as the main anchor and as the map's parent
    /// </summary>
    /// <param name="anchor">The new created anchor</param>
    void SetSelectedPlane(ARAnchor anchor)
    {
        mapBuilder.SetSelectedPlane(anchor);
    }

}