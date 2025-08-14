using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIStartLoadingWindow : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Image imgTeamLogo;
    [SerializeField] private Image imgSpartaLogo;
    
    [SerializeField] private float fadeInTime = 1.0f;
    [SerializeField] private float holdTime = 1.0f;
    [SerializeField] private float fadeOutTime = 1.0f;

    private const string MeowAdr = "SFX_CatAttack01";

    private void Reset()
    {
        cg = GetComponent<CanvasGroup>();
        imgTeamLogo = transform.FindChildByName<Image>("Img_TeamLogo");
        imgSpartaLogo = transform.FindChildByName<Image>("Img_SpartaLogo");
    }

    public void PlayIntro()
    {
        imgSpartaLogo.color = new Color(1, 1, 1, 0);
        imgTeamLogo.color = new Color(1, 1, 1, 0);

        Sequence seq = DOTween.Sequence();
        seq.Append(imgSpartaLogo.DOFade(1f, fadeInTime)) // 페이드 인
            .AppendInterval(holdTime) // 잠시 유지
            .Append(imgSpartaLogo.DOFade(0f, fadeOutTime)).SetUpdate(true);
        seq.Append(imgTeamLogo.DOFade(1f, fadeInTime)) // 페이드 인
            .AppendInterval(holdTime) // 잠시 유지
            .JoinCallback(() => SoundManager.Instance.PlaySfx(MeowAdr))
            .Append(imgTeamLogo.DOFade(0f, fadeOutTime)).SetUpdate(true); // 페이드 아웃

        seq.OnComplete(FadeOut);
    }

    private void FadeOut()
    {
        cg.FadeAnimation(0, 0.2f, DestroyThis);
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
        _ = SceneLoadController.EnterSceneAsync(SceneType.Start);
    }
}
