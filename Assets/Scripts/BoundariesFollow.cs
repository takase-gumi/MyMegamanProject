using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundariesFollow : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float timeOffset;
    [SerializeField] Vector3 offsetPos;
    [SerializeField] Vector3 boundsMin;
    [SerializeField] Vector3 boundsMax;
    public float wallLeftPos = 1.5f;
    Transform mainCamObj;

    void Start() {
        mainCamObj = Camera.main.gameObject.transform;
    }

    private void LateUpdate() {
        if (player != null)
        {
            Vector3 startPos = transform.position;
            // Vector3 targetPos = player.position;
            // Vector3 mainCamPos = mainCamObj.position;
            if (player.position.x - transform.position.x > wallLeftPos)
            {
                transform.position = new Vector3 (mainCamObj.position.x - wallLeftPos, 0.24f ,0);
            }
            // Vector3 startPos = transform.position;
            // Vector3 targetPos = player.position;

            // targetPos.x += offsetPos.x;
            // targetPos.y += offsetPos.y;
            // targetPos.z = transform.position.z;

            // targetPos.x = Mathf.Clamp(targetPos.x -2f,boundsMin.x,boundsMax.x);
            // targetPos.y = Mathf.Clamp(targetPos.y,boundsMin.y,boundsMax.y);

            // float t = 1f - Mathf.Pow(1f - timeOffset, Time.deltaTime * 30);
            // transform.position = Vector3.Lerp(startPos, targetPos, t);

            // 壁に似たようなのつけて、カメラが追従はするが戻らないようにしたい
            //ロックマンが右に動いたときだけ右に動けばいいか＞
        }
    }
}
