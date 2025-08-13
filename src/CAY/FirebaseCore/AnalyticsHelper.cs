using System;
using Firebase.Analytics;
/// <summary>
/// Firebase GA 이벤트 로깅 담당
/// </summary>
public static class AnalyticsHelper 
{
    static readonly string uid = FirebaseManager.Instance.DbUser.UserId;
    
    /// <summary>
    /// GA4용 screen_view 이벤트 로깅 함수
    /// </summary>
    public static void LogScreenView(string screen, string uiName)
    {
        // GA4에서 요구하는 필수 파라미터
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventScreenView, new Parameter[] {
            new Parameter(FirebaseAnalytics.ParameterScreenName, screen),
            new Parameter(FirebaseAnalytics.ParameterScreenClass, uiName)
        });

        MyDebug.Log($"[Analytics] Screen View Logged: {screen} ({uiName})");
    }
    
    // 가챠 이벤트 시작 로깅
    public static void LogGachaStartEvent(ResourceType resourceType, int cost)
    {

        string eventId = Guid.NewGuid().ToString(); // 고유 이벤트 ID 
        // Firebase Analytics로 가챠 이벤트 기록
        FirebaseAnalytics.LogEvent(AnalyticsEvent.GachaStart, new Parameter[] {
            new Parameter("eventId", eventId),
            new Parameter("uid", uid),
            new Parameter("resourceType", resourceType.ToString()),
            new Parameter("cost", cost)
            }
        );

        MyDebug.Log($"Logged Gacha Start Event : eventId {eventId} / uid {uid} / 재화타입 {resourceType} / 비용 {cost}"); 
    }
    
    // 가챠 이벤트 결과 로깅
    public static void LogGachaResultEvent(ResourceType resourceType, int gachaMode, ItemRarity rarity, string itemCode)
    {
        string eventId = Guid.NewGuid().ToString(); // 고유 이벤트 ID 
        // Firebase Analytics로 가챠 이벤트 기록
        FirebaseAnalytics.LogEvent(AnalyticsEvent.GachaResult, new Parameter[] {
                new Parameter("eventId", eventId),
                new Parameter("uid", uid),
                new Parameter("resourceType", resourceType.ToString()),
                new Parameter("gachaMode", gachaMode),
                new Parameter("gachaRarity", rarity.ToString()),
                new Parameter("gachaItemCode", itemCode)
            }
        );

        MyDebug.Log($"Logged Gacha Result Event : {uid} 뽑기구분 {gachaMode} / 결과등급 {rarity.ToString()} / 아이템 코드 {itemCode}"); 
    }
    
    // 가챠 이벤트 천장 도달 로깅
    public static void LogGachaPityEvent(ResourceType resourceType, string pityType, int pityCount)
    {
        string eventId = Guid.NewGuid().ToString(); // 고유 이벤트 ID 
        
        // Firebase Analytics로 가챠 이벤트 기록
        FirebaseAnalytics.LogEvent(AnalyticsEvent.GachaCelling, new Parameter[] {
                new Parameter("eventId", eventId),
                new Parameter("uid", uid),
                new Parameter("resourceType", resourceType.ToString()),
                new Parameter("pityType", pityType)
            }
        );

        MyDebug.Log($"Logged Gacha Start Event : {uid} 재화타입 {resourceType} 천장 도달 구분 {pityType}"); 
    }
        
}
