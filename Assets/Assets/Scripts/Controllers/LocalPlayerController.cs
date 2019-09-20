//-----------------------------------------------------------------------
// <copyright file="LocalPlayerController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Local player controller. Handles the spawning of the networked Game Objects.
/// </summary>
public class LocalPlayerController : NetworkBehaviour
{
    /// <summary>
    /// The Flag model that will represent networked objects in the scene.
    /// </summary>
    public GameObject FlagPrefab;

    public CloudAnchorController controller;

    /// <summary>
    /// The Unity OnStartLocalPlayer() method.
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        gameObject.name = "LocalPlayer";
    }


    /// <summary>
    /// A command run on the server that will spawn the Flag prefab in all clients.
    /// </summary>
    /// <param name="position">Position of the object to be instantiated.</param>
    /// <param name="rotation">Rotation of the object to be instantiated.</param>
    [Command]
    public void CmdSpawnFlag(Vector3 position, Quaternion rotation)
    {
        var fl = Instantiate(FlagPrefab, position, rotation);
        NetworkServer.Spawn(fl);
    }


    public override void OnStartClient()
    {
        NetworkManagerController.m_Manager.client.Send(CloudAnchorController.kAskCloudId, new UpdateHostMessage());
    }
}

