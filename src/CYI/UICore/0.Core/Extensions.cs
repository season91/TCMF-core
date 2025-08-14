using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum CodeType
{
    Unit,
    Monster,
    Collection,
    Item,
    Stage,
    Skill
}

public enum RewardIconType
{
    Gold,
    Diamond,
    Piece
}

public static class Extensions
{
    // Extension: Transform
    public static T FindChildByName<T>(this Transform trans, string name) where T : Component
    {
        // 비활성화된 것까지 전부 
        T[] children = trans.GetComponentsInChildren<T>(true);
        foreach (T child in children)
        {
            if (child.name == name)
            {
                return child;
            }
        }
        return null;
    }
    
    // Extension: Canvas Group
    public static void SetAlpha(this CanvasGroup canvasGroup, float alpha)
    {
        if (alpha > 0.1f)
        {
            canvasGroup.alpha = alpha;
        }
        else
        {
            canvasGroup.alpha = 0;
        }
        SetInteractable(canvasGroup, alpha >= 0.1f);
    }

    public static void SetInteractable(this CanvasGroup canvasGroup, bool interactable)
    {
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }

    public static Tween FadeAnimation(
        this CanvasGroup canvasGroup,
        float endValue, 
        float duration = 0.2f, 
        Action callback = null)
    {
        canvasGroup.DOKill(true);
        var tween = canvasGroup.DOFade(endValue, duration).SetUpdate(true);
    
        if (callback != null)
            tween.OnComplete(callback.Invoke);

        return tween;
    }

    public static Tween FadeInOut(
        this CanvasGroup canvasGroup, 
        float startValue = 1, 
        float endValue = 0, 
        float fadeDuration = 0.2f,
        float keepDuration = 0.2f, 
        Action callback = null)
    {
        canvasGroup.DOKill(true);
        canvasGroup.alpha = endValue; // 시작 alpha 설정
        
        var seq = DOTween.Sequence()
            .SetUpdate(true)
            .Append(canvasGroup.DOFade(startValue, fadeDuration))
            .AppendInterval(keepDuration)
            .Append(canvasGroup.DOFade(endValue, fadeDuration));

        if (callback != null)
            seq.OnComplete(callback.Invoke);

        return seq;
    }

    public static Tween BounceAnimation(this RectTransform rectTr)
    {
        rectTr.DOKill(true);
        return rectTr.DOScale(1.05f, 0.1f)
                     .SetLoops(2, LoopType.Yoyo);
    }
    
    private static Sequence AnimatePopup(CanvasGroup canvasGroup, RectTransform rectTr, RectTransform.Axis axis, float startPosValue, float finalPosValue, float finalAlpha)
    {
        // DOTween 애니메이션 초기화
        canvasGroup.DOKill(true);
        rectTr.DOKill(true);

        float startAlpha = finalAlpha == 0 ? 1 : 0;
        canvasGroup.SetAlpha(startAlpha);

        // 시퀀스 생성
        Sequence sequence = DOTween.Sequence();
        
        if (axis == RectTransform.Axis.Horizontal)
        {
            rectTr.anchoredPosition = new Vector2(startPosValue, rectTr.anchoredPosition.y);
            sequence.Join(rectTr.DOAnchorPosX(finalPosValue, 0.5f));
        }
        else if (axis == RectTransform.Axis.Vertical)
        {
            rectTr.anchoredPosition = new Vector2(rectTr.anchoredPosition.x, startPosValue);
            sequence.Join(rectTr.DOAnchorPosY(finalPosValue, 0.5f));
        }
        else
        {
            MyDebug.Log("Animation Failed: AnimatePopup");
            return null;
        }

        // 페이드 애니메이션: 시작 0.1초 후 실행 (0.5초 중간쯤)
        sequence.Insert(0.1f, canvasGroup.FadeAnimation(finalAlpha, 0.4f));
        return sequence;
    }
    
    public static Sequence OpenPopupAnimation(this CanvasGroup canvasGroup, RectTransform rectTr, RectTransform.Axis axis, float startPosValue, float finalPosValue)
        => AnimatePopup(canvasGroup, rectTr, axis, startPosValue, finalPosValue, 1f);
    public static Sequence ClosePopupAnimation(this CanvasGroup canvasGroup, RectTransform rectTr, RectTransform.Axis axis, float startPosValue, float finalPosValue)
        => AnimatePopup(canvasGroup, rectTr, axis, startPosValue, finalPosValue, 0f);

    public static void AddListener(this Button button, UnityAction action)
    {
        button.onClick.RemoveListener(SoundManager.Instance.PlayButtonSound);
        button.onClick.AddListener(SoundManager.Instance.PlayButtonSound);
        button.onClick.AddListener(action);
    }
    
    public static string GetFullCode(this CodeType codeType, float code)
    {
        return codeType switch
        {
            CodeType.Unit => $"UNIT_{code}",
            CodeType.Monster => $"MONS_{code}",
            CodeType.Collection => $"COLL_{code}",
            CodeType.Item => $"ITEM_{code}",
            CodeType.Stage => $"STAG_{code}",
            CodeType.Skill => $"SKIL_{code}",
            _ => null
        };
    }
    
    public static string ToSpriteByTmp(this RewardIconType iconType)
    {
        return iconType switch
        {
            RewardIconType.Gold => "<sprite name=Icon_Gold>",
            RewardIconType.Diamond => "<sprite name=Icon_Dia>",
            RewardIconType.Piece => "<sprite name=Icon_Piece>",
            _ => String.Empty
        };
    }

    public static RewardType ToRewardType(this ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.None => RewardType.None,
            ResourceType.Gold => RewardType.Gold,
            ResourceType.Diamond => RewardType.Diamond,
            ResourceType.Piece => RewardType.Piece,
            _ => RewardType.None
        };
    }

    public static ResourceType ToResourceType(this RewardType rewardType)
    {
        switch (rewardType)
        {
            default:
            case RewardType.None:
            case RewardType.Exp:
            case RewardType.Item:
            case RewardType.Unit:
                return ResourceType.None;
            case RewardType.Gold:
                return ResourceType.Gold;
            case RewardType.Diamond:
                return ResourceType.Diamond;
            case RewardType.Piece:
                return ResourceType.Piece;
        }
    }

    #region  Cutin 

    /// <summary>
    /// RectTransform을 특정 위치로 이동 후 일정 시간 대기, 다시 다른 위치로 복귀하는 연출 시퀀스 생성
    /// </summary>
    /// <param name="rectTr">애니메이션 대상 RectTransform</param>
    /// <param name="enterPos">처음 이동할 목표 위치</param>
    /// <param name="exitPos">복귀할 최종 위치</param>
    /// <param name="inTime">등장 이동 시간</param>
    /// <param name="stayTime">도착 후 대기 시간</param>
    /// <param name="outTime">복귀 이동 시간</param>
    /// <param name="startTime">시퀀스 시작 타이밍 (지연 시간)</param>
    public static Sequence MoveToAndBack(this RectTransform rectTr,
        Vector2 enterPos, Vector2 exitPos,
        float inTime, float stayTime, float outTime, float startTime = 0f)
    {
        var seq = DOTween.Sequence().SetUpdate(true);
        seq.Insert(startTime, rectTr.DOAnchorPos(enterPos, inTime).SetEase(Ease.Linear));
        seq.Insert(startTime + inTime + stayTime, rectTr.DOAnchorPos(exitPos, outTime).SetEase(Ease.InSine));
        return seq;
    }

    /// <summary>
    /// RectTransform을 빠르게 진입 → 대기 → 천천히 이탈 → 마지막 순간 재이동 처리하는 슬래시 타입 연출
    /// </summary>
    /// <param name="rectTr">애니메이션 대상 RectTransform</param>
    /// <param name="firstTargetPos">빠르게 진입할 1차 위치</param>
    /// <param name="firstDuration">1차 진입에 걸리는 시간</param>
    /// <param name="waitDuration">도착 후 정지 시간</param>
    /// <param name="secondTargetPos">천천히 이동할 2차 위치</param>
    /// <param name="secondDuration">2차 이동에 걸리는 시간</param>
    /// <param name="instantTime">마지막 순간 이동 처리 시간 (보통 0.01초)</param>
    /// <param name="startTime">시퀀스 시작 타이밍 (지연 시간)</param>
    public static Sequence FlySlash(this RectTransform rectTr,
        Vector2 firstTargetPos, float firstDuration,
        float waitDuration,
        Vector2 secondTargetPos, float secondDuration,
        float instantTime = 0.01f,
        float startTime = 0f)
    {
        var seq = DOTween.Sequence().SetUpdate(true);

        Debug.Log($"[FlySlash] 초기 위치: {rectTr.anchoredPosition}, 이동1: {firstTargetPos} → 이동2: {secondTargetPos}");

        // 초기 빠른 이동
        seq.Insert(startTime, rectTr.DOAnchorPos(firstTargetPos, firstDuration).SetEase(Ease.Linear));
    
        // 대기
        seq.Insert(startTime + firstDuration, DOTween.Sequence().AppendInterval(waitDuration));
    
        // 천천히 2차 위치로 이동
        seq.Insert(startTime + firstDuration + waitDuration,
            rectTr.DOAnchorPos(secondTargetPos, secondDuration).SetEase(Ease.Linear));
    
        // 철수 순간 이동 (사라짐 처리)
        seq.Insert(startTime + firstDuration + waitDuration + secondDuration,
            rectTr.DOAnchorPos(secondTargetPos, instantTime));

        return seq;
    }


    #endregion
    
    #region 엔딩크레딧
    /// <summary>
    /// CanvasGroup의 RectTransform을 위로 스크롤
    /// </summary>
    /// <param name="rootCanvasGroup">스크롤 대상 CanvasGroup</param>
    /// <param name="speedPxPerSec">초당 이동 속도(px)</param>
    /// <param name="topPadding">화면 위쪽 여유 패딩(px)</param>
    public static Tween ScrollCredits(this CanvasGroup rootCanvasGroup, CanvasGroup parentCanvasGroup, float speedPxPerSec = 200f, float topPadding = 100f)
    {
        RectTransform rect = rootCanvasGroup.GetComponent<RectTransform>();
        RectTransform canvasRect = rootCanvasGroup.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        float screenHeight = canvasRect.rect.height;
        float contentHeight = rect.rect.height;

        // 시작 위치: 컨텐츠 전체가 화면 아래에 숨어있도록
        float startY = -(screenHeight); 
        // 목표 위치: 컨텐츠가 화면 위로 사라질 때 + topPadding
        float endY = contentHeight + topPadding;

        // 이동 시간 계산
        float duration = Mathf.Abs(endY - startY) / speedPxPerSec;

        float lastClickTime = 0f; // 첫 클릭
        float doubleClickThreshold = 0.25f; // 더블 클릭 간격(초)

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, startY);

        return rect.DOAnchorPosY(endY, duration)
                   .SetEase(Ease.Linear)
                   .OnUpdate(() =>
                   {
                       if (Input.GetMouseButtonDown(0)) // 왼쪽 마우스 클릭
                       {
                           if (Time.time - lastClickTime <= doubleClickThreshold)
                           {
                               // 더블 클릭 시 → 스킵
                               _ = UIManager.Instance.EnterLoadingAsync(SceneType.Lobby);
                           }
                           lastClickTime = Time.time;
                       }
                   })
                   .OnStart(() =>
                   {
                       parentCanvasGroup.SetAlpha(1);
                       rootCanvasGroup.SetAlpha(1);
                       rootCanvasGroup.gameObject.SetActive(true);
                   })
                   .OnComplete(() =>
                   {
                       rootCanvasGroup.SetAlpha(0);
                       rootCanvasGroup.gameObject.SetActive(false);

                       _ = UIManager.Instance.EnterLoadingAsync(SceneType.Lobby);
                   });
    }

    
    #endregion
    
    #region 한글 변환

    public static string ToKor(this ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Gold => "골드",
            ResourceType.Diamond => "다이아",
            _ => String.Empty
        };
    }

    public static string ToKor(this StatType statType)
    {
        return statType switch
        {
            StatType.Hp => "체력",
            StatType.Atk => "공격력",
            StatType.Def => "방어력",
            StatType.Evasion => "회피",
            StatType.SkillCoolTime => "스킬 쿨타임",
            _ => string.Empty
        };
    }
    
    public static string ToKor(this ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Armor => "방어구",
            ItemType.Weapon => "무기",
            _ => string.Empty
        };
    }
    
    public static string ToEng(this ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Armor => "Armor",
            ItemType.Weapon => "Weapon",
            _ => string.Empty
        };
    }

    public static string ToKor(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "일반",
            ItemRarity.Uncommon => "고급",
            ItemRarity.Rare => "희귀",
            ItemRarity.SuperRare => "영웅",
            ItemRarity.Legendary => "전설",
            _ => string.Empty
        };
    }
    
    public static string ToEng(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "Common",
            ItemRarity.Uncommon => "Uncommon",
            ItemRarity.Rare => "Rare",
            ItemRarity.SuperRare => "SuperRare",
            ItemRarity.Legendary => "Legendary",
            _ => string.Empty
        };
    }
    
    public static Color ToColor(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => GameColor.Gray,
            ItemRarity.Uncommon => GameColor.Green,
            ItemRarity.Rare => GameColor.Blue,
            ItemRarity.SuperRare => GameColor.Purple,
            ItemRarity.Legendary => GameColor.Red,
            _ => GameColor.White
        };
    }
    
        public static string ToKor(this ContentType contentType)
        {
            return contentType switch
            {
                ContentType.None or ContentType.Gacha or ContentType.Blacksmith or ContentType.Battle or ContentType.Lobby 
                    => string.Empty,
                ContentType.UnitInventory => "유닛 인벤토리",
                ContentType.ItemInventory => "아이템 인벤토리",
                ContentType.Collection => "아이템 도감",
                ContentType.GoldGacha => "가챠 - 골드",
                ContentType.DiaGacha => "가챠 - 다이아",
                ContentType.Enhancement => "대장장이 - 강화",
                ContentType.LimitBreak => "대장장이 - 돌파",
                ContentType.Dismantle => "대장장이 - 분해",
                ContentType.UnitSelect => "유닛 출정석",
                _ => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null)
            };
        }
    
    #endregion
}

public static class GameColor
{
    public static readonly Color Black  = new(50  / 255f, 50  / 255f, 50  / 255f, 1f);   // #323232
    public static readonly Color White  = new(247 / 255f, 247 / 255f, 247 / 255f, 1f);   // #F7F7F7
    public static readonly Color Red    = new(255 / 255f, 0   / 255f, 0   / 255f, 1f);   // #FF0000
    public static readonly Color Purple = new(255 / 255f, 0   / 255f, 251 / 255f, 1f);   // #FF00FB
    public static readonly Color Blue   = new(0   / 255f, 120 / 255f, 255 / 255f, 1f);   // #0078FF
    public static readonly Color Green  = new(59  / 255f, 255 / 255f, 0   / 255f, 1f);   // #3BFF00
    public static readonly Color Gray   = new(192 / 255f, 192 / 255f, 192 / 255f, 1f);   // #C0C0C0

    public static readonly Color Danger  = new(141 / 255f, 14  / 255f, 14  / 255f, 1f); // #8D0E0E
    public static readonly Color Warning = new(210 / 255f, 157 / 255f, 21  / 255f, 1f); // #D29D15
    public static readonly Color Safe    = new(85  / 255f, 204 / 255f, 41  / 255f, 1f); // #55CC29
}

public static class GameColorHexCode
{
    public static readonly string Black  = "#323232";
    public static readonly string White  = "#F7F7F7";
    public static readonly string Red    = "#E91114";
    public static readonly string Purple = "#8803CB";
    public static readonly string Blue   = "#065D9F";
    public static readonly string Green  = "#509F06";
    public static readonly string Gray   = "#C0C0C0";
}