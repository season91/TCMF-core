using UnityEngine;

/// <summary>
/// 진동 피드백 관리 클래스
/// </summary>
public class VibrationManager : Singleton<VibrationManager>
{
    protected override bool ShouldDontDestroyOnLoad => true;

    public bool isVibrationEnabled = true;

    protected override void Awake()
    {
        base.Awake();
        // 저장된 진동 설정 불러오기 (1: 켜짐, 0: 꺼짐)
        isVibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
    }

    /// <summary>
    /// 진동 설정 저장
    /// </summary>
    public void SetVibration(bool enabled)
    {
        isVibrationEnabled = enabled;
        PlayerPrefs.SetInt("VibrationEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 피격 시 진동
    /// </summary>
    public void VibrateOnHit()
    {
        if (!isVibrationEnabled) return;
        Handheld.Vibrate();
    }
}
