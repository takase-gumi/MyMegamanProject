using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    Animator animator;
    CircleCollider2D cc2d;

    bool freezeBullet;
    RigidbodyConstraints2D rb2dConstraints;

    float destroyTime;
    public int damage = 1;
    [SerializeField] float bulletSpeed;
    [SerializeField] Vector2 bulletDirection;
    [SerializeField] float destroyDelay;
    [SerializeField] string[] collideWithTags = {"Enemy"};
    public enum BulletTypes {Default, MiniBlue, MiniGreen, MiniOrange, MiniPink, MiniRed};
    [SerializeField] BulletTypes bulletType = BulletTypes.Default;

    [System.Serializable]
    public struct BulletStruct
    {
        public Sprite sprite;
        public float radius;
        public Vector3 scale;
    }

    [SerializeField] BulletStruct[] bulletData;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        cc2d = GetComponent<CircleCollider2D>();

        SetBulletType(bulletType);
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

    public void SetBulletType(BulletTypes type)
    {
        spriteRenderer.sprite = bulletData[(int)type].sprite;
        cc2d.radius = bulletData[(int)type].radius;
        transform.localScale = bulletData[(int)type].scale;
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

    //引数の個数を自動で伸び縮み(可変長引数)の書き方らしい。今ん所Player,Enemyだけだが。
    public void SetCollideWithTag(params string[] tags)
    {
        this.collideWithTags = tags;
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
        foreach (string tag in collideWithTags)
        {
            if (other.gameObject.CompareTag(tag))
            {
                switch (tag)
                {
                    case "Enemy":
                        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
                        if (enemy != null)
                        {
                            enemy.TakeDamage(this.damage);
                        }
                        Destroy(gameObject,0.03f);
                        break;
                    case "Player":
                        PlayerController2D player = other.gameObject.GetComponent<PlayerController2D>();
                        if (player != null)
                        {
                            player.HitSide(transform.position.x > player.transform.position.x);
                            player.TakeDamage(this.damage);
                        }
                        break;
                }
                //すぐに弾丸が消えると変なかんじになるため
                Destroy(gameObject,0.03f);
            }
        }
    }
}
