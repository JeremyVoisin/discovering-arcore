using UnityEngine;
using System;
using System.Collections.Generic;

public class ClientMapBuilder : MapBuilder
{

    /// <summary>
    /// Manages when user is pinching to apply a scale factor
    /// </summary>
    protected override void ProcessTwoTouches()
    {

        base.ProcessTwoTouches();
        if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            transform.position = new Vector3(0, 0, 0);
            NetworkManagerController.m_Manager.client.Send(CloudAnchorController.kScaleId, new UpdateScaleMessage() { newScale = transform.localScale });
        }
    }

    /// <summary>
    /// When received from host, update a map tile
    /// </summary>
    /// <param name="itm">Maptile to be updated</param>
    public void AddTile(MapTile itm)
    {
        if (!_mapTiles.Contains(itm))
        {
            _mapTiles.Add(itm);
            itm.transform.SetParent(transform);
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
                    MapTile component = obj.GetComponent<ClientMapTile>();
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
}
