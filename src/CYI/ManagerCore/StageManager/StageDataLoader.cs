using System.Collections.Generic;

/// <summary>
/// 챕터별 스테이지 데이터를 로딩하고 관리하는 클래스
/// </summary>
public class StageDataLoader
{
    public Dictionary<int, List<StageData>> StageDataListByChapter { get; } = new();

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    public void Initialize()
    {
        int chapterCount = MasterData.StageDataDict.Count / 3;

        for (int i = 1; i <= chapterCount; i++)
        {
            StageDataListByChapter[i] = GetStageDataByChapter(i);
        }
    }

    /// <summary>
    /// 챕터에 따른 스테이지 데이터 리스트 받아오기
    /// </summary>
    public List<StageData> GetStageDataByChapter(int chapterNum)
    {
        var stageDatas = new List<StageData>();

        int chapterCode = chapterNum * 1000;
        for (int i = 1; i <= 3; i++)
        {
            int stageNum = i;
            string curStageCode = CodeType.Stage.GetFullCode(chapterCode + stageNum);

            if (MasterData.StageDataDict.TryGetValue(curStageCode, out var stageData))
            {
                stageData.CacheFirstMonsterCode();
                stageDatas.Add(stageData);
            }
            else
            {
                MyDebug.LogWarning($"Stage data is not found => {curStageCode}");
            }
        }

        return stageDatas;
    }
}