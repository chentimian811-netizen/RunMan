using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private EnemyContorller _enemy;
    // Start is called before the first frame update
    void Awake()
    {
        _enemy = GetComponentInParent<EnemyContorller>();
    }

    public void OnAttackHit()
    {
        if (_enemy != null)
            _enemy.OnAttackHit();
    }
}
