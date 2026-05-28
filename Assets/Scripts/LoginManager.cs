using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField accountInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginBtn;

    private void Start()
    {
        loginBtn.onClick.AddListener(OnLogin);
    }

    private void OnLogin()
    {
        string account = accountInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("请输入账号和密码");
            return;
        }

        PlayerPrefs.SetString("PlayerAccount", account);
        PlayerPrefs.SetString("PlayerPassword", password);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Demo6");
    }
}
