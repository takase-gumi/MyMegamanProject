using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    Animator animator;
    BoxCollider2D box2d;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    public bool isInvincible;

    GameObject explodeEffect;

    RigidbodyConstraints2D rb2dConstraints;

    public bool freezeEnemy;
    public int scorePoints = 500;
    public int currentHealth;
    public int maxHealth = 1;
    public int contactDamage = 1;
    public int explosionDamage = 0;
    public int bulletDamage = 1;
    public float bulletSpeed = 3f;
    public AudioClip damageClip;
    public AudioClip blockAttackClip;
    public AudioClip shootBulletClip;
    public GameObject bulletShootPos;
    public GameObject bulletPrefab;
    public GameObject explodeEffectPrefab;

    // MambuController mambuController;
    GameObject player;

    void Start()
    {
        animator = GetComponent<Animator>();
        box2d = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;

        // mambuController = GetComponent<MambuController>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void Flip()
    {
        transform.Rotate(0, 180f, 0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Invincible(bool invincibility)
    {
        isInvincible = invincibility;
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            SoundManager.Instance.Play(damageClip);
            if (currentHealth <= 0)
            {
                Defeat();
            }
        }
        else
        {
            //無敵のときは反射
            ShootBulletReflection(player);
            // SoundManager.Instance.Play(blockAttackClip);
        }
    }

    void StartDefeatAnimation()
    {
        explodeEffect = Instantiate(explodeEffectPrefab);
        explodeEffect.name = explodeEffectPrefab.name;
        explodeEffect.transform.position = spriteRenderer.bounds.center;
        explodeEffect.GetComponent<ExplosionScript>().SetDamageValue(this.explosionDamage);
        Destroy(explodeEffect, 2f);
    }

    void StopDefeatAnimaition()
    {
        Destroy(explodeEffect);
    }
    void Defeat()
    {
        StartDefeatAnimation();
        Destroy(gameObject);
        GameManager.Instance.AddScorePoints(this.scorePoints);
    }

    public void FreezeEnemy(bool freeze)
    {
        if (freeze)
        {
            freezeEnemy = true;
            animator.speed = 0;
            rb2dConstraints = rb2d.constraints;
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            freezeEnemy = false;
            animator.speed = 1;
            rb2d.constraints = rb2dConstraints;
        }
    }

    //ここの関数で反射を実装
    public void ShootBulletReflection(GameObject player){
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.name = bulletPrefab.name;
        bullet.transform.position = bulletShootPos.transform.position;
        bullet.GetComponent<BulletScript>().SetBulletType(BulletScript.BulletTypes.Default);
        bullet.GetComponent<BulletScript>().SetDamageValue(bulletDamage);
        bullet.GetComponent<BulletScript>().SetBulletSpeed(bulletSpeed);
        bullet.GetComponent<BulletScript>().SetBulletDirection((player.transform.position.x - transform.position.x < 0) ? new Vector2(-1, 1) : new Vector2(1, 1));
        bullet.GetComponent<BulletScript>().SetCollideWithTag("Player");
        bullet.GetComponent<BulletScript>().SetDestroyDelay(5f);
        bullet.GetComponent<BulletScript>().Shoot();
        SoundManager.Instance.Play(blockAttackClip);
        // SoundManager.Instance.Play(enemyController.shootBulletClip);
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Debug.Log("Player Hit");
            PlayerController2D player = other.gameObject.GetComponent<PlayerController2D>();
            player.HitSide(transform.position.x > player.transform.position.x);
            player.TakeDamage(this.contactDamage);
        }
    }

    //OnCollisionEnterで衝突座標を取得して跳ね返したい
}
