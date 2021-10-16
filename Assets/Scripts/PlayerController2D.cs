using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerController2D : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    InputAction move, jump, shoot;
    float keyJump,keyHorizontal,keyShoot;
    bool isGrounded;
    bool isShooting;
    bool isJumpKeyPress;
    bool isJumping;
    bool isFacingRight;
    bool keyPress;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform groundCheckL;
    [SerializeField] Transform groundCheckR;
    [SerializeField] private float runSpeed = 1.5f;
    [SerializeField] private float jumpSpeed = 4.0f;
    [SerializeField] int bulletDamage = 1;
    [SerializeField] float bulletSpeed = 5f;
    [SerializeField] float teleportSpeed = -10f;
    [SerializeField] Transform bulletShootPos;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] AudioClip jumpLandedClip;
    [SerializeField] AudioClip shootBulletClip;
    [SerializeField] AudioClip teleportClip;
    [SerializeField] AudioClip takingDamageClip;
    [SerializeField] AudioClip explodeEffectClip;

    [SerializeField] GameObject explodeEffectPrefab;

    public enum TeleportState {Descending, Landed, Idle};
    [SerializeField] TeleportState teleportState;
    float shootTime;
    bool keyShootRelease;
    bool isTakingDamage;
    bool isTeleporting;
    bool isInvincible = false;
    bool hitSideRight;
    public int currentHealth;
    public int maxHealth = 28;

    bool freezeInput;
    bool freezePlayer;
    bool freezeBullets;

    bool GamePad = true;
    RigidbodyConstraints2D rb2dConstraints;

    // void Awake() {
    //     animator = GetComponent<Animator>();
    //     rb2d = GetComponent<Rigidbody2D>();
    //     spriteRenderer = GetComponent<SpriteRenderer>();
    //     var playerInput = GetComponent<PlayerInput>();
    //     move = playerInput.currentActionMap["Move"];
    //     jump = playerInput.currentActionMap["Jump"];
    //     shoot = playerInput.currentActionMap["Shoot"];
    // }
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        var playerInput = GetComponent<PlayerInput>();
        move = playerInput.currentActionMap["Move"];
        jump = playerInput.currentActionMap["Jump"];
        shoot = playerInput.currentActionMap["Shoot"];
        isFacingRight = true;
        currentHealth = maxHealth;

        if (Gamepad.current == null)
        {
            GamePad = false;
        }

#if UNITY_STANDALONE
        GameObject.Find("InputCanvas").SetActive(false);
#endif

    }

    private void Update()
    {
        if (isTeleporting)
        {
            switch (teleportState)
            {
                case TeleportState.Descending:
                    isJumping = false;
                    Invoke("TeleportAnimationStart", 0.2f);
                    //本当は下のコードでState切り替えて実現したかったが、なぜか出来ず。
                    //しゃあないのでInvokeで無理やり実現。
                    // if (isGrounded)
                    // {
                    //     teleportState = TeleportState.Landed;
                    //     Debug.Log("Landed");
                    // }
                    break;
                case TeleportState.Landed:
                    animator.speed = 1;
                    break;
                case TeleportState.Idle:
                    Teleport(false);
                    break;
            }
            return;
        }

        if (isTakingDamage)
        {
            animator.Play("Player_hit");
            return;
        }

        PlayerDebugInput();
        PlayerDirectionInput();
        PlayerShootInput();
        PlayerMovement();
        PlayerJumpInput();
    }

    void PlayerDebugInput()
    {
        if (Keyboard.current[Key.B].wasPressedThisFrame)
        {
            GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
            if (bullets.Length > 0)
            {
                freezeBullets = !freezeBullets;
                foreach (GameObject bullet in bullets)
                {
                    bullet.GetComponent<BulletScript>().FreezeBullet(freezeBullets);
                }
            }
            Debug.Log("Freeze Bullets:" + freezeBullets);
        }

        if (Keyboard.current[Key.E].wasPressedThisFrame)
        {
            Defeat();
            Debug.Log("Defeat()");
        }

        if (Keyboard.current[Key.I].wasPressedThisFrame)
        {
            Invincible(!isInvincible);
            Debug.Log("Invincible:" + isInvincible);
            // spriteRenderer.sortingOrder = 20;
        }

        if (Keyboard.current[Key.K].wasPressedThisFrame)
        {
            FreezeInput(!freezeInput);
            Debug.Log("Freeze Input:" + freezeInput);
        }

        if (Keyboard.current[Key.P].wasPressedThisFrame)
        {
            FreezePlayer(!freezePlayer);
            Debug.Log("Freeze Player:" + freezePlayer);
        }
        if (Keyboard.current[Key.T].wasPressedThisFrame)
        {
            Teleport(true);
            Debug.Log("Teleport(true)");
        }
    }
    void PlayerDirectionInput()
    {
        if (!freezeInput)
        {
            keyHorizontal = move.ReadValue<float>();
        }
    }

    void PlayerJumpInput(){
        if (!freezeInput)
        {
            keyJump = jump.ReadValue<float>();
        }
    }

    void PlayerMovement()
    {
        if (GamePad == true)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                isJumpKeyPress = true;
            }else
            {
                isJumpKeyPress = false;
            }
        }else
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                isJumpKeyPress = true;
            }else
            {
                isJumpKeyPress = false;
            }
        }
        // if (this.keyShoot >= InputSystem.settings.defaultButtonPressPoint)
        // {
        //     isShooting = true;
        // }else
        // {
        //     isShooting = false;
        // }

        if (Physics2D.Linecast(transform.position , groundCheck.position, 1 << LayerMask.NameToLayer("Ground"))||
        Physics2D.Linecast(transform.position , groundCheckL.position, 1 << LayerMask.NameToLayer("Ground"))||
        Physics2D.Linecast(transform.position , groundCheckR.position, 1 << LayerMask.NameToLayer("Ground"))
        )
        {
            isGrounded = true;
            if (isJumping)
            {
                SoundManager.Instance.Play(jumpLandedClip);
                isJumping = false;
            }
        }
        else
        {
            isGrounded = false;
            animator.Play("Player_jump");
        }

        if (this.keyHorizontal > 0f)
        {
            if (!isFacingRight)
            {
                Flip();
            }
            rb2d.velocity = new Vector2(runSpeed,rb2d.velocity.y);
            if (isGrounded)
            {
                if (isShooting)
                {
                    animator.Play("Player_runShoot");
                }else
                {
                    animator.Play("Player_run");
                }
            }
            // spriteRenderer.flipX = false;
        }
        else if(this.keyHorizontal < -0f)
        {
            if (isFacingRight)
            {
                Flip();
            }
            rb2d.velocity = new Vector2(-runSpeed,rb2d.velocity.y);
            if (isGrounded)
            {
                if (isShooting)
                {
                    animator.Play("Player_runShoot");
                }else
                {
                    animator.Play("Player_run");
                }
            }
            // spriteRenderer.flipX = true;
        }
        else
        {
            if (isGrounded)
            {
                if (isShooting)
                {
                    animator.Play("Player_shoot");
                }else
                {
                    animator.Play("Player_idle");
                }
            }
            rb2d.velocity = new Vector2(0,rb2d.velocity.y);
        }

        if (isJumpKeyPress && isGrounded)
        {
            if (isShooting)
            {
                animator.Play("Player_jumpShoot");
            }else
            {
                animator.Play("Player_jump");
            }
            rb2d.velocity = new Vector2(rb2d.velocity.x,jumpSpeed);
        }

        if (!isGrounded)
        {
            // isJumpKeyPress= true;
            isJumping = true;
            if (isShooting)
            {
                animator.Play("Player_jumpShoot");
            }else
            {
                animator.Play("Player_jump");
            }

            // if (rb2d.velocity.y > 0)
            // {
            //     rb2d.AddForce(new Vector2(0, -0.5f), ForceMode2D.Impulse);
            // }
        }

        if (GamePad == true)
        {
            if (rb2d.velocity.y > 2 && (Keyboard.current[Key.Space].wasReleasedThisFrame || Gamepad.current[UnityEngine.InputSystem.LowLevel.GamepadButton.South].wasReleasedThisFrame) == true)
            {
                rb2d.AddForce(new Vector2(0, -1.5f), ForceMode2D.Impulse);
            }
            else if(rb2d.velocity.y > 0 && (Keyboard.current[Key.Space].wasReleasedThisFrame || Gamepad.current[UnityEngine.InputSystem.LowLevel.GamepadButton.South].wasReleasedThisFrame) == true)
            {
                rb2d.AddForce(new Vector2(0, -1.0f), ForceMode2D.Impulse);
            }
        }else
        {
            if (rb2d.velocity.y > 2 && Keyboard.current[Key.Space].wasReleasedThisFrame == true)
            {
                rb2d.AddForce(new Vector2(0, -1.5f), ForceMode2D.Impulse);
            }
            else if(rb2d.velocity.y > 0 && Keyboard.current[Key.Space].wasReleasedThisFrame == true)
            {
                rb2d.AddForce(new Vector2(0, -1.0f), ForceMode2D.Impulse);
            }
        }
    }


    void PlayerShootInput()
    {
        float shootTimeLength = 0;
        float keyShootReleaseTimeLength = 0;
        if (!freezeInput)
        {
            keyShoot = shoot.ReadValue<float>();
            // keyPress = Keyboard.current[Key.Z].wasReleasedThisFrame;
        }

        if (this.keyShoot >= InputSystem.settings.defaultButtonPressPoint)
        {
            keyPress = true;
        }else
        {
            keyPress = false;
        }

        if (keyPress && keyShootRelease)
        {
            isShooting = true;
            keyShootRelease = false;
            shootTime = Time.time;
            //キー押して撃った。弾丸位置調整したので遅延はいらないと判断。後に調整するかも。
            Invoke("ShootBullet",0f);
            // Debug.Log("Shoot Bullet");
        }
        // キー押した状態で撃ってない。つまり間隔開けている状態
        if (!keyPress && !keyShootRelease)
        {
            keyShootReleaseTimeLength = Time.time - shootTime;
            keyShootRelease = true;
        }
        // キー押しっぱ、少ししてfalseにしている
        if (isShooting)
        {
            // InvokeRepeating("ShootBullet", 0.1f, 1.0f);
            shootTimeLength = Time.time - shootTime;
            if (shootTimeLength >= 0.25f || keyShootReleaseTimeLength >= 0.15f)
            {
                isShooting = false;
                // CancelInvoke();
            }
        }
    }
    void Flip()
    {
        // invert facing direction and rotate object 180 degrees on y axis
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void ShootBullet(){
        GameObject bullet = Instantiate(bulletPrefab,bulletShootPos.position,Quaternion.identity);
        bullet.name = bulletPrefab.name;
        bullet.GetComponent<BulletScript>().SetDamageValue(bulletDamage);
        bullet.GetComponent<BulletScript>().SetBulletSpeed(bulletSpeed);
        bullet.GetComponent<BulletScript>().SetBulletDirection((isFacingRight) ? Vector2.right : Vector2.left);
        bullet.GetComponent<BulletScript>().Shoot();
        SoundManager.Instance.Play(shootBulletClip);
    }
    public void HitSide(bool rightSide)
    {
        hitSideRight = rightSide;
    }

    public void Invincible(bool invincibility)
    {

        isInvincible = invincibility;
    }

    public void InvincibleDelay()
    {
        isInvincible = false;
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
            if (currentHealth <= 0)
            {
                Defeat();
            }
            else
            {
                StartDamageAnimation();
            }
        }
    }

    void StartDamageAnimation()
    {
        if (!isTakingDamage)
        {
            keyJump = 0;//これしないとダメージアニメーション終わった後跳ぶ
            isTakingDamage = true;
            Invincible(true);
            FreezeInput(true);
            float hitForceX = 0.5f;
            float hitForceY = 1.5f;
            if (hitSideRight)
            {
                hitForceX = -hitForceX;
            }
            rb2d.velocity = Vector2.zero;
            // rb2d.velocity = new Vector2(hitForceX, hitForceY);
            rb2d.AddForce(new Vector2(hitForceX, hitForceY), ForceMode2D.Impulse);
            SoundManager.Instance.Play(takingDamageClip);
        }
    }

    void StopDamageAnimation()
    {
        isTakingDamage = false;
        //InvincibleDelay関数によって無敵時間を延長したい、本当は点滅させてロックマン動かしたいが
        Invoke("InvincibleDelay", 1f);
        Blink();
        // Invincible(false);
        FreezeInput(false);
        animator.Play("Player_hit", -1, 0f);
    }

    void StartDefeatAnimation()
    {
        FreezeInput(true);
        // FreezePlayer(true);　下の関数に入れたほうがいい気がする……
        GameObject explodeEffect = Instantiate(explodeEffectPrefab);
        explodeEffect.name = explodeEffectPrefab.name;
        explodeEffect.transform.position = spriteRenderer.bounds.center;
        SoundManager.Instance.Play(explodeEffectClip);
        Destroy(gameObject);
    }

    void StopDefeatAnimaition()
    {
        FreezeInput(false);
        FreezePlayer(false);
    }
    void Defeat()
    {
        GameManager.Instance.PlayerDefeated();
        //一度静止した状態で少し待って爆発四散
        Invoke("StartDefeatAnimation", 0.5f);
    }

    public void FreezeInput(bool freeze)
    {
        freezeInput = freeze;
    }

    public void FreezePlayer(bool freeze)
    {
        if (freeze)
        {
            freezePlayer = true;
            animator.speed = 0;
            rb2dConstraints = rb2d.constraints;
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            freezePlayer = false;
            animator.speed = 1;
            rb2d.constraints = rb2dConstraints;
        }
    }

    public void Teleport(bool teleport)
    {
        if (teleport)
        {
            isTeleporting = true;
            FreezeInput(true);
            animator.Play("Player_Teleport");
            animator.speed = 0;
            teleportState = TeleportState.Descending;
            rb2d.velocity = new Vector2(rb2d.velocity.x, teleportSpeed);
        }
        else
        {
            isTeleporting = false;
            FreezeInput(false);
        }
    }

    void TeleportAnimationSound()
    {
        SoundManager.Instance.Play(teleportClip);
    }

    void TeleportAnimationStart()
    {
        teleportState = TeleportState.Landed;
    }
    void TeleportAnimationEnd()
    {
        teleportState = TeleportState.Idle;
    }

    //Blink関数で被ダメ後の無敵時間を点滅で実装する
    void Blink()
    {
        spriteRenderer.color = new Color(1,1,1,1);
        //DOFade、第1引数で透明に、第2引数で1秒間で
        //SetEaseの第2引数は上記の間の点滅回数。偶数にすると元に戻る。奇数だと消えたところで終わる。
        spriteRenderer.DOFade(0, 1f).SetEase(Ease.Flash, 10);
    }
}
