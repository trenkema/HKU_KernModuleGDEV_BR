using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject usernameMenu;
    [SerializeField] private GameObject mainMenu;

    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField createPrivateGameInput;
    [SerializeField] private TMP_InputField joinPrivateGameInput;
    [SerializeField] private Button joinPrivateButton;
    [SerializeField] private Button createPrivateButton;
    [SerializeField] private Slider playerCountSlider;

    [SerializeField] private GameObject roomNotExistingText;

    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject cancelButton;
    [SerializeField] private TextMeshProUGUI amountOfPlayersSearching;
    [SerializeField] private string levelToLoad;

    private bool searchingPublic = false;

    public float delay = 1f;

    private void Start()
    {
        playButton.SetActive(false);
        cancelButton.SetActive(false);

        //if (PhotonNetwork.IsConnected)
        //    PhotonNetwork.Disconnect();

        PhotonNetwork.ConnectUsingSettings();

        amountOfPlayersSearching.text = "";

        PhotonNetwork.SendRate = 40;
        PhotonNetwork.SerializationRate = 20;
    }

    public override void OnConnectedToMaster()
    {
        //if (!PhotonNetwork.InLobby)
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        playButton.SetActive(true);
    }

    public void ChangeUserNameInput()
    {
        if (usernameInput.text.Length >= 3)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public void SetUserName()
    {
        usernameMenu.SetActive(false);
        PhotonNetwork.NickName = usernameInput.text;
        mainMenu.SetActive(true);
    }

    public void SearchPublicGame()
    {
        searchingPublic = true;
        playButton.SetActive(false);
        cancelButton.SetActive(true);
        cancelButton.GetComponent<Button>().interactable = false;
        StartCoroutine(EnableButtonActive(cancelButton));
        PhotonNetwork.JoinRandomRoom();
    }

    IEnumerator EnableButtonActive(GameObject _button)
    {
        yield return new WaitForSeconds(0.75f);
        _button.GetComponent<Button>().interactable = true;
    }

    public void CancelSearchPublicGame()
    {
        amountOfPlayersSearching.text = "";
        searchingPublic = false;
        cancelButton.SetActive(false);
        playButton.SetActive(true);
        playButton.GetComponent<Button>().interactable = false;
        StartCoroutine(EnableButtonActive(playButton));
        PhotonNetwork.LeaveRoom();
    }

    public void CreatePrivateGame()
    {
        byte playerCount = (byte)playerCountSlider.value;
        PhotonNetwork.CreateRoom(createPrivateGameInput.text, new RoomOptions() { MaxPlayers = playerCount, IsVisible = false }, null);
    }

    public void JoinPrivateGame()
    {
        PhotonNetwork.JoinRoom(joinPrivateGameInput.text);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        StartCoroutine(RoomNotExisting());
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        MakeNewPublicRoom();
    }

    public override void OnJoinedRoom()
    {
        if (searchingPublic)
        {
            Debug.Log("Joined Room");
            amountOfPlayersSearching.text = "Players Searching: " + PhotonNetwork.CurrentRoom.PlayerCount + " / 2";
        }
        else
        {
            PhotonNetwork.LoadLevel(levelToLoad);
        }
    }

    public IEnumerator RoomNotExisting()
    {
        roomNotExistingText.SetActive(true);
        joinPrivateGameInput.interactable = false;
        joinPrivateButton.interactable = false;
        yield return new WaitForSeconds(delay);
        roomNotExistingText.SetActive(false);
        joinPrivateGameInput.interactable = true;
        joinPrivateButton.interactable = true;
    }

    public void MakeNewPublicRoom()
    {
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 5, IsVisible = true, IsOpen = true };
        Hashtable RoomCustomProps = new Hashtable();
        RoomCustomProps.Add("PlayersAlive", 0);
        roomOptions.CustomRoomProperties = RoomCustomProps;

        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient && searchingPublic)
        {
            PhotonNetwork.LoadLevel(levelToLoad);
        }
        else if (searchingPublic)
        {
            amountOfPlayersSearching.text = "Players Searching: " + PhotonNetwork.CurrentRoom.PlayerCount + " / 2";
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (searchingPublic)
        {
            amountOfPlayersSearching.text = "Players Searching: " + PhotonNetwork.CurrentRoom.PlayerCount + " / 2";
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
