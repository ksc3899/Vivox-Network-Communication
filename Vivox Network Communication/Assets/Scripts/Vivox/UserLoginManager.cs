using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class UserLoginManager : MonoBehaviour
{
    public Button loginButton;
    public TMP_InputField userName;
    public GameObject loginScreen;

    private VivoxVoiceManager vivoxVoiceManager;
    private bool micPermissionDenied;

    private void Awake()
    {
        vivoxVoiceManager = VivoxVoiceManager.Instance;
        vivoxVoiceManager.OnUserLoggedInEvent += OnUserLoggedIn;
        vivoxVoiceManager.OnUserLoggedOutEvent += OnUserLoggedOut;

        if (vivoxVoiceManager.LoginState == VivoxUnity.LoginState.LoggedIn)
        {
            OnUserLoggedIn();
            userName.text = vivoxVoiceManager.LoginSession.Key.DisplayName;
        }
    }

    private void OnDestroy()
    {
        vivoxVoiceManager.OnUserLoggedInEvent -= OnUserLoggedIn;
        vivoxVoiceManager.OnUserLoggedOutEvent -= OnUserLoggedOut;
    }

    public void LoginToVivoxService()
    {
        if(Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            LoginToVivox();
        }
        else
        {
            if(micPermissionDenied)
            {
                micPermissionDenied = false;
                LoginToVivox();
            }
            else
            {
                micPermissionDenied = true;
                Permission.RequestUserPermission(Permission.Microphone);
            }
        }
    }

    private void LoginToVivox()
    {
        loginButton.interactable = false;

        if(string.IsNullOrEmpty(userName.text))
        {
            Debug.LogError("Enter User Name");
            return;
        }

        vivoxVoiceManager.Login(userName.text);
    }

    private void OnUserLoggedIn()
    {
        loginScreen.SetActive(false);
    }

    private void OnUserLoggedOut()
    {
        loginScreen.SetActive(true);
        loginButton.interactable = true;
    }
}