using Mirror;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public float moveSpeed = 3.5f;
    public float turnSpeed = 120f;

    private VivoxVoiceManager vivoxVoiceManager;
    private TextMeshPro playerNameText;
    public Animator animator;
    [SyncVar(hook =nameof(SetPlayer))] private string playerName;

    private void Awake()
    {
        playerNameText = GetComponentInChildren<TextMeshPro>();
    }

    private void Start()
    {
        if (!isLocalPlayer)
            return;

        vivoxVoiceManager = VivoxVoiceManager.Instance;
        //vivoxNetworkManager = FindObjectOfType<VivoxNetworkManager>();
        
        AssignStartPosition();
        playerName = vivoxVoiceManager.LoginSession.Key.DisplayName;
        CmdUpdatePlayer(playerName);
    }


    private void Update()
    {
        if (!isLocalPlayer)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime * v);
        transform.localRotation *= Quaternion.AngleAxis(h * turnSpeed * Time.deltaTime, Vector3.up);

        if (v != 0)
            animator.SetBool("Moving", true);
        else
            animator.SetBool("Moving", false);
    }

    private void AssignStartPosition()
    {
        NetworkStartPosition[] startPositions = FindObjectsOfType<NetworkStartPosition>();
        int index = UnityEngine.Random.Range(0, startPositions.Length);

        if (startPositions[index].occupied)
        {
            AssignStartPosition();
        }
        else
        {
            startPositions[index].occupied = true;

            this.transform.position = startPositions[index].transform.position;
            this.transform.rotation = startPositions[index].transform.rotation;
        }
    }

    [Command]
    private void CmdUpdatePlayer(string nameOfPlayer)
    {
        playerNameText.SetText(nameOfPlayer);
        RpcSetPlayer(nameOfPlayer);
    }

    [ClientRpc]
    private void RpcSetPlayer(string nameOfPlayer)
    {
        playerNameText.SetText(nameOfPlayer);
        
        if(nameOfPlayer.Contains("Aksa"))
        {
            transform.GetChild(0).gameObject.SetActive(true);
            animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        }
        else if (nameOfPlayer.Contains("Sai"))
        {
            transform.GetChild(1).gameObject.SetActive(true);
            animator = transform.GetChild(1).gameObject.GetComponent<Animator>();
        }

        NetworkAnimator networkAnimator = GetComponent<NetworkAnimator>();
    }

    private void SetPlayer(string nameOfPlayer)
    {
        playerNameText.SetText(nameOfPlayer);
        
        if (nameOfPlayer.Contains("Aksa") || nameOfPlayer.Contains("aksa"))
        {
            transform.GetChild(0).gameObject.SetActive(true);
            animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        }
        else if (nameOfPlayer.Contains("Sai") || nameOfPlayer.Contains("sai"))
        {
            transform.GetChild(1).gameObject.SetActive(true);
            animator = transform.GetChild(1).gameObject.GetComponent<Animator>();
        }
        
        GetComponent<NetworkAnimator>().animator = animator;
    }
}
