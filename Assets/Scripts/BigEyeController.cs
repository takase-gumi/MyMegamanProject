using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigEyeController : MonoBehaviour
{
    Animator animator;
    BoxCollider2D box2d;
    Rigidbody2D rb2d;
    EnemyController enemyController;

    bool isFacingRight;
    bool isGrounded;
    bool isJumping;

    float jumpTimer;
    float jumpDelay = 0.25f;

    int jumpPatternIndex;
    int[] jumpPattern;
    int[][] jumpPatterns = new int[][] {
        new int[1] { 1 },           // High Jump
        new int[2] { 0, 1 },        // Low Jump, High Jump
        new int[3] { 0, 0, 1 }      // Low Jump, Low Jump, High Jump
    };

    int jumpVelocityIndex;
    Vector2 jumpVelocity;
    Vector2[] jumpVelocities = {
        new Vector2(1.0f, 3.0f),    // Low Jump
        new Vector2(0.75f, 4.0f)    // High Jump
    };

    public AudioClip jumpLandedClip;

    public enum BigEyeColors { Blue, Orange, Red };
    [SerializeField] BigEyeColors bigEyeColor = BigEyeColors.Blue;

    [SerializeField] RuntimeAnimatorController racBigEyeBlue;
    [SerializeField] RuntimeAnimatorController racBigEyeOrange;
    [SerializeField] RuntimeAnimatorController racBigEyeRed;

    public enum MoveDirections { Left, Right };
    [SerializeField] MoveDirections moveDirection = MoveDirections.Left;
    bool isEnemyAppear = false;

    void Awake() {
        // get components from EnemyController
        enemyController = GetComponent<EnemyController>();
        animator = enemyController.GetComponent<Animator>();
        box2d = enemyController.GetComponent<BoxCollider2D>();
        rb2d = enemyController.GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.tag = "Untagged";
        
        // sprite sheet images face right
        // switch to facing left if it's set
        isFacingRight = true;
        if (moveDirection == MoveDirections.Left)
        {
            isFacingRight = false;
            enemyController.Flip();
        }

        // set big eye color of choice
        SetColor(bigEyeColor);

        // start with no pattern
        jumpPattern = null;
    }

    void FixedUpdate()
    {
        isGrounded = false;
        Color raycastColor;
        RaycastHit2D raycastHit;
        float raycastDistance = 0.05f;
        int layerMask = 1 << LayerMask.NameToLayer("Ground");
        // ground check
        Vector3 box_origin = box2d.bounds.center;
        box_origin.y = box2d.bounds.min.y + (box2d.bounds.extents.y / 4f);
        Vector3 box_size = box2d.bounds.size;
        box_size.y = box2d.bounds.size.y / 4f;
        raycastHit = Physics2D.BoxCast(box_origin, box_size, 0f, Vector2.down, raycastDistance, layerMask);
        // big eye box colliding with ground layer
        if (raycastHit.collider != null)
        {
            isGrounded = true;
            // just landed from jumping/falling
            if (isJumping)
            {
                isJumping = false;
                SoundManager.Instance.Play(jumpLandedClip);
            }
        }
        // draw debug lines
        raycastColor = (isGrounded) ? Color.green : Color.red;
        Debug.DrawRay(box_origin + new Vector3(box2d.bounds.extents.x, 0), Vector2.down * (box2d.bounds.extents.y / 4f + raycastDistance), raycastColor);
        Debug.DrawRay(box_origin - new Vector3(box2d.bounds.extents.x, 0), Vector2.down * (box2d.bounds.extents.y / 4f + raycastDistance), raycastColor);
        Debug.DrawRay(box_origin - new Vector3(box2d.bounds.extents.x, box2d.bounds.extents.y / 4f + raycastDistance), Vector2.right * (box2d.bounds.extents.x * 2), raycastColor);
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyController.freezeEnemy)
        {
            // add anything here to happen while frozen i.e. time compensations
            return;
        }

        float enemyDistance = Camera.main.transform.position.x - transform.position.x;

        if (enemyDistance > 2.5f)
        {
            Destroy(gameObject);
        }
        if (enemyDistance > -2.5f && !isEnemyAppear)
        {
            //画面外で死ぬのを防ぐため
            this.tag = "Enemy";

            //1回だけ呼び出したいので
            isEnemyAppear = true;
        }

        // get player object - used for jumping direction
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (isEnemyAppear)
        {
            if (isGrounded)
            {
                animator.Play("BigEye_Grounded");
                rb2d.velocity = new Vector2(0, rb2d.velocity.y);
                jumpTimer -= Time.deltaTime;
                if (jumpTimer < 0)
                {
                    if (jumpPattern == null)
                    {
                        jumpPatternIndex = 0;
                        jumpPattern = jumpPatterns[Random.Range(0, jumpPatterns.Length)];
                    }
                    jumpVelocityIndex = jumpPattern[jumpPatternIndex];
                    jumpVelocity = jumpVelocities[jumpVelocityIndex];
                    if (player.transform.position.x <= transform.position.x)
                    {
                        jumpVelocity.x *= -1;
                    }
                    rb2d.velocity = new Vector2(rb2d.velocity.x, jumpVelocity.y);
                    jumpTimer = jumpDelay;
                    if (++jumpPatternIndex > jumpPattern.Length - 1)
                    {
                        jumpPattern = null;
                    }
                }
            }
            else
            {
                animator.Play("BigEye_Jumping");
                rb2d.velocity = new Vector2(jumpVelocity.x, rb2d.velocity.y);
                isJumping = true;
                if (jumpVelocity.x <= 0)
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
        }
    }

    public void SetColor(BigEyeColors color)
    {
        bigEyeColor = color;
        SetAnimatorController();
    }

    void SetAnimatorController()
    {
        // set animator control from color
        switch (bigEyeColor)
        {
            case BigEyeColors.Blue:
                animator.runtimeAnimatorController = racBigEyeBlue;
                break;
            case BigEyeColors.Orange:
                animator.runtimeAnimatorController = racBigEyeOrange;
                break;
            case BigEyeColors.Red:
                animator.runtimeAnimatorController = racBigEyeRed;
                break;
        }
    }
}