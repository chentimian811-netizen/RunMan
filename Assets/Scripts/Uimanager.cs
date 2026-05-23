using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Uimanager : MonoBehaviour
{
    public static Uimanager instance;

    private Text leveText;
    void Start()
    {
        instance = this;
        leveText = GetComponentInChildren<Text>();
    }

    public void SetLevel(int level)
    {
        leveText.text = "Level: " + level;
    }
}
