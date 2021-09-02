using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    public Image mask;
    float originalSize;
    public static UIHealthBar instance {get; private set; }
    // Start is called before the first frame update
    private void Awake() {
        instance = this;
    }

    // Update is called once per frame
    //maskの高さ取る
    private void Start()
    {
        originalSize = mask.rectTransform.rect.height;
    }

    //アンカーを考慮して高さをへらす
    public void SetValue(float value)
    {
        mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize * value);
    }
}
