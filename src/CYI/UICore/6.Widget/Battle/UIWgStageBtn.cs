using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 챕터 및 스테이지 선택창 - 스테이지 버튼
/// </summary>
public class UIWgStageBtn : MonoBehaviour
{
    [SerializeField] private Button btnEnterStage;
    [SerializeField] private Image imgMonster;
    [SerializeField] private TextMeshProUGUI tmpStageTitle;
    private Action<int, bool> selectAction;
    private int curStageNum;
    private bool isUnlocked;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        btnEnterStage = GetComponent<Button>();
        imgMonster = transform.FindChildByName<Image>("Img_StageMonster");
        tmpStageTitle = transform.FindChildByName<TextMeshProUGUI>("Tmp_StageTitle");
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public void Initialize(Action<int, bool> callback, int stageNum)
    {
        btnEnterStage.onClick.RemoveAllListeners();
        btnEnterStage.AddListener(OnSelectButton);
        selectAction = callback;
        curStageNum = stageNum;
    }

    /// <summary>
    /// 스테이지 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnSelectButton()
    {
        selectAction?.Invoke(curStageNum, isUnlocked);
    }
    
    /// <summary>
    /// UI 표시: 몬스터 이미지, 스테이지 제목, 현재 열렸는가
    /// </summary>
    public void Show(Sprite monster, string stageTitle, bool isActive)
    {
        imgMonster.sprite = monster;
        imgMonster.SetNativeSize();
        tmpStageTitle.text = stageTitle;
        isUnlocked = isActive;
    }
}
