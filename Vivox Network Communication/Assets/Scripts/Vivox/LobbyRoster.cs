using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VivoxUnity;

public class LobbyRoster : MonoBehaviour
{
    public GameObject rosterItem;
    public GameObject rosterContentWindow;
    
    private VivoxVoiceManager vivoxVoiceManager;
    private Dictionary<ChannelId, List<Roster>> rosterList = new Dictionary<ChannelId, List<Roster>>();

    private void Awake()
    {
        vivoxVoiceManager = VivoxVoiceManager.Instance;
        vivoxVoiceManager.OnParticipantAddedEvent += OnParticipantAdded;
        vivoxVoiceManager.OnParticipantRemovedEvent += OnParticipantRemoved;
        vivoxVoiceManager.OnUserLoggedOutEvent += OnUserLoggedOut;

        if(vivoxVoiceManager && vivoxVoiceManager.ActiveChannels.Count>0)
        {
            VivoxNetworkManager vivoxNetworkManager = FindObjectOfType<VivoxNetworkManager>();
            var lobbyChannel = vivoxVoiceManager.ActiveChannels.FirstOrDefault(ac => ac.Channel.Name == vivoxNetworkManager.LobbyChannelName);
            foreach(var participant in vivoxVoiceManager.LoginSession.GetChannelSession(lobbyChannel.Channel).Participants/*lobbyChannel.Participants*/)
            {
                UpdateParticipateRoster(participant, participant.ParentChannelSession.Channel, true);
            }
        }
    }

    private void OnDestroy()
    {
        vivoxVoiceManager.OnParticipantAddedEvent -= OnParticipantAdded;
        vivoxVoiceManager.OnParticipantRemovedEvent -= OnParticipantRemoved;
        vivoxVoiceManager.OnUserLoggedOutEvent -= OnUserLoggedOut;
    }

    private void UpdateParticipateRoster(IParticipant participant, ChannelId channelID, bool participantAdded)
    {
        if(participantAdded)
        {
            GameObject rosterObjects = Instantiate(rosterItem, rosterContentWindow.transform);
            Roster roster = rosterObjects.GetComponent<Roster>();
            List<Roster> channelRosterList;

            if(rosterList.ContainsKey(channelID))
            {
                rosterList.TryGetValue(channelID, out channelRosterList);
                roster.SetupRoster(participant);
                channelRosterList.Add(roster);
                rosterList[channelID] = channelRosterList;
            }
            else
            {
                channelRosterList = new List<Roster>();
                roster.SetupRoster(participant);
                channelRosterList.Add(roster);
                rosterList.Add(channelID, channelRosterList);
            }
        }
        else
        {
            if(rosterList.ContainsKey(channelID))
            {
                Roster removedRoster = rosterList[channelID].FirstOrDefault(ac => ac.participant.Account.Name == participant.Account.Name);
                if(removedRoster != null)
                {
                    rosterList[channelID].Remove(removedRoster);
                    Destroy(removedRoster.gameObject);
                }
                else
                {
                    Debug.Log("Trying to remove a participant with no Roster componenet");
                }
            }
        }
    }

    private void ClearAllRosters()
    {
        foreach(List<Roster> rosterLists in rosterList.Values)
        {
            foreach(Roster roster in rosterLists)
            {
                Destroy(roster.gameObject);
            }
            rosterLists.Clear();
        }
        rosterList.Clear();
    }

    private void OnParticipantAdded(string username, ChannelId channel, IParticipant participant)
    {
        UpdateParticipateRoster(participant, channel, true);
    }

    private void OnParticipantRemoved(string username, ChannelId channel, IParticipant participant)
    {
        UpdateParticipateRoster(participant, channel, false);
    }

    private void OnUserLoggedOut()
    {
        ClearAllRosters();
    }
}
