using UnityEngine;

namespace Mirror
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkStartPosition")]
    [HelpURL("https://vis2k.github.io/Mirror/Components/NetworkStartPosition")]
    public class NetworkStartPosition : MonoBehaviour
    {
        [HideInInspector] public bool occupied = false;

        public void Awake()
        {
            NetworkManager.RegisterStartPosition(transform);
        }

        public void OnDestroy()
        {
            NetworkManager.UnRegisterStartPosition(transform);
        }
    }
}
