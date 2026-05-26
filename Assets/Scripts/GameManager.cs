using System.Collections;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [SyncVar] public float timeRemaining;
    [SyncVar] public bool gameIsOver = false;

    public float roundTime = 180f;
    public int totalCollectibles = 10;

    private Coroutine timerCoroutine;

    private void Start()
    {
        instance = this;
        timeRemaining = roundTime;
        StartCoroutine(InitResultUI());
    }

    private IEnumerator InitResultUI()
    {
        // 等一帧确保所有 Awake/Start 执行完
        yield return null;

        // 用 Resources.FindObjectsOfTypeAll 能找到失活的物体
        ResultUI[] allUI = Resources.FindObjectsOfTypeAll<ResultUI>();
        foreach (ResultUI ui in allUI)
        {
            if (ui.GetComponent<ResultUI>() != null)
            {
                ResultUI.instance = ui;
                ui.Init();
                Debug.Log("[GameManager] ResultUI found and initialized");
                break;
            }
        }

        if (ResultUI.instance == null)
            Debug.LogError("[GameManager] ResultUI not found!");
    }

    public override void OnStartServer()
    {
        StartRound();
    }

    [Server]
    public void StartRound()
    {
        Debug.Log("[GameManager] StartRound called");

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        gameIsOver = false;
        timeRemaining = roundTime;
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        Debug.Log("[GameManager] Timer started: " + timeRemaining);
        while (timeRemaining > 0 && !gameIsOver)
        {
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }
        if (!gameIsOver)
            OnTimeUp();
    }

    [Server]
    private void OnTimeUp()
    {
        Debug.Log("[GameManager] Time's up!");
        gameIsOver = true;

        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            player.Die();
        }

        RpcShowResult("失败", 0);
    }

    [Server]
    public void PlayerExtracted(int count)
    {
        if (gameIsOver) return;
        gameIsOver = true;
        Debug.Log("[GameManager] Player extracted! Sending result...");
        RpcShowResult("成功", count);
    }

    [ClientRpc]
    private void RpcShowResult(string result, int count)
    {
        Debug.Log("[GameManager] RpcShowResult: " + result + " count=" + count);

        if (ResultUI.instance != null)
            ResultUI.instance.ShowResult(result, count);
        else
            Debug.LogError("[GameManager] ResultUI.instance is null!");
    }
}
