using HPSocket.Base;
using Mirror;
using System;
using System.Collections;
using UnityEngine;

enum E_State
{
    Patrol,
    Chase,
    Attack,
}
public class EnemyContorller : NetworkBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 720f;

    public float ChaseRange = 10f;

    public float patrolRange = 20f;  // 巡逻半径
    public float attackRange = 1.5f;
    public float AttckCoolDownTimer = 0f;
    public float AttackCoolDownTime = 2f;

    [SyncVar(hook =nameof(OnHealthChanged))]
    public float currentHealth = 0f;

    public float maxHealth = 50f;

    public float deathDuration = 0.5f;

    //public Transform piontA;
    //public Transform piontB;

    public Transform player;
    Vector3 piontA;
    Vector3 piontB;
    Vector3 targetPiont;

    //Transform targetPiont;

    CharacterController characterController;

    private float patrolTimer;          // 巡逻计时器
    private float patrolStuckTime = 5f; // 超过5秒没到就换点

    [SerializeField] private HealthBarController healthBar;

    private Renderer _enemyRenderer;
    private Color _OriginalColor;
    public float falshDuration;

    public GameObject deathParticlePrefab;

    private Animator animator;
    private bool _hasTriggeredAttack;

    [SyncVar]
    E_State currenteState = E_State.Patrol;

    [SyncVar] private bool attackTriggeredSync;
    private bool lastAttackTriggered;
    private float findPlayerTimer = 0f;


    void Start()
    {
        characterController = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();

        // 直接在 Prefab 里拖子物体，或用代码找：
        piontA = transform.position + new Vector3(UnityEngine.Random.Range(-patrolRange, patrolRange), 0, UnityEngine.Random.Range(-patrolRange, patrolRange));
        piontB = transform.position + new Vector3(UnityEngine.Random.Range(-patrolRange, patrolRange), 0, UnityEngine.Random.Range(-patrolRange, patrolRange));
        targetPiont = piontA;


        Invoke(nameof(FindPlayer), 0.5f);  // 延迟等玩家生成
        
        if (isServer)
        {
            currentHealth = maxHealth;
        }

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
        if (isServer)
        {
            findPlayerTimer -= Time.deltaTime;
            if (findPlayerTimer <= 0)
            {
                FindPlayer();
                findPlayerTimer = 1f;
            }
            if (player == null)
            {
                return;
            }

            UpdateState();
        }

        // 双方都执行：根据 state 播放动画
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (!isServer || player == null) return;

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

    private void UpdateAnimation()
    {
        if (animator == null) return;

        switch (currenteState)
        {
            case E_State.Patrol:
                animator.SetFloat("MoveAmount", 0.6f);
                break;
            case E_State.Chase:
                animator.SetFloat("MoveAmount", 1f);
                break;
            case E_State.Attack:
                animator.SetFloat("MoveAmount", 0f);
                if (attackTriggeredSync && !lastAttackTriggered)
                {
                    animator.SetTrigger("Attack");
                }
                lastAttackTriggered = attackTriggeredSync;
                break;
        }
    }

    private void FindPlayer()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (PlayerController p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = p.transform;
            }
        }

        player = closestPlayer;
    }

    private void OnHealthChanged(float oldHealth,float newHealth)
    {
        if(healthBar != null)
        {
            healthBar.UpdateHealth(newHealth, maxHealth);
        }
    }

    private void UpdateState()
    {
        if (currenteState == E_State.Patrol)
        {
            if (Vector3.Distance(transform.position, player.position) < ChaseRange)
            {
                currenteState = E_State.Chase;
            }
        }
        else if (currenteState == E_State.Chase)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist > ChaseRange * 1.2f)
            {
                currenteState = E_State.Patrol;
            }
            else if (dist < attackRange)
            {
                currenteState = E_State.Attack;
                _hasTriggeredAttack = false;
                AttckCoolDownTimer = AttackCoolDownTime;
            }
        }
        else if (currenteState == E_State.Attack)
        {
            if (Vector3.Distance(transform.position, player.position) > attackRange * 1.5f)
            {
                currenteState = E_State.Chase;
            }
            else
            {
                if (!_hasTriggeredAttack)
                {
                    attackTriggeredSync = !attackTriggeredSync;
                    _hasTriggeredAttack = true;
                }

                AttckCoolDownTimer -= Time.deltaTime;
                if (AttckCoolDownTimer <= 0)
                {
                    AttckCoolDownTimer = AttackCoolDownTime;
                    _hasTriggeredAttack = false;
                }
            }
        }
    }


    private void Attack()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDir), rotationSpeed * Time.fixedDeltaTime);
    }

    private void Chase()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        characterController.Move(direction * moveSpeed * Time.fixedDeltaTime);

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDir), rotationSpeed * Time.fixedDeltaTime);
    }

    private void Patrol()
    {
        float distToTarget = Vector3.Distance(targetPiont, transform.position);

        if (distToTarget < 2.5f || patrolTimer > patrolStuckTime)
        {
            targetPiont = transform.position + new Vector3(
                UnityEngine.Random.Range(-patrolRange, patrolRange),
                0,
                UnityEngine.Random.Range(-patrolRange, patrolRange));
            patrolTimer = 0f;
        }

        patrolTimer += Time.fixedDeltaTime;

        Vector3 direction = (targetPiont - transform.position).normalized;
        characterController.Move(moveSpeed * direction * Time.fixedDeltaTime);

        Vector3 lookDir = targetPiont - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDir), rotationSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator FalshRed()
    {
        _enemyRenderer.material.color = Color.red;
        yield return new WaitForSeconds(falshDuration);
        _enemyRenderer.material.color = _OriginalColor;
    }


    internal void TakeDamage(int amount)
    {
        if (!isServer) return;

        currentHealth -= amount;

        RpcFlashRed();
        //Debug.Log("敌人受伤，当前血量:" + currentHealth);

        //if(healthBar != null)
        //{
        //    healthBar.UpdateHealth(currentHealth, maxHealth);
        //}

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [ClientRpc]
    private void RpcFlashRed()
    {
        StartCoroutine(FalshRed());
        animator.SetTrigger("Hurt");
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

        NetworkServer.Destroy(gameObject);
    }

    private void Die()
    {

        if(deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }
        RpcDieEffects();

        StartCoroutine(DeathEffect());
        
    }
    [ClientRpc]
    private void RpcDieEffects()
    {
        animator.SetTrigger("Die");
        animator.SetBool("isDead", true);
    }
    public void OnAttackHit()
    {
        // 只在攻击状态下才造成伤害
        if (currenteState != E_State.Attack) return;
        if (!isServer) return;
        if (player == null) return;

        player.GetComponent<PlayerController>().TakeDamage(10);
        Debug.Log("🗡️ 剑砍到了！");
    }

}
