using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Firestore;

/// <summary>
/// 기본 base 데이터 DB 조회해주는 Helper
/// Item, Monster, Unit, Stage, Skill, 도감 등등
/// </summary>
public static class FirestoreHelper
{
    public static FirebaseFirestore DB => FirebaseFirestore.DefaultInstance;

    /// <summary>
    /// 특정 로우 데이터 DB 단건 조회 - pk 기준 
    /// </summary>
    public static async Task<T> GetRowDataDocumentByCollPk<T>(string coll, string docId)
    {
        var snapshot = await DB.Collection(FirestoreCollection.Data)
                               .Document(FirestoreDocument.RowData)
                               .Collection(coll)
                               .Document(docId).GetSnapshotAsync();
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<T>();
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// 특정 테이블 데이터 조회
    /// </summary>
    public static async Task<T> GetTableDocumentByDocid<T>(string docid)
    {
        var snapshot = await DB.Collection(FirestoreCollection.Table)
                               .Document(docid)
                               .GetSnapshotAsync();
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<T>();
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// 특정 로우데이터 DB 전체 조회
    /// </summary>
    public static async Task<List<T>> GetRowDataDocumentByColl<T>(string coll)
    {
        var snapshot = await DB.Collection(FirestoreCollection.Data)
                               .Document(FirestoreDocument.RowData)
                               .Collection(coll)
                               .GetSnapshotAsync();

        return snapshot.Documents.Select(d => d.ConvertTo<T>()).ToList();
    }
    
    /// <summary>
    /// Stage 데이터 타이틀로 단건 조회
    /// </summary>
    public static async Task<StageData> GetStageDataByTitle(string title)
    {
        var snapshot = await DB.Collection(FirestoreCollection.Data)
                               .Document(FirestoreDocument.RowData)
                               .Collection(FirestoreCollection.Stage)
                               .WhereEqualTo(FirebaseCondition.stageTitle, title)
                               .GetSnapshotAsync();

        var doc = snapshot.Documents.FirstOrDefault();

        if (doc == null)
        {
            MyDebug.LogError($"'{title}'에 해당하는 스테이지를 찾을 수 없습니다.");
            return null;
        }

        var stageData = doc.ConvertTo<StageData>();
        return stageData;
    }

    /// <summary>
    /// Save data 전체 조회
    /// </summary>
    public static async Task GetAccountDataByUserId(string uid)
    {
        var snapshot = await DB.Collection(FirestoreCollection.User)
                               .Document(uid)
                               .Collection(FirestoreCollection.Account)
                               .Document(FirestoreDocument.Profile)
                               .GetSnapshotAsync();
        
        UserData.userProfile = snapshot.ConvertTo<AccountData>();
    }

    /// <summary>
    /// Save data 전체 조회
    /// </summary>
    public static async Task GetUserDataByUserId(string uid)
    {
        var snapshot = await DB.Collection(FirestoreCollection.User)
                               .Document(uid)
                               .Collection(FirestoreCollection.Save)
                               .GetSnapshotAsync();

        foreach (var docSnap in snapshot.Documents)
        {
            string docId = docSnap.Id.ToUpper();
            switch (docId)
            {
                case FirestoreDocument.Inventory:
                    UserData.inventory = docSnap.ConvertTo<UserInventory>();
                    UserData.inventory.SaveCurrencyToTemplate(); // 재화 조회용 변수에 초기화
                    UserData.inventory.SavePityCountToTemplate();
                    // 서브컬렉션 ITEMS
                    var itemsSnapshot = await docSnap.Reference
                                                     .Collection(FirestoreCollection.Items)
                                                     .GetSnapshotAsync();
                    UserData.inventory.items = itemsSnapshot.Documents
                                                            .Select(doc => doc.ConvertTo<InventoryItem>())
                                                            .ToList();

                    // 서브컬렉션 UNITS
                    var unitsSnapshot = await docSnap.Reference
                                                     .Collection(FirestoreCollection.Units)
                                                     .GetSnapshotAsync();
                    UserData.inventory.units = unitsSnapshot.Documents
                                                            .Select(doc => doc.ConvertTo<InventoryUnit>())
                                                            .ToList();
                    break;
                case FirestoreDocument.Info:
                    UserData.info = docSnap.ConvertTo<UserInfo>();
                    break;
                case FirestoreDocument.Stage:
                    UserData.stage = docSnap.ConvertTo<UserStage>();
                    // 서브컬렉션 PROGESSES
                    var progressesSnapshot = await docSnap.Reference
                                                        .Collection(FirestoreCollection.Progresses)
                                                        .GetSnapshotAsync();
                    UserData.stage.progresses = progressesSnapshot.Documents
                                                                  .Select(doc => doc.ConvertTo<StageProgress>())
                                                                  .ToList();
                    break;
                case FirestoreDocument.Collected:
                    UserData.collected = docSnap.ConvertTo<UserCollected>();
                    // 서브컬렉션 COLLECTS
                    var collectsSnapshot = await docSnap.Reference
                                                        .Collection(FirestoreCollection.Collects)
                                                        .GetSnapshotAsync();
                    UserData.collected.collects = collectsSnapshot.Documents
                                                                  .Select(doc => doc.ConvertTo<CollectionStatus>())
                                                                  .ToList();
                    break;
                default:
                    MyDebug.LogWarning($"SAVE 컬렉션에 알 수 없는 문서: {docId}");
                    break;
            }
        }
    }

    /// <summary>
    /// Save data 중 도감 정보 전체 조회 
    /// </summary>
    public static async Task<List<CollectionStatus>> GetUserCollectData(string uid)
    {
        var snapshot = await DB.Collection(FirestoreCollection.User)
                               .Document(uid)
                               .Collection(FirestoreCollection.Save)
                               .Document(FirestoreDocument.Collected)
                               .Collection(FirestoreCollection.Collects)
                               .GetSnapshotAsync();
        
        return snapshot.Documents.Select(d => d.ConvertTo<CollectionStatus>()).ToList();
    }
  
    /// <summary>
    /// 유저 인벤토리 - 보유 재화 정보 조회
    /// </summary>
    public static async Task<UserInventory> GetUserInventoryByUserId(string uid)
    {
        var snapshot = await DB.Collection(FirestoreCollection.User)
                               .Document(uid)
                               .Collection(FirestoreCollection.Save)
                               .Document(FirestoreDocument.Inventory)
                               .GetSnapshotAsync();
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<UserInventory>();
        }
        else
        {
            return default;
        }
    }
    
    /// <summary>
    /// 유저 인벤토리 - 보유 아이템 전체 조회
    /// </summary>
    public static async Task<List<InventoryItem>> GetAllUserInventoryItemByUserId(string uid)
    {
        var snapshot = await DB.Collection(FirestoreCollection.User)
                               .Document(uid)
                               .Collection(FirestoreCollection.Save)
                               .Document(FirestoreDocument.Inventory)
                               .Collection(FirestoreCollection.Items)
                               .GetSnapshotAsync();
        
        return snapshot.Documents.Select(d => d.ConvertTo<InventoryItem>()).ToList();
    }
    
    /// <summary>
    /// 유저 인벤토리 - 보유 아이템 단건 조회
    /// </summary>
    public static async Task<InventoryItem> GetUserInventoryItemByUserIdAndItemUid(string uid, string  itemUid)
    {
        var snapshot = await DB.Collection(FirestoreCollection.User)
                               .Document(uid)
                               .Collection(FirestoreCollection.Save)
                               .Document(FirestoreDocument.Inventory)
                               .Collection(FirestoreCollection.Items)
                               .Document(itemUid)
                               .GetSnapshotAsync();
        
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<InventoryItem>();
        }
        else
        {
            return default;
        }
    }
    
    public static async Task<UserStage> GetUserStageProgressByUserId(string uid)
    {
        var snapshot = await DB.Collection(FirestoreCollection.User)
                               .Document(uid)
                               .Collection(FirestoreCollection.Save)
                               .Document(FirestoreDocument.Inventory)
                               .Collection(FirestoreCollection.Items)
                               .Document("StageClearInfo")
                               .GetSnapshotAsync();
        
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<UserStage>();
        }
        else
        {
            return default;
        }
    }
    
    /// <summary>
    /// 가챠할 아이템 목록 조회 
    /// </summary>
    public static async Task<List<ItemData>> GetGachaEquipItems()
    {
        var db = FirebaseFirestore.DefaultInstance;
        
        var allowedTypes = new List<object>
        {
            ItemType.Armor.ToString(),
            ItemType.Weapon.ToString()
        };
        
        var query = db.Collection(FirestoreCollection.Data)
                      .Document(FirestoreDocument.RowData)
                      .Collection(FirestoreCollection.Item)
                      .WhereIn("type", allowedTypes)
                      .WhereEqualTo("isGacha", true);

        var snapshot = await query.GetSnapshotAsync();

        return snapshot.Documents.Select(d => d.ConvertTo<ItemData>()).ToList();
    }
    
}