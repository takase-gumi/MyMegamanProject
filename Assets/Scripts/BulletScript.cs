using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    Animator animator;

    bool freezeBullet;
    RigidbodyConstraints2D rb2dConstraints;

    float destroyTime;
    public int damage = 1;
    [SerializeField] float bulletSpeed;
    [SerializeField] Vector2 bulletDirection;
    [SerializeField] float destroyDelay;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(freezeBullet) return;

        destroyTime -= Time.deltaTime;
        if (destroyTime < 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetBulletSpeed(float speed)
    {
        this.bulletSpeed = speed;
    }

    public void SetBulletDirection(Vector2 direction)
    {
        this.bulletDirection = direction;
    }

    public void SetDamageValue(int damage)
    {
        this.damage = damage;
    }

    public void SetDestroyDelay(float delay)
    {
        this.destroyDelay = delay;
    }

    public void Shoot()
    {
        spriteRenderer.flipX = (bulletDirection.x < 0);
        rb2d.velocity = bulletDirection * bulletSpeed;
        destroyTime = destroyDelay;
    }

    public void FreezeBullet(bool freeze)
    {
        if (freeze)
        {
            freezeBullet = true;
            rb2dConstraints = rb2d.constraints;
            animator.speed = 0;
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            rb2d.velocity = Vector2.zero;
        }
        else
        {
            freezeBullet = false;
            animator.speed = 1;
            rb2d.constraints = rb2dConstraints;
            rb2d.velocity = Vector2.zero;
            rb2d.velocity = bulletDirection * bulletSpeed;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(this.damage);
            }
            Destroy(gameObject,0.03f);
        }
    }
}