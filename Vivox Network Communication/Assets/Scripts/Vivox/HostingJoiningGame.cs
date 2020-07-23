using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VivoxUnity;

public class HostingJoiningGame : MonoBehaviour
{
    public TextChat textChat;
    public GameObject hostedGamePrefab;
    public GameObject hostingJoiningContentWindow;

    private VivoxNetworkManager vivoxNetworkManager;
    private VivoxVoiceManager vivoxVoiceManager;
    private Dictionary<string, Button> lobbyPlayers = new Dictionary<string, Button>();

    private void Awake()
    {
        vivoxNetworkManager = FindObjectOfType<VivoxNetworkManager>();
        vivoxVoiceManager = VivoxVoiceManager.Instance;
        vivoxVoiceManager.OnParticipantAddedEvent += OnParticipantAdded;
        vivoxVoiceManager.OnTextMessageLogReceivedEvent += OnTextMessageLogReceived;
    }

    private void OnDestroy()
    {
        vivoxVoiceManager.OnParticipantAddedEvent -= OnParticipantAdded;
        vivoxVoiceManager.OnTextMessageLogReceivedEvent -= OnTextMessageLogReceived;
    }

    private void OnParticipantAdded(string username, ChannelId channel, IParticipant participant)
    {
        throw new NotImplementedException();
    }

    private void OnTextMessageLogReceived(string sender, IChannelTextMessage channelTextMessage)
    {
        if(String.IsNullOrEmpty(channelTextMessage.ApplicationStanzaNamespace))
        {
            return;
        }

        if(channelTextMessage.ApplicationStanzaNamespace.EndsWith(VivoxNetworkManager.MatchStatus.Open.ToString()))
        {
            if (AddJoinButton(channelTextMessage.Sender.Name, channelTextMessage.Sender.DisplayName, channelTextMessage.ApplicationStanzaBody))
                textChat.DisplayHostingMessage(channelTextMessage);
        }
        else if(channelTextMessage.ApplicationStanzaBody.EndsWith(VivoxNetworkManager.MatchStatus.Closed.ToString()))
        {
            if (RemoveJoinButton(channelTextMessage.Sender.Name))
                textChat.DisplayHostingMessage(channelTextMessage);
        }
    }

    private bool AddJoinButton(string hostUserName, string hostDisplayName, string hostIP)
    {
        if (!lobbyPlayers.ContainsKey(hostUserName))
        {
            GameObject hostedGame = Instantiate(hostedGamePrefab, hostingJoiningContentWindow.transform);
            Button hostedGameButton = hostedGame.GetComponent<Button>();
            hostedGameButton.onClick.AddListener(() => JoinMatch(hostIP));
            hostedGame.GetComponentInChildren<TextMeshProUGUI>().SetText(hostDisplayName + "'s Game");
            lobbyPlayers.Add(hostUserName, hostedGameButton);
            return true;
        }

        return false;
    }

    private bool RemoveJoinButton(string hostUserName)
    {
        Button buttonToDestry;
        if(lobbyPlayers.TryGetValue(hostUserName, out buttonToDestry))
        {
            buttonToDestry.onClick.RemoveAllListeners();
            Destroy(buttonToDestry.gameObject);
            lobbyPlayers.Remove(hostUserName);
            return true;
        }

        return false;
    }

    public void JoinMatch(string hostedGameIP)
    {
        vivoxNetworkManager.networkAddress = hostedGameIP;
        vivoxNetworkManager.StartClient();
        vivoxNetworkManager.LeaveAllChannels();
    }

    public void HostMatch()
    {
        vivoxNetworkManager.StartHost();
        vivoxNetworkManager.SendLobbyUpdate(VivoxNetworkManager.MatchStatus.Open);
        vivoxNetworkManager.LeaveAllChannels();
    }
}
