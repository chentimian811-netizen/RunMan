using Mirror;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;

    public override void OnStartLocalPlayer()
    {
        if(playerCamera != null)
        {
            playerCamera.enabled = true;
        }
    }

    private void Awake()
    {
        if(playerCamera != null)
        {
            playerCamera.enabled = false;
        }
    }
}
