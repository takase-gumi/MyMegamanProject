using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PepeController : MonoBehaviour
{
    public EnemyController enemyController;
    GameObject player;

    Animator animator;
    Rigidbody2D rb2d;
    bool isFacingRight;
    bool isEnemyAppear = false;

    public enum MoveDirections { Left, Right };
    [SerializeField] MoveDirections moveDirection = MoveDirections.Left;
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
        // Vector3[] Pepenobasho = path;
        enemyController = GetComponent<EnemyController>();
        animator = enemyController.GetComponent<Animator>();
        spriteRenderer = enemyController.GetComponent<SpriteRenderer>();
        this.tag = "Untagged";


        // GameObject Camera = GetComponent<Camera>();
        // GameObject isCamera = GameObject.FindGameObjectWithTag("MainCamera");


        // Vector3[] path =
        // {
        //     new Vector3(-2f, 0, 0),
        //     new Vector3(0, 0, 0),
        //     new Vector3(-0.5f, 1f, 0),
        //     new Vector3(-4f, 0, 0),
        //     new Vector3(-2f, 0, 0),
        //     new Vector3(-3.5f, -1f, 0),
        // };

        // this.gameObject.transform.DOLocalPath(path, 6f,PathType.CubicBezier)
        //             .SetDelay(3f)
        //             .SetRelative()
        //             .SetLoops(4, LoopType.Incremental)
        //             .SetEase(Ease.Linear)
        //             .SetLink(gameObject);

        //スプライトが右向いてる場合に左向けさす
        isFacingRight = true;
        if (moveDirection == MoveDirections.Left)
        {
            isFacingRight = false;
            enemyController.Flip();
        }
    }


    void Update()
    {
        animator.Play("Pepe_Flying");

        //カメラに入る前のタイミングで動き出し画面外に飛んでいったらDestoroy
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

}
