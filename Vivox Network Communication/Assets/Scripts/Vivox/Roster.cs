using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VivoxUnity;

public class Roster : MonoBehaviour
{
    public IParticipant participant;
    public TextMeshProUGUI playerNameText;
    public Image chatStatusImage;
    public Sprite playerMuted, playerSpeaking, playerNotSpeaking;

    private VivoxVoiceManager vivoxVoiceManager;

    private bool isMuted;
    public bool IsMuted
    {
        get { return isMuted; }
        private set
        {
            if(participant.IsSelf)
            {
                vivoxVoiceManager.AudioInputDevices.Muted = value;
            }
            else
            {
                if(participant.InAudio)
                {
                    participant.LocalMute = value;
                }
            }
            
            isMuted = value;

            UpdateChatStatusImage();
        }
    }

    private bool isSpeaking;
    public bool IsSpeaking
    {
        get { return isSpeaking; }
        private set
        {
            if(chatStatusImage && !IsMuted)
            {
                isSpeaking = value;
                UpdateChatStatusImage();
            }
        }
    }

    private void UpdateChatStatusImage()
    {
        if (IsMuted)
        {
            chatStatusImage.sprite = playerMuted;
            chatStatusImage.gameObject.transform.localScale = Vector3.one;
        }
        else
        {
            if(IsSpeaking/*IsSpeaking*/)
            {
                chatStatusImage.sprite = playerSpeaking;
                chatStatusImage.gameObject.transform.localScale = Vector3.one;
            }
            else
            {
                chatStatusImage.sprite = playerNotSpeaking;
                chatStatusImage.gameObject.transform.localScale = Vector3.one * 0.85f;
            }
        }
    }

    public void SetupRoster(IParticipant Participant)
    {
        vivoxVoiceManager = VivoxVoiceManager.Instance;
        participant = Participant;

        playerNameText.SetText(participant.Account.DisplayName);
        IsMuted = participant.IsSelf ? vivoxVoiceManager.AudioInputDevices.Muted : participant.LocalMute;
        IsSpeaking = participant.SpeechDetected;

        participant.PropertyChanged += (obj, args) =>
        {
            switch (args.PropertyName)
            {
                case "SpeechDetected":
                    IsSpeaking = participant.SpeechDetected;
                    break;
            }
        };
    }

    public void ToggleMuteStatus()
    {
        IsMuted = !IsMuted;
    }
}
