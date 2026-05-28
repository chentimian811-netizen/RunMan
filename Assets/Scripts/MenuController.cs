using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [Header("启动菜单")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button quitBtn;

    [Header("Host 弹窗")]
    [SerializeField] private GameObject hostPopupPanel;
    [SerializeField] private TMP_InputField roomPasswordInput;
    [SerializeField] private Button confirmHostBtn;
    [SerializeField] private Button cancelHostBtn;

    [Header("Join 弹窗")]
    [SerializeField] private GameObject joinPopupPanel;
    [SerializeField] private TMP_InputField joinIpInput;
    [SerializeField] private TMP_InputField joinPasswordInput;
    [SerializeField] private Button confirmJoinBtn;
    [SerializeField] private Button cancelJoinBtn;

    [Header("游戏内HUD")]
    [SerializeField] private GameObject gameHUDPanel;

    [Header("游戏内暂停菜单")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button exitGameBtn;

    public bool IsMenuOpen => menuPanel.activeSelf || (pausePanel != null && pausePanel.activeSelf);

    private void Start()
    {
        instance = this;

        menuPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
        hostPopupPanel.SetActive(false);
        joinPopupPanel.SetActive(false);

        hostBtn.onClick.AddListener(OnHost);
        joinBtn.onClick.AddListener(OnJoin);
        quitBtn.onClick.AddListener(OnQuit);

        confirmHostBtn.onClick.AddListener(OnConfirmHost);
        cancelHostBtn.onClick.AddListener(OnCancelPopup);
        confirmJoinBtn.onClick.AddListener(OnConfirmJoin);
        cancelJoinBtn.onClick.AddListener(OnCancelPopup);

        if (resumeBtn != null)
            resumeBtn.onClick.AddListener(OnResume);
        if (exitGameBtn != null)
            exitGameBtn.onClick.AddListener(OnExitGame);

        SetCursorState(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (NetworkServer.active || NetworkClient.active)
            {
                if (!pausePanel.activeSelf)
                {
                    pausePanel.SetActive(true);
                    SetCursorState(true);
                }
                else
                {
                    OnResume();
                }
            }
            else
            {
                if (hostPopupPanel.activeSelf || joinPopupPanel.activeSelf)
                {
                    OnCancelPopup();
                }
                else if (!menuPanel.activeSelf)
                {
                    menuPanel.SetActive(true);
                    SetCursorState(true);
                }
            }
        }

        if (!NetworkServer.active && !NetworkClient.active)
        {
            if (pausePanel.activeSelf)
                pausePanel.SetActive(false);

            if (!menuPanel.activeSelf && !hostPopupPanel.activeSelf && !joinPopupPanel.activeSelf)
                menuPanel.SetActive(true);

            SetCursorState(true);
        }
    }

    private void OnHost()
    {
        menuPanel.SetActive(false);
        hostPopupPanel.SetActive(true);
        roomPasswordInput.text = "";
    }

    private void OnConfirmHost()
    {
        RoomAuthenticator authenticator = NetworkManager.singleton.GetComponent<RoomAuthenticator>();
        if (authenticator != null)
        {
            authenticator.roomPassword = roomPasswordInput.text;
            authenticator.SetClientPassword(roomPasswordInput.text);
        }

        NetworkManager.singleton.StartHost();
        hostPopupPanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(true);
        SetCursorState(false);
    }

    private void OnJoin()
    {
        menuPanel.SetActive(false);
        joinPopupPanel.SetActive(true);
        joinIpInput.text = "localhost";
        joinPasswordInput.text = "";
    }

    private void OnConfirmJoin()
    {
        if (string.IsNullOrEmpty(joinIpInput.text))
        {
            Debug.LogWarning("请输入IP地址");
            return;
        }

        NetworkManager.singleton.networkAddress = joinIpInput.text;

        RoomAuthenticator authenticator = NetworkManager.singleton.GetComponent<RoomAuthenticator>();
        if (authenticator != null)
        {
            authenticator.SetClientPassword(joinPasswordInput.text);
        }

        NetworkManager.singleton.StartClient();
        joinPopupPanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(true);
        SetCursorState(false);
    }

    private void OnCancelPopup()
    {
        hostPopupPanel.SetActive(false);
        joinPopupPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private void OnResume()
    {
        pausePanel.SetActive(false);
        SetCursorState(false);
    }

    private void OnExitGame()
    {
        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
        else if (NetworkClient.active)
            NetworkManager.singleton.StopClient();

        pausePanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
        menuPanel.SetActive(true);
        SetCursorState(true);
    }

    private void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
