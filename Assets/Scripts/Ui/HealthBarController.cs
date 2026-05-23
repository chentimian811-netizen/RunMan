using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Slider slider;

    [SerializeField] private float lerpSpeed = 5f;

    private float currentFill;
    private float targetFill;


    // Start is called before the first frame update
    void Start()
    {
        slider.value = 1f;
        slider.maxValue = 1f;
        currentFill = 1f;
        targetFill = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if(Mathf.Abs(currentFill - targetFill) > 0.0001f)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, lerpSpeed * Time.deltaTime);
            slider.value = currentFill;
            UpdateColor();
        }
    }

    public void UpdateHealth(float currentHP,float maxHP)
    {
        targetFill = currentHP / maxHP;
    }

    private void UpdateColor()
    {
        if(currentFill > 0.5f)
        {
            float t = (currentFill - 0.5f) * 2f;
            fillImage.color = Color.Lerp(Color.yellow, Color.green, t);
        }
        else
        {
            float t = currentFill * 2f;
            fillImage.color = Color.Lerp(Color.red, Color.yellow, t);
        }
    }
}
