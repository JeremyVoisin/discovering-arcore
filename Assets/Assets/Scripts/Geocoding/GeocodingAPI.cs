using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;

public class GeocodingAPI
{
    /// <summary>
    /// Starts an asynchronous call to Bing Maps to find the best location according to a user search.
    /// </summary>
    /// <param name="searchQuery">A string searched by user</param>
    /// <param name="callback">The action to execute when Bing Maps answers</param>
    public void Geocode(string searchQuery, Action<List<double>> callback)
    {

        using (var webClient = new WebClient())
        {
            try
            {
                var url = string.Format(Config.BingMapsUrl + searchQuery + "?maxResults=1&key=" + Config.BingMapsAPIKey);

                webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                webClient.DownloadStringCompleted += (sender, resultEvent) =>
                {
                    if(resultEvent.Error != null)
                    {
                        Debug.LogError(resultEvent.Error);
                    }
                    else {
                        var result = JsonUtility.FromJson<GeocodingResult>(resultEvent.Result);
                        callback(result.resourceSets[0].resources[0].point.coordinates);
                    }
                };
                webClient.DownloadStringAsync(new Uri(url));
            }
            catch (WebException e)
            {
                Debug.LogError(e);
            }

        }
    }
}
