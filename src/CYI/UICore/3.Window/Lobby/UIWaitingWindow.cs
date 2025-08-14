using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIWaitingWindow : UIBase
{
    [SerializeField] private RectTransform rectTrIcon;
    [SerializeField] private Image imgIcon;
    private List<string> iconAdrList = new();

    protected override void Reset()
    {
        base.Reset();
        imgIcon = transform.FindChildByName<Image>("Img_Icon");
        rectTrIcon = imgIcon.transform.GetComponent<RectTransform>();
    }

    public override void Initialize()
    {
        base.Initialize();
        iconAdrList = StringAdrIcon.UnitIconDict.Values.ToList();
    }

    public override void Open(OpenContext openContext = null)
    {
        canvasGroup.SetInteractable(true);
        canvasGroup.FadeAnimation(1, 1f, WaitingAnimation); // duration 때문에 따로 구현
        SetRandomImage();
    }

    private void SetRandomImage()
    {
        // 랜덤 이미지 선택
        imgIcon.rectTransform.rotation = Quaternion.identity;
        int ranIndex = Random.Range(0, iconAdrList.Count);
        Sprite icon = ResourceManager.Instance.GetResource<Sprite>(iconAdrList[ranIndex]);
        imgIcon.sprite = icon;
    }

    private void WaitingAnimation()
    {
        // RectTransform을 좌우로 반복 회전
        rectTrIcon.DORotate(
                new Vector3(0, 0, 10f), // 목표 각도 (z축 10도)
                0.5f                    // 시간
            )
            .SetEase(Ease.InOutSine)   // 부드럽게
            .SetLoops(-1, LoopType.Yoyo); // 무한 반복 좌우 왕복
    }
}
