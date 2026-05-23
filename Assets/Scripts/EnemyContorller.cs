using System;
using System.Collections;
using System.Collections.Generic;
//using System.Drawing;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;

enum E_State
{
    Patrol,
    Chase,
    Attack,
}
public class EnemyContorller : MonoBehaviour
{
    public float moveSpeed = 3f;

    public float ChaseRange = 10f;

    public float attackRange = 1.5f;
    public float AttckCoolDownTimer = 0f;
    public float AttackCoolDownTime = 2f;

    public float currentHealth = 0f;

    public float maxHealth = 50f;

    public float deathDuration = 0.5f;

    public Transform piontA;
    public Transform piontB;

    public Transform player;

    Transform targetPiont;

    CharacterController characterController;

    [SerializeField] private HealthBarController healthBar;

    private Renderer _enemyRenderer;
    private Color _OriginalColor;
    public float falshDuration;

    public GameObject deathParticlePrefab;

    E_State currenteState = E_State.Patrol;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        targetPiont = piontA;

        currentHealth = maxHealth;

        if(healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        _enemyRenderer = GetComponentInChildren<Renderer>();
        _OriginalColor = _enemyRenderer.material.color;

    }

    // Update is called once per frame
    void Update()
    {
        if (currenteState == E_State.Patrol)
        {
            Patrol();
        }
        else if (currenteState == E_State.Chase)
        {
            Chase();
        }
        else if (currenteState == E_State.Attack)
        {
            Attack();
        }

    }

    private void Attack()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        if (Vector3.Distance(transform.position, player.position) > ChaseRange)
        {
            currenteState = E_State.Chase;
            return;
        }


        AttckCoolDownTimer -= Time.deltaTime;

        if(AttckCoolDownTimer <= 0)
        {
            player.GetComponent<PlayerController>().TakeDamage(10);
            AttckCoolDownTimer = AttackCoolDownTime;
        } 
    }

    private void Chase()
    {
        if (Vector3.Distance(player.position, transform.position) > ChaseRange * 1.2f)
        {
            currenteState = E_State.Patrol;
            return;
        }
        else if (Vector3.Distance(player.position, transform.position) < attackRange )
        {
            currenteState = E_State.Attack;

            AttckCoolDownTimer = AttackCoolDownTime;
        }

        Vector3 direction = (player.position - transform.position).normalized;

        characterController.Move(direction * moveSpeed * Time.deltaTime);

        Vector3 lookDir = player.position - transform.position;

        lookDir.y = 0;

        transform.rotation = Quaternion.LookRotation(lookDir);

    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, player.position) < ChaseRange)
        {
            currenteState = E_State.Chase;
            return;
        }

        if (Vector3.Distance(targetPiont.position, transform.position) < 1.5f)
        {
            if(targetPiont == piontA)
            {
                targetPiont = piontB;
            }
            else
            {
                targetPiont = piontA;
            }
        }

        Vector3 direction = (targetPiont.position - transform.position).normalized;

        characterController.Move(moveSpeed * direction * Time.deltaTime);

        Vector3 lookDir = targetPiont.position - transform.position;

        lookDir.y = 0;

        transform.rotation = Quaternion.LookRotation(lookDir);

    }

    private IEnumerator FalshRed()
    {
        _enemyRenderer.material.color = Color.red;
        yield return new WaitForSeconds(falshDuration);
        _enemyRenderer.material.color = _OriginalColor;
    }


    internal void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("敌人受伤，当前血量:" + currentHealth);
        if(healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
            StartCoroutine(FalshRed());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DeathEffect()//敌人缩小死亡效果
    {
        float elapsed = 0;
        Vector3 originalScale = transform.localScale;

        while(elapsed < deathDuration)
        {
            float t = elapsed / deathDuration;

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            Color color = _enemyRenderer.material.color;
            color.a = Mathf.Lerp(1, 0, t);
            _enemyRenderer.material.color = color;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void Die()
    {
        if(deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }
        StartCoroutine(DeathEffect());
    }
}
