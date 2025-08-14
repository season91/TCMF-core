using UnityEngine;

/// <summary>
/// 가챠 결과 카드 위젯
/// </summary>
public class UIWgCard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRdrCard;
    [SerializeField] private SpriteRenderer spriteRdrItem;
    [SerializeField] private ParticleSystem psBack;
    [SerializeField] private ParticleSystem psBackSmallStar;
    [SerializeField] private ParticleSystem psBackStar;
    [SerializeField] private ParticleSystem psBackGlow;
    [SerializeField] private ParticleSystem psBackCard;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        psBack = transform.FindChildByName<ParticleSystem>("FX_UI_CardBack_S");
        spriteRdrCard = psBack.transform.FindChildByName<SpriteRenderer>("SpriteCard");
        spriteRdrItem = psBack.transform.FindChildByName<SpriteRenderer>("SpriteItem");
        psBackSmallStar = psBack.transform.FindChildByName<ParticleSystem>("smallstar");
        psBackStar = psBack.transform.FindChildByName<ParticleSystem>("Star");
        psBackGlow = psBack.transform.FindChildByName<ParticleSystem>("glow");
        psBackCard = psBack.transform.FindChildByName<ParticleSystem>("card");
    }
    
    /// <summary>
    /// 가챠 카드 설정 - 희귀도에 따른 -> 카드 배경, 카드 아이콘
    /// </summary>
    public void Set(Sprite cardBg, Sprite icon, ItemRarity rarity)
    {
        gameObject.SetActive(true);
        spriteRdrCard.gameObject.SetActive(false);
        spriteRdrItem.gameObject.SetActive(false);

        ParticleSystem.MainModule main;
        
        psBackSmallStar.gameObject.SetActive(false);
        psBackStar.gameObject.SetActive(false);
        
        if (rarity == ItemRarity.SuperRare)
        {
            psBackStar.gameObject.SetActive(true);
            main = psBackStar.main;
            main.startColor = rarity.ToColor();
        }
        else if (rarity == ItemRarity.Legendary)
        {
            psBackSmallStar.gameObject.SetActive(true);
            psBackStar.gameObject.SetActive(true);
            main = psBackStar.main;
            main.startColor = rarity.ToColor();
        }
        main = psBackCard.main;
        main.startColor = rarity.ToColor();
        main = psBackGlow.main;
        main.startColor = rarity.ToColor();
        
        spriteRdrItem.sprite = icon;
        spriteRdrCard.sprite = cardBg;
    }

    /// <summary>
    /// UI 표시
    /// </summary>
    public void Show()
    {
        spriteRdrCard.gameObject.SetActive(true);
        spriteRdrItem.gameObject.SetActive(true);
        psBack.Play();
    }

    /// <summary>
    /// UI 숨김
    /// </summary>
    public void Hide()
    {
        spriteRdrCard.gameObject.SetActive(false);
        spriteRdrItem.gameObject.SetActive(false);
        psBack.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        gameObject.SetActive(false);
    }
}
