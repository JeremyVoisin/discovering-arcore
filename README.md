# Discovering ARCore

## Let's try Augmented Reality with Unity and ARCore !

If you would like to try ARCore, here is a sample app, developped to be presented in some conferences. 

Have a quick look at this video if you would know what this repo is about :

[![A mountain in your room](https://www.youtube.com/watch?v=igoekAdzNyQ/0.jpg)](https://www.youtube.com/watch?v=igoekAdzNyQ)

## Behind the scene

This application is based on Unity and uses external services :

* [Bing Maps API](https://www.bingmapsportal.com)
* [OpenElevation API](https://github.com/JeremyVoisin/open-elevation)
* [MapTiler API](https://www.maptiler.com)
* [GCP Cloud Anchors](https://console.cloud.google.com/apis/library/arcorecloudanchor.googleapis.com)

Today, it only runs on Android but it can be ported to iOS with some minor modifications. 

## Prerequisites

Before you start, there are some prerequisites to enable cloud services to be set up in your app.

### Bing Maps API

First of all, to resolve real world coordinates given to the app to load all elevations and map tiles, you should get a Bing Maps API Key by following [these few steps](https://docs.microsoft.com/en-us/bingmaps/getting-started/bing-maps-dev-center-help/getting-a-bing-maps-key). It's free with a good quota.

Once you get it, you could paste it in place of the `Change me by a bing maps API Key` string in the Configuration class (found in `Assets/Assets/Scripts/Configuration/Configuration.cs`)

### Map Tiler API

To be able to load each tile as the texture of the MapTiles, you have to get a Map Tiler API Key by [creating an account and an API Key](https://www.maptiler.com/cloud/).

Once you get it, you could paste it in place of the `Change me by a MapTiler API Key` string in the Configuration class (found in `Assets/Assets/Scripts/Configuration/Configuration.cs`)

### GCP Cloud Anchors - Unity Network

To play the multiplayer mode you see in the video, there are two important steps :

* First you need to [enable multiplayer services in Unity](https://docs.unity3d.com/Manual/UnityMultiplayerSettingUp.html)
* Then, to enable Google Cloud Anchors, you have to follow the steps [described here](https://developers.google.com/ar/develop/unity/cloud-anchors/quickstart-unity-android#add_an_api_key)

### Open Elevation API

The last external service you need, and certainly the most important, is Open Elevation. You need it to be able to load all elevations you can see in the video. To set up your own Open Elevation server, you can [all you need here](https://github.com/JeremyVoisin/open-elevation). 

When your server is ready to go, just replace the `Change me by an open elevation URL` string in the Configuration class (found in `Assets/Assets/Scripts/Configuration/Configuration.cs`), by your freshly created server's URL.

## Getting the app started

When your services are ready, go to Unity, **File>Build settings>Android** and if it's not already done, click **Switch platform**.

You're now ready to start your app ! Just hit **Build and Run** and enjoy traveling the world from your living room ! :)
