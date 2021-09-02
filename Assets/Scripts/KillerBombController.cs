using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class KillerBombController : MonoBehaviour
{
    public EnemyController enemyController;
    GameObject player;

    Animator animator;
    Rigidbody2D rb2d;
    bool isFacingRight;
    bool isEnemyAppear = false;

    public enum MoveDirections { Left, Right };
    [SerializeField] MoveDirections moveDirection = MoveDirections.Left;

    public enum KillerBombColors {Blue, Orange, Red};
    [SerializeField] KillerBombColors killerBombColor = KillerBombColors.Blue;
    [SerializeField] RuntimeAnimatorController racKillerBombBlue;
    [SerializeField] RuntimeAnimatorController racKillerBombOrange;
    [SerializeField] RuntimeAnimatorController racKillerBombRed;

    // GameObject killerBomb;


    // Start is called before the first frame update
    public Vector3[] path=
    {
        new Vector3(-2f, 0, 0),
        new Vector3(0, 0, 0),
        new Vector3(-1f, 1f, 0),
        new Vector3(-4f, 0, 0),
        new Vector3(-2f, 0, 0),
        new Vector3(-3f, -1f, 0),
    };

    public float flyTime = 4f;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        enemyController = GetComponent<EnemyController>();
        animator = enemyController.GetComponent<Animator>();
        spriteRenderer = enemyController.GetComponent<SpriteRenderer>();
        // spriteRenderer.sortingOrder = 0;
        this.tag = "Untagged";

        //スプライトが右向いてる場合に左向けさす
        isFacingRight = true;
        if (moveDirection == MoveDirections.Left)
        {
            isFacingRight = false;
            enemyController.Flip();
        }

         SetColor(killerBombColor);
    }


    void Update()
    {
        animator.Play("KillerBomb_Flying");

        //カメラに入る前のタイミングで動き出し画面外に飛んでいったらDestoroy
        float enemyDistance = Camera.main.transform.position.x - transform.position.x;

        if (enemyDistance > 1.8f)
        {
            Destroy(gameObject);
        }
        if (enemyDistance > -2.0f && !isEnemyAppear)
        {
            //画面外で死ぬのを防ぐため
            // spriteRenderer.sortingOrder = 10;
            this.tag = "Enemy";
            //1回だけ呼び出したいので
            isEnemyAppear = true;
            EnemybezierFly(path, flyTime);
        }
    }

    //この関数でDoTweenを呼び出す
    public void EnemybezierFly(Vector3[] path, float flyTime)
    {
        this.gameObject.transform.DOLocalPath(path, flyTime, PathType.CubicBezier)
                    .SetRelative()
                    .SetLoops(4, LoopType.Incremental)
                    .SetEase(Ease.Linear)
                    .SetLink(gameObject);
    }

    // public void EnemystraightFly(Vector3[] path, float flyTime)
    // {
    //     this.gameObject.transform.DOLocalPath(path, flyTime, PathType.Linear)
    //                 .SetRelative()
    //                 .SetLoops(4, LoopType.Incremental)
    //                 .SetEase(Ease.Linear)
    //                 .SetLink(gameObject);
    // }


    public void SetColor(KillerBombColors color)
    {
        killerBombColor = color;
        SetAnimationController();
    }

    public void SetAnimationController()
    {
        switch (killerBombColor)
        {
            case KillerBombColors.Blue:
            animator.runtimeAnimatorController = racKillerBombBlue;
            break;
            case KillerBombColors.Orange:
            animator.runtimeAnimatorController = racKillerBombOrange;
            break;
            case KillerBombColors.Red:
            animator.runtimeAnimatorController = racKillerBombRed;
            break;
        }
    }
}