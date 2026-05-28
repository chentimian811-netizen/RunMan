using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoomAuthenticator : NetworkAuthenticator
{
    public static RoomAuthenticator instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Room Password")]
    public string roomPassword = "";
    private string clientPassword = "";

    private HashSet<NetworkConnectionToClient> pendingDisconnect = new HashSet<NetworkConnectionToClient>();

    #region Messages

    public struct AuthRequestMessage : NetworkMessage
    {
        public string roomPassword;
    }

    public struct AuthResponseMessage : NetworkMessage
    {
        public byte code;
        public string message;
    }

    #endregion

    #region Server

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    public override void OnStopServer()
    {
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    public override void OnServerAuthenticate(NetworkConnectionToClient conn)
    {
        // 等待客户端发送 AuthRequestMessage
    }

    public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        if (pendingDisconnect.Contains(conn)) return;

        if (msg.roomPassword == roomPassword)
        {
            AuthResponseMessage authResponse = new AuthResponseMessage
            {
                code = 100,
                message = "Success"
            };

            conn.Send(authResponse);
            ServerAccept(conn);
        }
        else
        {
            pendingDisconnect.Add(conn);

            AuthResponseMessage authResponse = new AuthResponseMessage
            {
                code = 200,
                message = "房间密码错误"
            };

            conn.Send(authResponse);
            conn.isAuthenticated = false;

            StartCoroutine(DelayedDisconnect(conn, 1f));
        }
    }

    private IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ServerReject(conn);
        yield return null;
        pendingDisconnect.Remove(conn);
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    public override void OnStopClient()
    {
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    public override void OnClientAuthenticate()
    {
        AuthRequestMessage authRequest = new AuthRequestMessage
        {
            roomPassword = clientPassword
        };

        NetworkClient.Send(authRequest);
    }

    public void SetClientPassword(string password)
    {
        clientPassword = password;
    }

    public void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        if (msg.code == 100)
        {
            ClientAccept();
        }
        else
        {
            Debug.LogError("房间密码错误");
            ClientReject();
        }
    }

    #endregion
}
