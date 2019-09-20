using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour 
{

    public Button searchButton;
    public Text text;
    private MapBuilder map;

    void Start()
    {
        searchButton.onClick.AddListener(ButtonClicked);
        this.map = GetComponent<MapBuilder>();
    }

    /// <summary>
    /// Launches the search of a new geolocation
    /// </summary>
    void ButtonClicked()
    {
        GeocodingAPI api = new GeocodingAPI();
        api.Geocode(text.text, (List<double> result) =>
        {
            map.ShowMap((float)result[0], (float)result[1]);
        });
    }

}
