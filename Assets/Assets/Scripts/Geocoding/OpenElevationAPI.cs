using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class OpenElevationAPI
{
    /// <summary>
    /// Starts an asynchronous request to OpenElevation with a list of coordinates
    /// </summary>
    /// <param name="request">A list of coordinates where to find elevations</param>
    /// <returns></returns>
    public static async Task<ElevationResult> RequestElevation(ElevationRequest request)
    {
        var webClient = new WebClient();
            
            string postData = JsonUtility.ToJson(request);
            string url = string.Format(Config.OpenElevationURL);

            webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");


            string response =  await webClient.UploadStringTaskAsync(new Uri(url), "PUT", postData);
            ElevationResult elevationData = JsonUtility.FromJson<ElevationResult>(response);
            return elevationData;
        
    }
}
