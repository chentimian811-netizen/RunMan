using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [Header("启动菜单")]
    [SerializeField] private GameObject menuPanel;     // 整个菜单 Panel
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private InputField ipInput;       // 默认 "localhost"

    [Header("游戏内暂停菜单")]
    [SerializeField] private GameObject pausePanel;    // 暂停菜单 Panel
    [SerializeField] private Button resumeBtn;        // 回到游戏按钮
    [SerializeField] private Button exitGameBtn;      // 退出游戏按钮

    public bool IsMenuOpen => menuPanel.activeSelf || (pausePanel != null && pausePanel.activeSelf);

    private void Start()
    {
        instance = this;

        // 游戏启动 → 显示启动菜单，场景画面在后面作为背景
        menuPanel.SetActive(true);
        
        // 确保暂停菜单初始隐藏
        if (pausePanel != null)
            pausePanel.SetActive(false);

        hostBtn.onClick.AddListener(OnHost);
        joinBtn.onClick.AddListener(OnJoin);
        quitBtn.onClick.AddListener(OnQuit);
        
        if (resumeBtn != null)
            resumeBtn.onClick.AddListener(OnResume);
        if (exitGameBtn != null)
            exitGameBtn.onClick.AddListener(OnExitGame);

        ipInput.text = "localhost";
        
        // 启动时显示鼠标
        SetCursorState(true);
    }

    private void Update()
    {
        // ESC 键处理
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 如果已连接（游戏进行中），显示暂停菜单
            if (NetworkServer.active || NetworkClient.active)
            {
                if (!pausePanel.activeSelf)
                {
                    pausePanel.SetActive(true);
                    SetCursorState(true);  // 显示并解锁鼠标
                }
                else
                {
                    // 如果暂停菜单已经打开，按ESC回到游戏
                    OnResume();
                }
            }
            // 如果未连接，显示启动菜单
            else
            {
                if (!menuPanel.activeSelf)
                {
                    menuPanel.SetActive(true);
                    SetCursorState(true);  // 显示并解锁鼠标
                }
            }
        }
        
        // 断开连接后重新显示启动菜单
        if (!NetworkServer.active && !NetworkClient.active)
        {
            if (pausePanel.activeSelf)
                pausePanel.SetActive(false);
                
            if (!menuPanel.activeSelf)
                menuPanel.SetActive(true);
            
            // 断开连接时显示鼠标
            SetCursorState(true);
        }
    }

    private void OnHost()
    {
        NetworkManager.singleton.StartHost();
        menuPanel.SetActive(false);
        SetCursorState(false);
    }

    private void OnJoin()
    {
        if (!string.IsNullOrEmpty(ipInput.text))
            NetworkManager.singleton.networkAddress = ipInput.text;

        NetworkManager.singleton.StartClient();
        menuPanel.SetActive(false);
        SetCursorState(false);
    }

    private void OnResume()
    {
        // 回到游戏：隐藏暂停菜单
        pausePanel.SetActive(false);
        SetCursorState(false);  // 隐藏并锁定鼠标
    }

    private void OnExitGame()
    {
        // 退出游戏：停止网络连接，显示启动菜单
        if (NetworkServer.active)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.active)
        {
            NetworkManager.singleton.StopClient();
        }
        
        pausePanel.SetActive(false);
        menuPanel.SetActive(true);
        SetCursorState(true);  // 显示并解锁鼠标
    }

    private void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// 设置鼠标状态
    /// </summary>
    /// <param name="visible">true=显示并解锁, false=隐藏并锁定</param>
    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
