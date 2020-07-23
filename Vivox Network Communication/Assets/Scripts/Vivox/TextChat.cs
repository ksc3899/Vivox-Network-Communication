using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Telepathy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VivoxUnity;

public class TextChat : MonoBehaviour
{
    public GameObject chatContentWindow;
    public TMP_InputField messageInputField;
    public GameObject messageObject;

    private VivoxVoiceManager vivoxVoiceManager;
    private VivoxNetworkManager vivoxNetworkManager;
    private ScrollRect textChatScrollRect;
    private List<GameObject> messagePool = new List<GameObject>();
    private ChannelId lobbyChannelID;
    private const string lobbyChannelName = "lobbyChannel";

    private void Awake()
    {
        vivoxVoiceManager = VivoxVoiceManager.Instance;
        vivoxNetworkManager = FindObjectOfType<VivoxNetworkManager>();
        textChatScrollRect = GetComponent<ScrollRect>();

        if (messagePool.Count > 0)
        {
            ClearChatWindow();
        }

        messageInputField.text = string.Empty;
        messageInputField.Select();
        messageInputField.ActivateInputField();

        vivoxVoiceManager.OnParticipantAddedEvent += OnParticipantAdded;
        vivoxVoiceManager.OnTextMessageLogReceivedEvent += OnTextMessageLogReceived;

        if (vivoxVoiceManager.ActiveChannels.Count > 0)
        {
            lobbyChannelID = vivoxVoiceManager.ActiveChannels.FirstOrDefault(ac => ac.Channel.Name == vivoxNetworkManager.LobbyChannelName).Key;
        }
    }

    private void OnDestroy()
    {
        vivoxVoiceManager.OnParticipantAddedEvent -= OnParticipantAdded;
        vivoxVoiceManager.OnTextMessageLogReceivedEvent -= OnTextMessageLogReceived;
    }

    public void SubmitTextToVivox()
    {
        if(string.IsNullOrEmpty(messageInputField.text))
        {
            return;
        }

        vivoxVoiceManager.SendTextMessage(messageInputField.text, lobbyChannelID);

        messageInputField.text = string.Empty;
        messageInputField.Select();
        messageInputField.ActivateInputField();
    }

    private void ClearChatWindow()
    {
        for(int i = 0; i< messagePool.Count; i++)
        {
            Destroy(messagePool[i]);
        }
        messagePool.Clear();
    }
    
    private void OnParticipantAdded(string username, ChannelId channel, IParticipant participant)
    {
        if(vivoxVoiceManager.ActiveChannels.Count > 0)
        {
            lobbyChannelID = vivoxVoiceManager.ActiveChannels.FirstOrDefault().Channel;
        }
    }

    private void OnTextMessageLogReceived(string sender, IChannelTextMessage channelTextMessage)
    {
        if(!String.IsNullOrEmpty(channelTextMessage.ApplicationStanzaNamespace))
        {
            return;
        }

        GameObject newMessageObject = Instantiate(messageObject, chatContentWindow.transform);
        messagePool.Add(newMessageObject);
        TextMeshProUGUI messageText = newMessageObject.GetComponent<TextMeshProUGUI>();

        if(channelTextMessage.FromSelf)
        {
            messageText.alignment = TextAlignmentOptions.MidlineRight;
            messageText.SetText($"{channelTextMessage.Message} <color=blue>:{sender}</color>\n<color=#5A5A5A><size=14>{channelTextMessage.ReceivedTime}</size></color>");
            StartCoroutine(SetScrollRectPositioning());
        }
        else
        {
            messageText.alignment = TextAlignmentOptions.MidlineLeft;
            messageText.SetText($"<color=green>{sender}:</color> {channelTextMessage.Message}\n<color=#5A5A5A><size=14>{channelTextMessage.ReceivedTime}</size></color>");
        }
    }

    public void DisplayHostingMessage(IChannelTextMessage channelTextMessage)
    {
        GameObject newMessageObject = Instantiate(messageObject, chatContentWindow.transform);
        messagePool.Add(newMessageObject);
        TextMeshProUGUI messageText = newMessageObject.GetComponent<TextMeshProUGUI>();

        if (channelTextMessage.ApplicationStanzaNamespace.EndsWith(VivoxNetworkManager.MatchStatus.Open.ToString()))
        {
            messageText.alignment = TextAlignmentOptions.MidlineLeft;
            messageText.SetText(string.Format($"<color=blue>{channelTextMessage.Sender.DisplayName} has begun hosting a match.</color>\n<color=#5A5A5A><size=8>{channelTextMessage.ReceivedTime}</size></color>"));
        }
        else if (channelTextMessage.ApplicationStanzaNamespace.EndsWith(VivoxNetworkManager.MatchStatus.Closed.ToString()))
        {
            messageText.alignment = TextAlignmentOptions.MidlineLeft;
            messageText.SetText(string.Format($"<color=green>{channelTextMessage.Sender.DisplayName}'s match has ended.</color>\n<color=#5A5A5A><size=8>{channelTextMessage.ReceivedTime}</size></color>"));
        }
    }

    private IEnumerator SetScrollRectPositioning()
    {
        yield return new WaitForEndOfFrame();
        textChatScrollRect.normalizedPosition = new Vector2(0f, 0f);
        yield return null;
    }
}
