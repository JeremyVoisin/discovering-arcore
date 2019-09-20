
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

/// <summary>
/// Controller managing UI for joining and creating rooms.
/// </summary>
public class NetworkManagerController : MonoBehaviour
{
    /// <summary>
    /// The Lobby Screen to see Available Rooms or create a new one.
    /// </summary>
    public Canvas LobbyScreen;

    /// <summary>
    /// The snackbar text.
    /// </summary>
    public Text SnackbarText;

    /// <summary>
    /// The Panel containing the list of available rooms to join.
    /// </summary>
    public GameObject RoomListPanel;

    /// <summary>
    /// Text indicating that no previous rooms exist.
    /// </summary>
    public Text NoPreviousRoomsText;

    /// <summary>
    /// The prefab for a row in the available rooms list.
    /// </summary>
    public GameObject JoinRoomListRowPrefab;

    /// <summary>
    /// The Network Manager.
    /// </summary>
    public static NetworkManager m_Manager = null;

    /// <summary>
    /// The Join Room buttons.
    /// </summary>
    private List<GameObject> m_JoinRoomButtonsPool = new List<GameObject>();

    /// <summary>
    /// The Unity Awake() method.
    /// </summary>
    public void Awake()
    {
        // Initialize the pool of Join Room buttons.
        for (int i = 0; i < 5; i++)
        {
            GameObject button = Instantiate(JoinRoomListRowPrefab);
            button.transform.SetParent(RoomListPanel.transform, false);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100 - (200 * i));
            button.SetActive(true);
            button.GetComponentInChildren<Text>().text = string.Empty;
            m_JoinRoomButtonsPool.Add(button);
        }

       

        m_Manager = GetComponent<NetworkManager>();

        m_Manager.StartMatchMaker();
        OnRefreshRoomListClicked();
        
    }

    /// <summary>
    /// Handles the user intent to create a new room.
    /// </summary>
    public void OnCreateRoomClicked()
    {
        m_Manager.matchMaker.CreateMatch(m_Manager.matchName, m_Manager.matchSize,
                                       true, string.Empty, string.Empty, string.Empty,
                                       0, 0, _OnMatchCreate);
    }

    /// <summary>
    /// Handles the user intent to refresh the room list.
    /// </summary>
    public void OnRefreshRoomListClicked()
    {
        m_Manager.matchMaker.ListMatches(
            startPageNumber: 0,
            resultPageSize: 5,
            matchNameFilter: string.Empty,
            filterOutPrivateMatchesFromResults: false,
            eloScoreTarget: 0,
            requestDomain: 0,
            callback: _OnMatchList);
    }

    /// <summary>
    /// Handles the user intent to join the room associated with the button clicked.
    /// </summary>
    /// <param name="match">The information about the match that the user intents to join.</param>
    private void _OnJoinRoomClicked(MatchInfoSnapshot match)
    {
        m_Manager.matchName = match.name;
        m_Manager.matchMaker.JoinMatch(match.networkId, string.Empty, string.Empty,
                                     string.Empty, 0, 0, _OnMatchJoined);
        
    }

    /// <summary>
    /// Callback that happens when a <see cref="NetworkMatch.ListMatches"/> request has been
    /// processed on the server.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">A text description for the error if success is false.</param>
    /// <param name="matches">A list of matches corresponding to the filters set in the initial list
    /// request.</param>
    private void _OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        m_Manager.OnMatchList(success, extendedInfo, matches);
        if (!success)
        {
            SnackbarText.text = "Could not list matches: " + extendedInfo;
            return;
        }

        if (m_Manager.matches != null)
        {
            // Reset all buttons in the pool.
            foreach (GameObject button in m_JoinRoomButtonsPool)
            {
                button.SetActive(false);
                button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                button.GetComponentInChildren<Text>().text = string.Empty;
            }

            NoPreviousRoomsText.gameObject.SetActive(m_Manager.matches.Count == 0);

            // Add buttons for each existing match.
            int i = 0;
            foreach (var match in m_Manager.matches)
            {
                if (i >= 5)
                {
                    break;
                }

                var text = "Room " + _GeetRoomNumberFromNetworkId(match.networkId);
                GameObject button = m_JoinRoomButtonsPool[i++];
                button.GetComponentInChildren<Text>().text = text;
                button.GetComponentInChildren<Button>().onClick.AddListener(() => _OnJoinRoomClicked(match));
                button.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Callback that happens when a <see cref="NetworkMatch.CreateMatch"/> request has been
    /// processed on the server.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">A text description for the error if success is false.</param>
    /// <param name="matchInfo">The information about the newly created match.</param>
    private void _OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        m_Manager.OnMatchCreate(success, extendedInfo, matchInfo);
        if (!success)
        {
            SnackbarText.text = "Could not create match: " + extendedInfo;
            return;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("CloudAnchorsHost");
    }

    /// <summary>
    /// Callback that happens when a <see cref="NetworkMatch.JoinMatch"/> request has been
    /// processed on the server.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">A text description for the error if success is false.</param>
    /// <param name="matchInfo">The info for the newly joined match.</param>
    private void _OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        m_Manager.OnMatchJoined(success, extendedInfo, matchInfo);
        if (!success)
        {
            SnackbarText.text = "Could not join to match: " + extendedInfo;
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("CloudAnchorsClient");
    }

    private string _GeetRoomNumberFromNetworkId(UnityEngine.Networking.Types.NetworkID networkID)
    {
        return (System.Convert.ToInt64(networkID.ToString()) % 10000).ToString();
    }
}
