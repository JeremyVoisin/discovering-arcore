using UnityEngine;

using System.Collections.Generic;
using TMPro.SpriteAssetUtilities;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// A helper script to set the apparent world origin of ARCore through applying an offset to the
/// ARCoreDevice (and therefore also it's FirstPersonCamera child); this also provides mechanisms
/// to handle resulting changes to ARCore plane positioning and raycasting.
/// </summary>
public class WorldOriginHelper : MonoBehaviour
{
    /// <summary>
    /// The transform of the ARCore Device.
    /// </summary>
    public ARSessionOrigin SessionOrigin;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject DetectedPlanePrefab;

    /// <summary>
    /// A list to hold the planes ARCore began tracking before the WorldOrigin was placed.
    /// </summary>
    private List<GameObject> m_PlanesBeforeOrigin = new List<GameObject>();

    /// <summary>
    /// Indicates whether the Origin of the new World Coordinate System, i.e. the Cloud Anchor, was placed.
    /// </summary>
    private bool m_IsOriginPlaced = false;

    /// <summary>
    /// The Transform of the Anchor object representing the World Origin.
    /// </summary>
    private Transform m_AnchorTransform;

    /// <summary>
    /// The Unity Update() method.
    /// </summary>
    public void Update()
    {
       
    }

    /// <summary>
    /// Sets the apparent world origin of ARCore through applying an offset to the ARCoreDevice (and therefore also
    /// it's FirstPersonCamera child), so that the Origin of Unity's World Coordinate System coincides with the
    /// Anchor. This function needs to be called once the Cloud Anchor is either hosted or resolved.
    /// </summary>
    /// <param name="anchorTransform">Transform of the Cloud Anchor.</param>
    public void SetWorldOrigin(Transform anchorTransform)
    {
        // Each client will store the anchorTransform, and will have to move the ARCoreDevice (and therefore also
        // it's FirstPersonCamera child) and update other trakced poses (planes, anchors, etc.) so that they appear
        // in the same position in the real world.
        if (m_IsOriginPlaced)
        {
            Debug.LogWarning("The World Origin can be set only once.");
            return;
        }

        m_IsOriginPlaced = true;

        m_AnchorTransform = anchorTransform;

        var worldPose = _WorldToAnchorPose(new Pose(SessionOrigin.transform.position,
                                                     SessionOrigin.transform.rotation));
        SessionOrigin.transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);

        foreach (var plane in m_PlanesBeforeOrigin)
        {
            if (plane != null)
            {
                plane.transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
            }
        }

    }

    /// <summary>
    /// Converts a pose from Unity world space to Anchor-relative space.
    /// </summary>
    /// <returns>A pose in Unity world space.</returns>
    /// <param name="pose">A pose in Anchor-relative space.</param>
    public Pose _WorldToAnchorPose(Pose pose)
    {
        if (!m_IsOriginPlaced)
        {
            return pose;
        }

        Matrix4x4 anchorTWorld = Matrix4x4.TRS(m_AnchorTransform.position, m_AnchorTransform.rotation,
                                               Vector3.one).inverse;

        Vector3 position = anchorTWorld.MultiplyPoint(pose.position);
        Quaternion rotation = pose.rotation * Quaternion.LookRotation(
            anchorTWorld.GetColumn(2), anchorTWorld.GetColumn(1));

        return new Pose(position, rotation);
    }
}

