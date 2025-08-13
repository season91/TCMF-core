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
    /// <summary>
    /// 유저 account 계정 정보 저장
    /// </summary>
    public static async Task UploadAccountDataAsync(AccountData account)
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
    public static async Task SaveInventoryAsync(string uid, UserInventory inventory)
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
    public static async Task UpdateInventoryCurrencyAsync(string uid, string fieldName, int amount)
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
    public static async Task SaveInventoryItemAsync(string uid, InventoryItem item)
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
    public static async Task SaveInventoryItemsBatchAsync(string uid, List<InventoryItem> items)
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
    public static async Task DeleteInventoryItemsBatchAsync(string uid, List<InventoryItem> items)
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
    public static async Task DeleteInventoryItemAsync(string uid, string itemUid)
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
    public static async Task SaveInventoryUnitAsync(string uid, InventoryUnit unit)
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
    public static async Task SaveDataUserInfoAsync(string uid, UserInfo info)
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
    public static async Task SaveDataUserStageInitAsync(string uid, UserStage stage)
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
    public static async Task SaveUserStageProgressAsync(string uid, StageProgress progress)
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
    public static async Task SaveUserCollectedInitAsync(string uid, UserCollected collected)
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
    public static async Task SaveUserCollectedAsync(string uid, CollectionStatus status)
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
    public static async Task SaveUserCollectedBatchAsync(string uid, List<CollectionStatus> statusList)
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
    public static async Task<bool> CheckUserExistsAsync(string uid)
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