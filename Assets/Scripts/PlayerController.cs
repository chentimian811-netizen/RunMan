using System.Collections;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;

    public float jumpHeight = 3f;

    public float velocity = 0f;

    public float gravity = -9.8f;

    public float attackCooldownTimer = 0f;

    public float attackCooldownTime = 1.5f;

    float jumpBufferTimer = 0f;

    float jumpBufferTime = 0.3f;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float currentHealth = 0;
    public float maxHealth = 100f;


    CharacterController characterController;

    public float deathDuration = 0.5f;

    private Renderer _playerRenderer;
    private Color _OriginalColor;
    public float falshDuration;

    private Animator animator;

    [SerializeField] private HealthBarController healthBar;

    public int collectibles = 0;

    public GameObject deathParticlePrefab;
    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (isServer)
        {
            currentHealth = maxHealth; 
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        _playerRenderer = GetComponentInChildren<Renderer>();
        _OriginalColor = _playerRenderer.material.color;

        animator = GetComponentInChildren<Animator>();


        if (!isLocalPlayer)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if(cam != null)
            {
                cam.enabled = false;
            }
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false;
            }
        }
        else
        {
            Transform holder = transform.Find("CameraHolder");
            if (holder != null && holder.GetComponent<ThirdPersonCameraController>() == null)
            {
                holder.gameObject.AddComponent<ThirdPersonCameraController>();
            }
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        // 菜单打开时，冻结玩家操作
        if (MenuController.instance != null && MenuController.instance.IsMenuOpen)
            return;

        attackCooldownTimer -= Time.deltaTime;

        PlayerMove();

        PlayerAttack();
    }

    private void OnHealthChanged(float oldHealth,float newHealth)
    {
        if(healthBar != null)
        {
            healthBar.UpdateHealth(newHealth, maxHealth);
        }
    }
    private void PlayerAttack()
    {
        if(Input.GetMouseButtonDown(0) && attackCooldownTimer <= 0)
        {
            CmdAttack();
            attackCooldownTimer = attackCooldownTime;
        }
    }

    [Command]
    private void CmdAttack()
    {
        Collider[] collider = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, 2f);

        for (int i = 0; i < collider.Length; i++)
        {
            EnemyContorller enemy = collider[i].GetComponent<EnemyContorller>();
            if (enemy != null)
            {
                enemy.TakeDamage(10);
            }
        }
    }


    private void PlayerMove()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        move = move * speed * Time.deltaTime;

        if (characterController.isGrounded)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector3.down, out RaycastHit hit, 1f))
            {
                move = Vector3.ProjectOnPlane(move, hit.normal);
            }
        }

        velocity += gravity * Time.deltaTime;

        move.y = velocity * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0 && characterController.isGrounded)
        {
            velocity = Mathf.Sqrt(gravity * -2f * jumpHeight);
            jumpBufferTimer = 0f;
        }
        else if (characterController.isGrounded && velocity < 0)
        {
            velocity = -2f;
        }

        if(animator != null)
        {
            animator.SetFloat("Speed", z);

            if(z > 0.01f)
            {
                float angle = Mathf.Atan2(x, z) * Mathf.Rad2Deg;
                animator.SetFloat("Direction", angle / 180f);
            }

            animator.SetBool("Jump", !characterController.isGrounded);
        }


        characterController.Move(move);

    }

    private IEnumerator FalshRed()
    {
        _playerRenderer.material.color = Color.red;
        yield return new WaitForSeconds(falshDuration);
        _playerRenderer.material.color = _OriginalColor;
    }


    public void TakeDamage(int amount)
    {
        if (!isServer) return;
        currentHealth -= amount;
        Debug.Log("�������,ʣ��Ѫ��" + currentHealth);

        RpcOnHit();

        //if(healthBar != null)
        //{
        //    healthBar.UpdateHealth(currentHealth, maxHealth);
            
        //}

        if(currentHealth <= 0)
        {
            Die();
        }
    }
    [ClientRpc]
    private void RpcOnHit()
    {
        if (!isOwned) return;  // 只让被打的玩家本地震动

        CameraShake shake = Camera.main.GetComponentInChildren<CameraShake>();
        if (shake != null) shake.TriggerShake();
        StartCoroutine(FalshRed());
    }

    private IEnumerator DeathEffect()
    {
        float elapsed = 0;
        Vector3 originalScale = transform.localScale;

        while(elapsed < deathDuration)
        {
            float t = elapsed / deathDuration;

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            Color color = _playerRenderer.material.color;
            color.a = Mathf.Lerp(1, 0, t);
            _playerRenderer.material.color = color;

            elapsed += Time.deltaTime;
            yield return null;
        }
        NetworkServer.Destroy(gameObject);
    }

    public void Die()
    {
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }
        StartCoroutine(DeathEffect());
        Debug.Log("�������");
    }
}
