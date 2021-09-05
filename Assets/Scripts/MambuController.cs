using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MambuController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb2d;
    EnemyController enemyController;

    bool isFacingRight;
    bool isShooting;

    float openTimer;
    float closedTimer;
    float shootTimer;

    public float moveSpeed = 1f;
    public float openDelay = 1f;
    public float closedDelay = 1.5f;
    public float shootDelay = 0.5f;

    public enum MambuState {Closed, Open};
    public MambuState mambuState = MambuState.Closed;

    public enum MoveDirections { Left, Right};
    [SerializeField] MoveDirections moveDirection = MoveDirections.Left;

    bool isEnemyAppear = false;

    // Start is called before the first frame update
    void Start()
    {
        enemyController = GetComponent<EnemyController>();
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();

        isFacingRight = true;
        if (moveDirection == MoveDirections.Left)
        {
            isFacingRight = false;
            enemyController.Flip();
        }

        isShooting = false;

        if (mambuState == MambuState.Open)
        {
            closedTimer = closedDelay;
        }else if (mambuState == MambuState.Closed)
        {
            openTimer = openDelay;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyController.freezeEnemy)
        {
            return;
        }
        float enemyDistance = Camera.main.transform.position.x - transform.position.x;

        if (enemyDistance > 1.8f)
        {
            Destroy(gameObject);
        }
        if (enemyDistance > -2.0f && !isEnemyAppear)
        {
            //画面外で死ぬのを防ぐため
            this.tag = "Enemy";

            //1回だけ呼び出したいので
            isEnemyAppear = true;
        }

        if (isEnemyAppear)
        {
            switch (mambuState)
            {
                case MambuState.Closed:
                    animator.Play("Mambu_Closed");
                    rb2d.velocity = new Vector2(((isFacingRight) ? moveSpeed: - moveSpeed), rb2d.velocity.y);
                    closedTimer -= Time.deltaTime;
                    if (closedTimer < 0)
                    {
                        mambuState = MambuState.Open;
                        openTimer = openDelay;
                        shootTimer = shootDelay;
                    }
                    break;
                case MambuState.Open:
                    animator.Play("Mambu_Open");
                    rb2d.velocity = new Vector2 (0, rb2d.velocity.y);
                    shootTimer -= Time.deltaTime;
                    if (shootTimer < 0 && !isShooting)
                    {
                        ShootBullet();
                        isShooting = true;
                    }
                    openTimer -= Time.deltaTime;
                    if (openTimer < 0)
                    {
                        mambuState = MambuState.Closed;
                        closedTimer = closedDelay;
                        isShooting = false;
                    }
                    break;
            }
        }
    }


    public void SetMoveDirection(MoveDirections direction)
    {
        moveDirection = direction;
        if (moveDirection == MoveDirections.Left)
        {
            if (isFacingRight)
            {
                isFacingRight = !isFacingRight;
                enemyController.Flip();
            }
        }
        else
        {
            if (!isFacingRight)
            {
                isFacingRight = !isFacingRight;
                enemyController.Flip();
            }
        }
    }

    private void ShootBullet()
    {
        GameObject[] bullets = new GameObject[8];
        Vector2[] bulletVectors =
        {
            new Vector2(-1f, 0),
            new Vector2(1f, 0),
            new Vector2(0, -1f),
            new Vector2(0, 1f),
            new Vector2(-0.75f, -0.75f),
            new Vector2(-0.75f, 0.75f),
            new Vector2(0.75f, -0.75f),
            new Vector2(0.75f, 0.75f)
        };

        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i] = Instantiate(enemyController.bulletPrefab);
            bullets[i].name = enemyController.bulletPrefab.name;
            bullets[i].transform.position = enemyController.bulletShootPos.transform.position;
            bullets[i].GetComponent<BulletScript>().SetBulletType(BulletScript.BulletTypes.MiniPink);
            bullets[i].GetComponent<BulletScript>().SetDamageValue(enemyController.bulletDamage);
            bullets[i].GetComponent<BulletScript>().SetBulletSpeed(enemyController.bulletSpeed);
            bullets[i].GetComponent<BulletScript>().SetBulletDirection(bulletVectors[i]);
            bullets[i].GetComponent<BulletScript>().SetCollideWithTag("Player");
            bullets[i].GetComponent<BulletScript>().SetDestroyDelay(5f);
            bullets[i].GetComponent<BulletScript>().Shoot();
        }

        SoundManager.Instance.Play(enemyController.shootBulletClip);
    }

    private void StartInvincibleAnimation()
    {
        enemyController.Invincible(true);
    }

    // Open - beware the Mega Buster!
    private void StopInvincibleAnimation()
    {
        enemyController.Invincible(false);
    }
}
