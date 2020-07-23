using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VivoxUnity;

public class JoinLobbyManager : MonoBehaviour
{
    public GameObject joinLobbyScreen;
    public TMP_InputField lobbyNameInput;

    private VivoxNetworkManager vivoxNetworkManager;
    private VivoxVoiceManager vivoxVoiceManager;

    private void Awake()
    {
        vivoxVoiceManager = VivoxVoiceManager.Instance;
        vivoxVoiceManager.OnUserLoggedInEvent += OnUserLoggedIn;

        vivoxNetworkManager = FindObjectOfType<VivoxNetworkManager>();

        if (vivoxVoiceManager.LoginState == VivoxUnity.LoginState.LoggedIn)
        {
            joinLobbyScreen.SetActive(true);
            lobbyNameInput.text = null;
            lobbyNameInput.gameObject.SetActive(false);
        }
    }
    
    private void OnUserLoggedIn()
    {
        joinLobbyScreen.SetActive(true);
        lobbyNameInput.text = null;
        lobbyNameInput.gameObject.SetActive(false);
    }

    private void OnParticipantAdded(string userName, ChannelId channelId, IParticipant participant)
    {
        joinLobbyScreen.SetActive(false);
    }

    public void JoinLobby()
    {
        if(lobbyNameInput.gameObject.activeSelf)
        {
            vivoxNetworkManager.LobbyChannelName = lobbyNameInput.text;
        }
        else
        {
            string lobbyName = UnityEngine.Random.Range(1, 1000).ToString();
            vivoxNetworkManager.LobbyChannelName = lobbyName;
        }
        
        var lobbyChannel = vivoxVoiceManager.ActiveChannels.FirstOrDefault(ac => ac.Channel.Name == vivoxNetworkManager.LobbyChannelName);
        if ((vivoxVoiceManager && vivoxVoiceManager.ActiveChannels.Count == 0) || lobbyChannel == null)
        {
            JoinLobbyChannel();
        }
        else
        {
            if (lobbyChannel.AudioState == ConnectionState.Disconnected)
            {
                vivoxNetworkManager.SendLobbyUpdate(VivoxNetworkManager.MatchStatus.Seeking);

                lobbyChannel.BeginSetAudioConnected(true, true, ar =>
                {
                    Debug.Log("Now transmitting into lobby channel");
                });
            }
        }
    }

    private void JoinLobbyChannel()
    {
        vivoxVoiceManager.OnParticipantAddedEvent += OnParticipantAdded;
        vivoxVoiceManager.JoinChannel(vivoxNetworkManager.LobbyChannelName, ChannelType.NonPositional, VivoxVoiceManager.ChatCapability.TextAndAudio);
    }
}
