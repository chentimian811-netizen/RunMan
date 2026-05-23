using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    public float jumpHeight = 3f;

    public float velocity = 0f;

    public float gravity = -9.8f;

    public float senstivity = 3f;

    public float xRotatetion;

    public float attackCooldownTimer = 0f;

    public float attackCooldownTime = 1.5f;

    float jumpBufferTimer = 0f;

    float jumpBufferTime = 0.3f;

    public Vector3 spawnPostion;

    public float currentHealth = 0;
    public float maxHealth = 100f;
    CharacterController characterController;

    public Transform canmeraHolder;

    public float deathDuration = 0.5f;

    private Renderer _playerRenderer;
    private Color _OriginalColor;
    public float falshDuration;

    [SerializeField] private HealthBarController healthBar;

    public GameObject deathParticlePrefab;
    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        _playerRenderer = GetComponentInChildren<Renderer>();
        _OriginalColor = _playerRenderer.material.color;

        spawnPostion = transform.position;
    }

    private void Update()
    {
        attackCooldownTimer -= Time.deltaTime;

        PlayerMove();

        PlCameraRotate();

        PlayerAttack();
    }

    private void PlayerAttack()
    {
        if (Input.GetMouseButtonDown(0) && attackCooldownTimer <= 0)
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

            attackCooldownTimer = attackCooldownTime;
        }
    }

    private void PlCameraRotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up, mouseX * senstivity);

        xRotatetion -= mouseY * senstivity;
        xRotatetion = Mathf.Clamp(xRotatetion, -80f, 80f);
        canmeraHolder.localRotation = Quaternion.Euler(xRotatetion, 0, 0);

    }

    private void PlayerMove()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        move = move * speed * Time.deltaTime;

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
        currentHealth -= amount;
        Debug.Log("Íæ¼ÒÊÜÉË,Ê£ÓàÑªÁ¿" + currentHealth);

        CameraShake.instance?.TriggerShake();

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
            StartCoroutine(FalshRed());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DeathEffect()
    {
        float elapsed = 0;
        Vector3 originalScale = transform.localScale;

        while (elapsed < deathDuration)
        {
            float t = elapsed / deathDuration;

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            Color color = _playerRenderer.material.color;
            color.a = Mathf.Lerp(1, 0, t);
            _playerRenderer.material.color = color;

            elapsed += Time.deltaTime;
            yield return null;
        }
        //Destroy(gameObject);
    }

    private void Die()
    {
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }
        StartCoroutine(DeathEffect());
        Debug.Log("Íæ¼ÒËÀÍö");
    }

    public void SetVelocity(float jumpForce)
    {
        velocity = Mathf.Sqrt(gravity * -2f * jumpHeight);
    }

    public void ResetSpawn()
    {
        characterController.enabled = false;
        transform.position = spawnPostion;
        characterController.enabled = true;

        currentHealth = maxHealth;
    }
}
