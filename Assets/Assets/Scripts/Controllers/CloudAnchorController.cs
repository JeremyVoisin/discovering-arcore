using UnityEngine;
using System.Collections;
using GoogleARCore;
using System;
using GoogleARCore.Examples.Common;
using UnityEngine.Networking;
using System.Collections.Generic;


public abstract class CloudAnchorController : MonoBehaviour
{
    public Camera firstPersonCamera;
    public MapBuilder mapBuilder;
    public LocalPlayerController playerController;

    [Header("ARCore")]

    protected static readonly short kSyncRequestId = 1091;
    public static readonly short kMessageId = 1092;
    public static readonly short kScaleId = 1093;
    public static readonly short kRotationId = 1094;
    public static readonly short kStartSessionId = 1095;
    public static readonly short kAskCloudId = 1096;
    public static readonly short kReceivedCloudId = 1097;

    protected string m_CloudAnchorId = null;

    /// <summary>
    /// The helper that will calculate the World Origin offset when performing a raycast or generating planes.
    /// </summary>
    public WorldOriginHelper ARCoreWorldOriginHelper;

    /// <summary>
    /// Indicates whether the Origin of the new World Coordinate System, i.e. the Cloud Anchor, was placed.
    /// </summary>
    protected bool m_IsOriginPlaced = false;

    /// <summary>
    /// The last placed anchor.
    /// </summary>
    protected Anchor m_LastPlacedAnchor = null;

    /// <summary>
    /// The current cloud anchor mode.
    /// </summary>
    protected ApplicationMode m_CurrentMode;

    /// <summary>
    /// Enumerates modes the application can be in.
    /// </summary>
    public enum ApplicationMode
    {
        Waiting,
        Ready,
        Hosting,
        Resolving,
    }

    private void Start()
    {
        OnStart();
    }

    /// <summary>
    /// The Unity Start() method.
    /// </summary>
    public virtual void OnStart()
    {
        // A Name is provided to the Game Object so it can be found by other Scripts instantiated as prefabs in the
        // scene.
        gameObject.name = "CloudAnchorController";
        _ResetStatus();

    }

    public void Update()
    {
        OnUpdate();
    }

    /// <summary>
    /// The Unity Update() method.
    /// </summary>
    public virtual void OnUpdate()
    {
        _UpdateApplicationLifecycle();

        // If the origin anchor has not been placed yet, then update in resolving mode is complete.
        if (m_CurrentMode == ApplicationMode.Ready && !m_IsOriginPlaced)
        {
            Debug.Log("Origin anchor has already been placed");
            return;
        }

        // If the player has not touched the screen then the update is complete.
        Touch touch;

        if (Input.touchCount == 2)
        {
            touch = Input.GetTouch(0);
        }
        else if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began && Input.touchCount == 1)
        {
            return;
        }
        ProcessTouch(touch);
    }

    protected abstract void ProcessTouch(Touch touch);
    public abstract void OnAnchorInstantiated();

    /// <summary>
    /// Sets the apparent world origin so that the Origin of Unity's World Coordinate System coincides with the
    /// Anchor. This function needs to be called once the Cloud Anchor is either hosted or resolved.
    /// </summary>
    /// <param name="anchorTransform">Transform of the Cloud Anchor.</param>
    public void SetWorldOrigin(Transform anchorTransform)
    {
        if (m_IsOriginPlaced)
        {
            Debug.LogWarning("The World Origin can be set only once.");
            return;
        }

        m_IsOriginPlaced = true;


        ARCoreWorldOriginHelper.SetWorldOrigin(anchorTransform);
    }

    /// <summary>
    /// Indicates whether a flag can be placed.
    /// </summary>
    /// <returns><c>true</c>, if flag can be placed, <c>false</c> otherwise.</returns>
    protected abstract bool _CanPlaceFlags();

    /// <summary>
    /// Resets the internal status.
    /// </summary>
    protected void _ResetStatus()
    {
        // Reset internal status.
        if (m_LastPlacedAnchor != null)
        {
            Destroy(m_LastPlacedAnchor.gameObject);
        }

        m_LastPlacedAnchor = null;
    }

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        var sleepTimeout = SleepTimeout.NeverSleep;

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            sleepTimeout = lostTrackingSleepTimeout;
        }

        Screen.sleepTimeout = sleepTimeout;

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting. Please start the app again.");
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    protected void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }



}
