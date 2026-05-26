using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI collectCountText;

    private PlayerController localPlayer;

    void Update()
    {
        if (localPlayer == null)
        {
            FindLocalPlayer();
        }

        UpdateTimer();
        UpdateCollectCount();
    }

    private void FindLocalPlayer()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController p in players)
        {
            if (p.isLocalPlayer)
            {
                localPlayer = p;
                break;
            }
        }
    }

    private void UpdateTimer()
    {
        if (GameManager.instance == null) return;
        if (timerText == null) return;

        float t = GameManager.instance.timeRemaining;
        if (t < 0) t = 0;
        int minutes = Mathf.FloorToInt(t / 60);
        int seconds = Mathf.FloorToInt(t % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateCollectCount()
    {
        if (localPlayer == null) return;
        if (collectCountText == null) return;

        int total = GameManager.instance != null ? GameManager.instance.totalCollectibles : 0;
        collectCountText.text = "Items: " + localPlayer.collectibles + " / " + total;
    }
}
