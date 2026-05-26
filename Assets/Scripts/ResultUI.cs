using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ResultUI : MonoBehaviour
{
    public static ResultUI instance;

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite winSprite;
    [SerializeField] private Sprite loseSprite;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackToMenu);
    }

    public void Init()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void ShowResult(string result, int count)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        if (resultImage != null)
        {
            if (result == "成功" && winSprite != null)
                resultImage.sprite = winSprite;
            else if (result != "成功" && loseSprite != null)
                resultImage.sprite = loseSprite;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnBackToMenu()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (NetworkManager.singleton != null)
        {
            if (NetworkServer.active)
                NetworkManager.singleton.StopHost();
            else if (NetworkClient.active)
                NetworkManager.singleton.StopClient();
        }
    }
}
