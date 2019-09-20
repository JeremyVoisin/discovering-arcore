using UnityEngine;

public class Flag : MonoBehaviour
{
    /// <summary>
    /// Make the flags scale up and down with the map
    /// </summary>
    void Start()
    {
        var mapBuilder = GameObject.Find("Map");
        transform.localScale = mapBuilder.transform.localScale;
        transform.SetParent(mapBuilder.transform);
    }
}
