using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScreenController: MonoBehaviour
{
    //
    public Button soloButton;
    public Button cloudButton;
    public Button createButton;
    public Button refreshButton;
    public Canvas sessionCanvas;
    public GameObject snackBar;

    void Start()
    {
        soloButton.onClick.AddListener(LoadGame);
        cloudButton.onClick.AddListener(EnableCloudAnchorsInput);
    }

    /// <summary>
    /// Loads the "Singleplayer" scene
    /// </summary>
    void LoadGame() { 
        UnityEngine.SceneManagement.SceneManager.LoadScene("Anchors");
    }

    /// <summary>
    /// When user clicks "Join an ARCore session", display the related options
    /// </summary>
    void EnableCloudAnchorsInput()
    {
        cloudButton.gameObject.SetActive(false);
        soloButton.gameObject.SetActive(false);

        sessionCanvas.gameObject.SetActive(true);
        refreshButton.gameObject.SetActive(true);
        createButton.gameObject.SetActive(true);
        snackBar.SetActive(true);
    }

    /// <summary>
    /// Cancel the "Join an ARCore session" choice
    /// </summary>
    void Back()
    {
        cloudButton.gameObject.SetActive(true);
        soloButton.gameObject.SetActive(true);

        sessionCanvas.gameObject.SetActive(false);
        refreshButton.gameObject.SetActive(false);
        createButton.gameObject.SetActive(false);
        snackBar.SetActive(false);
    }
}
