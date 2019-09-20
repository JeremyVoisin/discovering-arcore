using UnityEngine.Networking;
using UnityEngine;
using System;

public class HostMapBuilder : MapBuilder
{

    /// <summary>
    /// Send a message when the anchor is hosted
    /// </summary>
	public void HostOnNetwork()
	{
        NetworkServer.SendToAll(CloudAnchorController.kStartSessionId, new StartSessionMessage());
	}

    /// <summary>
    /// Send all tiles elevation and tiles infos to other player in the current session
    /// </summary>
	public void UpdateNetwork()
	{
		NetworkServer.SendToAll(CloudAnchorController.kRotationId, new UpdateRotationMessage() { rotation = transform.rotation });
		foreach (HostMapTile tile in _mapTiles)
		{
			tile.SendToNetwork();
		}
	}

    /// <summary>
    /// When the user taps its screen, if the main anchor is set, we try to add a flag
    /// </summary>
    protected override void ProcessSingleTouch()
    {
        base.ProcessSingleTouch();
        Touch touch;
        if ((touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

      
        Ray raycast = firstPersonCamera.ScreenPointToRay(Input.GetTouch(0).position);
        RaycastHit raycastHit;
        if (Physics.Raycast(raycast, out raycastHit))
        {
            try
            {
                var obj = GameObject.Find(raycastHit.collider.name);

                if (obj != null)
                {
                    MapTile component = obj.GetComponent<MapTile>();
                    if (component != null)
                    {
                        GameObject.Find("LocalPlayer").GetComponent<LocalPlayerController>().CmdSpawnFlag(raycastHit.point, Quaternion.identity);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
        
    }

    /// <summary>
    /// Manages when user is pinching to apply a scale factor
    /// </summary>
    protected override void ProcessTwoTouches()
    {

        base.ProcessTwoTouches();
        if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            transform.position = new Vector3(0, 0, 0);
            NetworkServer.SendToAll(CloudAnchorController.kScaleId, new UpdateScaleMessage() { newScale = transform.localScale });
        }
    }

}
