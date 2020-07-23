using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VivoxUnity;

public class LobbyManager : MonoBehaviour
{
    public Button logoutButton;
    public GameObject lobby;
    public TextMeshProUGUI lobbyName;

    private VivoxVoiceManager vivoxVoiceManager;
    private VivoxNetworkManager vivoxNetworkManager;

    private void Awake()
    {
        vivoxVoiceManager = VivoxVoiceManager.Instance;
        vivoxVoiceManager.OnUserLoggedOutEvent += OnUserLoggedOut;
        vivoxVoiceManager.OnParticipantAddedEvent += OnParticipantAdded;

        vivoxNetworkManager = FindObjectOfType<VivoxNetworkManager>();
    }

    private void OnDestroy()
    {
        vivoxVoiceManager.OnUserLoggedOutEvent -= OnUserLoggedOut;
        vivoxVoiceManager.OnParticipantAddedEvent -= OnParticipantAdded;
    }

    private void OnUserLoggedOut()
    {
        vivoxVoiceManager.DisconnectAllChannels();

        lobby.SetActive(false);
    }

    private void OnParticipantAdded(string userName, ChannelId channelId, IParticipant participant)
    {
        lobby.SetActive(true);
        logoutButton.interactable = true;
        lobbyName.SetText(vivoxNetworkManager.LobbyChannelName);
        
        if (channelId.Name == vivoxNetworkManager.LobbyChannelName && participant.IsSelf)
        {
            vivoxNetworkManager.SendLobbyUpdate(VivoxNetworkManager.MatchStatus.Seeking);
        }
    }

    public void LogoutOfVivoxService()
    {
        logoutButton.interactable = false;
        vivoxVoiceManager.DisconnectAllChannels();

        vivoxVoiceManager.Logout();
    }
}
