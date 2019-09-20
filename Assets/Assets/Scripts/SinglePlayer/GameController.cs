using UnityEngine;
using GoogleARCore;

public class GameController : MonoBehaviour
{
    public MapBuilder mapBuilder;

    private float initialFingersDistance;
    private Vector3 initialScale;


    private bool isCurrentlyTracking = true;


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // The session status must be Tracking in order to access the Frame.
        if (Session.Status != SessionStatus.Tracking)
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

        TrackableHit hit;
        TrackableHitFlags raycastFilter =
            TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);
            SetSelectedPlane(anchor);
            isCurrentlyTracking = false;
        }        
    }

    /// <summary>
    /// After an anchor was created, set it as the main anchor and as the map's parent
    /// </summary>
    /// <param name="anchor">The new created anchor</param>
    void SetSelectedPlane(Anchor anchor)
    {
        mapBuilder.SetSelectedPlane(anchor);
    }

}