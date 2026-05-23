using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    private Vector3 _originalPos;

    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.15f;

    private bool _isShaking;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        _originalPos = transform.localPosition;
    }

    private IEnumerator Shake()
    {
        _isShaking = true;

        float elapsed = 0;
        while(elapsed < shakeDuration)
        {
            float t = elapsed / shakeDuration;
            float currentIntensity = Mathf.Lerp(shakeIntensity, 0, t);
            transform.localPosition = _originalPos + Random.insideUnitSphere * currentIntensity;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _originalPos;

        _isShaking = false;
    }

    public void TriggerShake()
    {
        if (!_isShaking)
        {
            StartCoroutine(Shake());
        }
        
    }
}
