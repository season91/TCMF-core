using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWcUserInfo : UIBase
{
    [SerializeField] private Image imgUserIcon;
    [SerializeField] private TextMeshProUGUI tmpUserName;
    [SerializeField] private List<UIWgResource> guiResourceList = new();
    
    [Header("====[Exp]")]
    [SerializeField] private TextMeshProUGUI tmpExp;
    [SerializeField] private Image imgGaugeExp;
    
    private readonly Dictionary<ResourceType, UIWgResource> guiResourceDict = new();
    private Action completeAction;
    private Sequence expSequence;
    
    protected override void Reset()
    {
        base.Reset();
        
        imgUserIcon = transform.FindChildByName<Image>("Img_UserIcon");
        tmpUserName = transform.FindChildByName<TextMeshProUGUI>("Tmp_UserName");
        guiResourceList = transform.FindChildByName<Transform>("Layout_Resources").GetComponentsInChildren<UIWgResource>().ToList();
        tmpExp = transform.FindChildByName<TextMeshProUGUI>("Tmp_Exp");
        imgGaugeExp = transform.FindChildByName<Image>("Img_GaugeExp");
    }

    public override void Initialize()
    {
        base.Initialize();

        imgUserIcon.sprite = ResourceManager.Instance.GetResource<Sprite>(StringAdrIcon.UserIcon[0]); // 임시, 수정 사항
        tmpUserName.text = UserData.userProfile.Nickname;
        
        var resources = UserData.inventory.currencyResourceTemplate;
        int index = 0;
        foreach (var resourceKvp in resources)
        {
            if (resourceKvp.Key != ResourceType.Gold && resourceKvp.Key != ResourceType.Diamond) continue;
            
            Sprite icon = ResourceManager.Instance.GetResource<Sprite>(StringAdrIcon.ResourceDict[resourceKvp.Key]);
            guiResourceList[index].Initialize(icon, resourceKvp.Value);
            guiResourceDict[resourceKvp.Key] = guiResourceList[index];
            index++;
        }
        
        tmpExp.text = UserData.info.Level.ToString();
        imgGaugeExp.fillAmount = UserData.info.GetExpRatio();
    }
    
    public override void Open(OpenContext openContext = null)
    {
        completeAction = openContext?.OnComplete;
        base.Open(OpenContext.WithCallback(UpdateGUI));
    }

    private void UpdateGUI()
    {
        // Resource들 업데이트 (변화가 있으면 애니메이션 재생할 것임)
        foreach (var resourceType in guiResourceDict.Keys)
        {
            UpdateResource(resourceType);
        }
        
        // Exp 업데이트 (변화가 있으면 애니메이션 재생할 것임)
        UpdateUserExp();
        completeAction?.Invoke();
    }

    public void UpdateResource(ResourceType resourceType)
    {
        if(resourceType == ResourceType.None || resourceType == ResourceType.Piece) return;
        if(canvasGroup.alpha <= 0) return;
        int value = UserData.inventory.currencyResourceTemplate[resourceType];
        guiResourceDict[resourceType].UpdateResource(value);
    }

    public void UpdateUserExp()
    {
        if(canvasGroup.alpha <= 0) return;
        
        if(expSequence != null)
            expSequence.Kill(true);
        expSequence = DOTween.Sequence();
        tmpExp.text = UserData.info.Level.ToString();
        
        float currentRatio = imgGaugeExp.fillAmount;
        float targetRatio = UserData.info.GetExpRatio();
        
        if (currentRatio > targetRatio)
        {
            expSequence.Append(
                DOTween.To(
                    () => imgGaugeExp.fillAmount,
                    x => imgGaugeExp.fillAmount = x,
                    1f,
                    1f
                )
            );
            expSequence.AppendCallback(() => imgGaugeExp.fillAmount = 0f);
            expSequence.Append(
                DOTween.To(
                    () => imgGaugeExp.fillAmount,
                    x => imgGaugeExp.fillAmount = x,
                    targetRatio,
                    1f
                )
            );
        }
        else
        {
            expSequence.Append(
                DOTween.To(
                    () => imgGaugeExp.fillAmount,
                    x => imgGaugeExp.fillAmount = x,
                    targetRatio,
                    1f
                )
            );
        }
    }
}
