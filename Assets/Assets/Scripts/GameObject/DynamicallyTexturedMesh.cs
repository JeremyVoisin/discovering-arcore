using UnityEngine;
using System.Net;
using System;

public class DynamicallyTexturedMesh : MonoBehaviour{
    public string ImageUrl;

    /// <summary>
    /// Apply a texture from server
    /// </summary>
    /// <param name="url"></param>
    public void SetTex(string url)
    {
        ImageUrl = url;
        GetTextureAsync();
    }

    /// <summary>
    /// Loads asynchronously a 256x256 map tile from MapTiler (OpenStreetMap tiles)
    /// </summary>
    protected void GetTextureAsync()
    {
        using (var webClient = new WebClient())
        {
            try
            {
                webClient.DownloadDataCompleted += (sender, e) =>
                {
                    var t = new Texture2D(256, 256);
                    t.LoadImage(e.Result);
                    Destroy(GetComponent<Renderer>().material.mainTexture);
                    GetComponent<Renderer>().material.mainTexture = t;
                };
                webClient.DownloadDataAsync(new Uri(ImageUrl));
            } catch( WebException e)
            {
                Debug.Log(e);
            }
            
        }
    }

}