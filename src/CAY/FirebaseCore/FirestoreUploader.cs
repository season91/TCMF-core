using System.Collections.Generic;
using System.Linq;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 기본 base 데이터 DB 적재하는 class
/// Item, Monster, Unit, Stage, Skill, 도감 등등
/// </summary>

public class FirestoreUploader
{
    
#if UNITY_EDITOR
    /// <summary>
    /// RAWDATA 적재 - 마스터 데이터
    /// </summary>
    public static void UploadMasterDataToFirestore(object data, string colType)
    {
        var db = FirebaseFirestore.DefaultInstance;
        var col = db.Collection(FirestoreCollection.Data)
                                    .Document(FirestoreDocument.RowData)
                                    .Collection(colType);
        string id = null;
        switch (colType)
        {
            case FirestoreCollection.Item:
                ItemData item = (ItemData) data;
                id = item.Code;
                col.Document(id).SetAsync(item).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        MyDebug.Log($"ITEM 업로드: {id}");
                    else
                        MyDebug.LogError($"ITEM 실패: {id} / {task.Exception}");
                });
                break;
            case FirestoreCollection.Monster:
                MonsterData monster = (MonsterData) data;
                id = monster.Code;
                col.Document(id).SetAsync(monster).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        MyDebug.Log($"MONSTER 업로드: {id}");
                    else
                        MyDebug.LogError($"MONSTER 실패: {id} / {task.Exception}");
                });
                break;
            case FirestoreCollection.Stage:
                StageData stage = (StageData) data;
                id = stage.Code;
                col.Document(id).SetAsync(stage).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        MyDebug.Log($"STAGE 업로드: {id}");
                    else
                        MyDebug.LogError($"STAGE 실패: {id} / {task.Exception}");
                });
                break;
            case FirestoreDocument.Gacha:
                GachaTable gachaTable = (GachaTable) data;
                db.Collection("TABLE")
                  .Document(FirestoreDocument.Gacha)
                  .SetAsync(gachaTable)
                  .ContinueWithOnMainThread(task =>
                  {
                      if (task.IsCompletedSuccessfully)
                          MyDebug.Log($"업로드 성공:");
                      else
                          MyDebug.LogError($"업로드 실패:");
                  });
                break;
            case FirestoreCollection.Unit:
                UnitData unit = (UnitData) data;
                id = unit.Code;
                col.Document(id).SetAsync(unit).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        MyDebug.Log($"UNIT 업로드: {id}");
                    else
                        MyDebug.LogError($"UNIT 실패: {id} / {task.Exception}");
                });
                break;
            case FirestoreCollection.Skill:
                SkillData skill = (SkillData) data;
                id = skill.Code;
                col.Document(id).SetAsync(skill).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        MyDebug.Log($"SKILL 업로드: {id}");
                    else
                        MyDebug.LogError($"SKILL 실패: {id} / {task.Exception}");
                });
                break;
            case FirestoreCollection.LevelUpExp:
                LevelUpExpData levelUpExp = (LevelUpExpData) data;
                id = levelUpExp.Level.ToString();
                col.Document(id).SetAsync(levelUpExp).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        MyDebug.Log($"EXP 업로드: {id}");
                    else
                        MyDebug.LogError($"EXP 실패: {id} / {task.Exception}");
                });
                break;
            case FirestoreCollection.Enhancement:
                EnhancementData enhancement = (EnhancementData) data;
                id = enhancement.EnhancementLevel.ToString();
                col.Document(id).SetAsync(enhancement).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        MyDebug.Log($"Enhancement 업로드: {id}");
                    else
                        MyDebug.LogError($"Enhancement 실패: {id} / {task.Exception}");
                });
                break;
            case FirestoreCollection.LimitBreak:
                LimitBreakData limitBreak = (LimitBreakData) data;
                id = limitBreak.Code;
                col.Document(id).SetAsync(limitBreak).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        MyDebug.Log($"limitBreak 업로드: {id}");
                    else
                        MyDebug.LogError($"limitBreak 실패: {id} / {task.Exception}");
                });
                break;
            case FirestoreCollection.Collection:
                CollectionData collection = (CollectionData) data;
                id = collection.Code;
                col.Document(id).SetAsync(collection).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        MyDebug.Log($"collection 업로드: {id}");
                    else
                        MyDebug.LogError($"collection 실패: {id} / {task.Exception}");
                });
                break;
        }
    }
#endif
    /// <summary>
    /// 유저 account 계정 정보 저장
    /// </summary>
    public static async Task UploadAccountData(AccountData account)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
          .Document(account.uid)
          .Collection(FirestoreCollection.Account)
          .Document(FirestoreDocument.Profile)
          .SetAsync(account);
    }

    /// <summary>
    /// 유저 인벤토리 데이터 - 재화 전체 정보 저장
    /// </summary>
    public static async Task SaveDataInventory(string uid, UserInventory inventory)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
                .Document(uid)
                .Collection(FirestoreCollection.Save)
                .Document(FirestoreDocument.Inventory)
                .SetAsync(inventory);
    }
    
    /// <summary>
    /// 유저 인벤토리 데이터 - 재화별 단건 저장
    /// </summary>
    public static async Task UpdateInventoryCurrency(string uid, string fieldName, int amount)
    {
        var db = FirebaseFirestore.DefaultInstance
                                .Collection(FirestoreCollection.User)
                                .Document(uid)
                                .Collection(FirestoreCollection.Save)
                                .Document(FirestoreDocument.Inventory);
        
        var update = new Dictionary<string, object>
        {
            { fieldName, amount }
        };

        MyDebug.Log($"인벤토리 수정 {fieldName} : {amount}");
        await db.UpdateAsync(update);
    }
    
    /// <summary>
    /// 유저 인벤토리 데이터 - 보유 아이템 단건 정보 저장
    /// </summary>
    public static async Task SaveInventoryItem(string uid, InventoryItem item)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
               .Document(uid)
               .Collection(FirestoreCollection.Save)
               .Document(FirestoreDocument.Inventory)
               .Collection(FirestoreCollection.Items)
               .Document(item.ItemUid)
               .SetAsync(item);
    }
    
    /// <summary>
    /// 유저 인벤토리 데이터 - 보유 아이템 여러건 정보 저장
    /// </summary>
    public static async Task SaveInventoryItemsBatch(string uid, List<InventoryItem> items)
    {
        WriteBatch batch = FirebaseFirestore.DefaultInstance.StartBatch();
        var db = FirebaseFirestore.DefaultInstance.Collection(FirestoreCollection.User)
                                  .Document(uid)
                                  .Collection(FirestoreCollection.Save)
                                  .Document(FirestoreDocument.Inventory)
                                  .Collection(FirestoreCollection.Items);
        foreach (var item in items)
        {
            DocumentReference docRef = db.Document(item.ItemUid);
            batch.Set(docRef, item);
        }

        await batch.CommitAsync();
        MyDebug.Log($"[FirestoreUploader] {items.Count}개 아이템 저장 완료");
    }
    
    /// <summary>
    /// 유저 인벤토리 데이터 - 보유 아이템 여러개 삭제
    /// </summary>
    public static async Task DeleteInventoryItemsBatch(string uid, List<InventoryItem> items)
    {
        WriteBatch batch = FirebaseFirestore.DefaultInstance.StartBatch();
        
        var db = FirebaseFirestore.DefaultInstance
                                  .Collection(FirestoreCollection.User)
                                  .Document(uid)
                                  .Collection(FirestoreCollection.Save)
                                  .Document(FirestoreDocument.Inventory)
                                  .Collection(FirestoreCollection.Items);
                                
        foreach (var item in items)
        {
            DocumentReference docRef = db.Document(item.ItemUid);
            batch.Delete(docRef);
        }
        
        await batch.CommitAsync();
        MyDebug.Log($"[FirestoreUploader] {items.Count}개 아이템 삭제 완료");
    }
    
    /// <summary>
    /// 유저 인벤토리 데이터 - 보유 아이템 단건 삭제
    /// </summary>
    public static async Task DeleteInventoryItem(string uid, string itemUid)
    {
        var db = FirebaseFirestore.DefaultInstance;

        var itemDocRef  = db.Collection(FirestoreCollection.User)
                            .Document(uid)
                            .Collection(FirestoreCollection.Save)
                            .Document(FirestoreDocument.Inventory)
                            .Collection(FirestoreCollection.Items)
                            .Document(itemUid);

        await itemDocRef.DeleteAsync();

    }
    
    /// <summary>
    /// 유저 인벤토리 데이터 - 유닛 아이템 단건 정보 저장
    /// </summary>
    public static async Task SaveInventoryUnit(string uid, InventoryUnit unit)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
                .Document(uid)
                .Collection(FirestoreCollection.Save)
                .Document(FirestoreDocument.Inventory)
                .Collection(FirestoreCollection.Units)
                .Document(unit.UnitUid)
                .SetAsync(unit);
    }
    
    /// <summary>
    /// 유저 레벨, 경험치 정보 전체 저장
    /// </summary>
    public static async Task SaveDataUserInfo(string uid, UserInfo info)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
                .Document(uid)
                .Collection(FirestoreCollection.Save)
                .Document(FirestoreDocument.Info)
                .SetAsync(info);
    }

    /// <summary>
    /// 유저 stage 진척 전보 최초1회 저장
    /// </summary>
    public static async Task SaveDataUserStageInit(string uid, UserStage stage)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
                .Document(uid)
                .Collection(FirestoreCollection.Save)
                .Document(FirestoreDocument.Stage)
                .SetAsync(stage);
    }
    
    /// <summary>
    /// 유저 stage 진척 정보 단건 저장
    /// </summary>
    public static async Task SaveUserStageInit(string uid, UserStage stage)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
                .Document(uid)
                .Collection(FirestoreCollection.Save)
                .Document(FirestoreDocument.Stage)
                .SetAsync(stage);
    }

    
    /// <summary>
    /// 유저 stage 진척 정보 단건 저장
    /// </summary>
    public static async Task SaveUserStageProgress(string uid, StageProgress progress)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
                .Document(uid)
                .Collection(FirestoreCollection.Save)
                .Document(FirestoreDocument.Stage)
                .Collection(FirestoreCollection.Progresses)
                .Document(progress.StageCode)
                .SetAsync(progress);
    }
    
    /// <summary>
    /// 유저 도감 데이터 - 유닛 도감 최초1회 정보 저장
    /// </summary>
    public static async Task SaveUserCollectedInit(string uid, UserCollected collected)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
                .Document(uid)
                .Collection(FirestoreCollection.Save)
                .Document(FirestoreDocument.Collected)
                .SetAsync(collected);
    }

    /// <summary>
    /// 유저 도감 데이터 - 유닛 도감 단건 정보 저장
    /// </summary>
    public static async Task SaveUserCollected(string uid, CollectionStatus status)
    {
        var db = FirebaseFirestore.DefaultInstance;
        await db.Collection(FirestoreCollection.User)
                .Document(uid)
                .Collection(FirestoreCollection.Save)
                .Document(FirestoreDocument.Collected)
                .Collection(FirestoreCollection.Collects)
                .Document(status.Code)
                .SetAsync(status);
    }
    
    /// <summary>
    /// 유저 도감 데이터 - 유닛 도감 여러건 정보 저장
    /// </summary>
    public static async Task SaveUserCollectedBatch(string uid, List<CollectionStatus> statusList)
    {
        WriteBatch batch = FirebaseFirestore.DefaultInstance.StartBatch();
        var db = FirebaseFirestore.DefaultInstance.Collection(FirestoreCollection.User)
                                  .Document(uid)
                                  .Collection(FirestoreCollection.Save)
                                  .Document(FirestoreDocument.Collected)
                                  .Collection(FirestoreCollection.Collects);
        foreach (var status in statusList)
        {
            DocumentReference docRef = db.Document(status.Code);
            batch.Set(docRef, status);
        }

        await batch.CommitAsync();
        MyDebug.Log($"[FirestoreUploader] {statusList.Count}개 도감 저장 완료");
    }

    /// <summary>
    /// 해당 UID에 대한 유저 계정 데이터가 Firestore에 존재하는지 확인
    /// </summary>
    public static async Task<bool> CheckUserExists(string uid)
    {
        var db = FirebaseFirestore.DefaultInstance;
        var snapshot = await db.Collection(FirestoreCollection.User)
                               .Document(uid)
                               .Collection(FirestoreCollection.Account)
                               .Document(FirestoreDocument.Profile)
                               .GetSnapshotAsync();
        MyDebug.Log($"[CheckUserProfileExists] {uid} PROFILE 문서 존재 여부: {snapshot.Exists}");
        return snapshot.Exists;
    }
}