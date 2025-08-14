using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWgReward : MonoBehaviour
{
    [SerializeField] private RectTransform rectTr;
    [SerializeField] private Image imgIconBg;
    [SerializeField] private Image imgIcon;
    [SerializeField] private TextMeshProUGUI tmpRewardCount;
    private RewardType rewardType;

    private void Reset()
    {
        rectTr = gameObject.GetComponent<RectTransform>();
        imgIconBg = transform.FindChildByName<Image>("Img_IconBG");
        imgIcon = transform.FindChildByName<Image>("Img_RewardIcon");
        tmpRewardCount = transform.FindChildByName<TextMeshProUGUI>("Tmp_RewardCount");
    }

    public void Initialize()
    {
        rewardType = RewardType.None;
    }
    
    public void Show(Sprite icon, int count, RewardType type = RewardType.None)
    {
        gameObject.SetActive(true);

        int value;
        if (type == rewardType)
        {
            value = Convert.ToInt32(tmpRewardCount.text) + count;
        }
        else
        {
            imgIcon.sprite = icon;
            value = count;
        }

        if (count == 0)
        {
            Hide();
            Initialize();
            return;
        }
        
        string signText = value > 1 ? "+" : "";
        string valueText = value == 1 ? "" : value.ToString();
        tmpRewardCount.text = $"{signText}{valueText}";
        imgIconBg.enabled = type == RewardType.None;

        rewardType = type;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
