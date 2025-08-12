using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
// Addressable 전용 using문
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager
{
    private static ResourceManager instance = null;
    public static ResourceManager Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new ResourceManager();
            }
            return instance;
        }
    }

    private readonly Dictionary<string, Object> resources = new(); // key: Addressable PrimaryKey
    private readonly Dictionary<AsyncOperationHandle, List<string>> loadedAssetHandles = new(); // 핸들과 resources의 key 매핑
    private readonly List<GameObject> loadedInstanceHandles = new();
    private readonly Dictionary<string, SettingEntityOffset> settingOffsetDict = new();
    
    public async Task LoadAssets(string labelFront)
    {
        await LoadAssetsByLabel<GameObject>(labelFront + StringAdrLabelBack.Object);
        await LoadAssetsByLabel<Sprite>(labelFront + StringAdrLabelBack.Sprite);
        await LoadAssetsByLabel<AudioClip>(labelFront + StringAdrLabelBack.AudioClip);
        await InstantiateUIPrefabsInBatches();
    }

    public async Task Initialize()
    {
        var handle = Addressables.InitializeAsync();
        await handle.Task;
        await LoadAssets(StringAdrLabelFront.EntryScene);
    }
    
    #region Util Task
    
    // Timeout Task
    private async Task<bool> LoadWithTimeoutAndProgress<T>(
        AsyncOperationHandle<T> handle,
        float timeoutSeconds,
        Action<float> onProgress = null)
    {
        var timeoutTask = Task.Delay((int)(timeoutSeconds * 1000));
        var progressTask = WaitUntilDoneWithGUI(handle, onProgress);

        var completedTask = await Task.WhenAny(progressTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            MyDebug.LogError("Load Resource Timeout");
            return false;
        }

        return handle.Status == AsyncOperationStatus.Succeeded;
    }

    // Progress Task
    private async Task WaitUntilDoneWithGUI<T>(AsyncOperationHandle<T> handle, Action<float> onProgress)
    {
        float lastProgress = -1f;

        while (!handle.IsDone)
        {
            float progress = handle.PercentComplete;
            if (onProgress != null && Mathf.Abs(progress - lastProgress) > 0.01f)
            {
                lastProgress = progress;
                onProgress(progress);
            }
            await Task.Yield();
        }

        // 최종 100% 보장
        onProgress?.Invoke(1f);
    }
    
    #endregion
    
    #region Load Resource By Label

    /// <summary>
    /// Addressable 리소스를 Label 기준으로 로드하고 Progress UI 업데이트 (타입 지정 필요)
    /// </summary>
    private async Task LoadAssetsByLabel<T>(string label) where T : Object
    {
        // 리소스 위치 로딩
        var locationHandle = Addressables.LoadResourceLocationsAsync(label);
        await locationHandle.Task;

        var locations = locationHandle.Result
            .Where(loc => loc.ResourceType == typeof(T))
            .ToList();

        if(locations.Count <= 0)
            return;
        
        // 에셋 로딩
        var assetHandle = Addressables.LoadAssetsAsync<T>(locations, null); // null = 콜백 없음

        bool success = await LoadWithTimeoutAndProgress(
            assetHandle,
            10f,
            progress =>
            {
                // 전체 리소스 로딩 가중치
                progress *= LoadType.Resources.Weight() / 2;
                UIManager.Instance.UpdateProgressBar(progress, true);
            });

        if (!success)
        {
            MyDebug.LogError($"LoadAssetsByLabel<{typeof(T)}> failed: {label}");
            return;
        }

        for (int i = 0; i < assetHandle.Result.Count; i++)
        {
            var asset = assetHandle.Result[i];
            if (asset == null) continue;
            
            var address = locations[i].PrimaryKey;
            
            if (!resources.ContainsKey(address))
            {
                resources[address] = asset;
            }
        }
        
        MyDebug.Log($"Loaded: 어드레서블 타입{typeof(T)} 등록");
        loadedAssetHandles.Add(assetHandle, locations.Select(loc => loc.PrimaryKey).ToList());
    }
    
    /// <summary>
    /// UI 프리팹 인스턴스화를 배치 단위로 분산 실행
    /// </summary>
    public async Task InstantiateUIPrefabsInBatches(int batchSize = 1, Action<float> onProgress = null)
    {
        // UI 리소스들만 리스트화
        var uiKeys = resources
            .Where(kv => kv.Value is GameObject obj && obj.GetComponent<UIBase>() != null)
            .Select(kv => kv.Key)
            .ToList();
        
        int total = uiKeys.Count;
        int processed = 0;
        
        // UI 프리팹들을 UIManager의 자식 오브젝트로 생성 및 등록
        foreach (var key in uiKeys)
        {
            var instHandle = Addressables.InstantiateAsync(key, UIManager.Instance.transform);
            await instHandle.Task;

            var instanceUI = instHandle.Result;
            loadedInstanceHandles.Add(instanceUI);
            instanceUI.GetComponent<UIBase>().RegisterToManager();

            processed++;
            onProgress?.Invoke((float)processed / total);

            // 배치 사이에 한 프레임 쉬기
            if (processed % batchSize == 0)
            {
                await Task.Yield();
            }
        }
    }
    
    /// <summary>
    /// 해당 sceneLabel의 모든 Addressable 리소스 핸들을 해제
    /// </summary>
    public async Task UnloadResourcesByLabel()
    {
        // 주의할 점
        // Instantiate한 오브젝트를 먼저 정리해야 함
        if (loadedInstanceHandles != null)
        {
            foreach (var instance in loadedInstanceHandles)
            {
                Addressables.ReleaseInstance(instance);
            }
            UIManager.Instance.ClearInstanceUIList();
            loadedInstanceHandles.Clear();
        }

        if (loadedAssetHandles != null)
        {
            foreach (var kvp in loadedAssetHandles)
            {
                var handle = kvp.Key;
                var keys = kvp.Value;

                if (handle.IsValid())
                {
                    if (handle.Result is IEnumerable<Object> objects)
                    {
                        int i = 0;
                        foreach (var obj in objects)
                        {
                            if (obj != null && i < keys.Count)
                                resources.Remove(keys[i]);
                            i++;
                        }
                    }
                    Addressables.Release(handle);
                }
            }
            loadedAssetHandles.Clear();
        }

        await Task.Yield();
    }
    #endregion
    
    /// <summary>
    /// 해당 sceneLabel의 모든 Addressable 리소스 핸들을 해제
    /// Resource 불러올 때, ResourceManager.Instance.GetResource<T>(); 해주면 됨!
    /// </summary>
    public T GetResource<T>(string key) where T : Object
    {
        if (!resources.TryGetValue(key, out var obj))
        {
            MyDebug.LogWarning($"this Resource is not exist: Key = {key}");
            return null;
        }

        T objAsT = obj as T;
        if (objAsT == null)
        {
            MyDebug.LogWarning($"this Resource is not this Type: Key = {key}, Type = {typeof(T)}");
            return null;
        }
        return objAsT;
    }
    
    /// <summary>
    /// 범용적으로 사용 가능한 SO 전용 로더
    /// </summary>
    private T GetScriptableObject<T>(string key) where T : ScriptableObject
    {
        return GetResource<T>(key);
    }
    
    public SettingEntityOffset GetSettingOffset(string key)
    {
        if (settingOffsetDict.TryGetValue(key, out var cached))
            return cached;

        var so = GetScriptableObject<SettingEntityOffset>(key);
        if (so != null)
            settingOffsetDict[key] = so;

        return so;
    }
    
    public async Task LoadAdrSceneAsync(string sceneAdr)
    {
        AsyncOperationHandle sceneLoadOp = Addressables.LoadSceneAsync(sceneAdr);
        await sceneLoadOp.Task;
        // while (!sceneLoadOp.IsDone)
        // {
        //     float progress = LoadType.Scene.Weight() * sceneLoadOp.PercentComplete;
        //     UIManager.Instance.UpdateProgressBar(progress);  
        //     await Task.Yield();
        // }

        float progress = LoadType.Scene.Weight();
        UIManager.Instance.UpdateProgressBar(progress, true);
    }
    
    public async Task LoadAdrSceneWithoutProgressBarAsync(string sceneAdr)
    {
        AsyncOperationHandle sceneLoadOp = Addressables.LoadSceneAsync(sceneAdr);
        await sceneLoadOp.Task;
    }
    #region 백업

    // 사실 UI Manager가 해야할 일.
    // 프리팹같은 오브젝트만 받올 일이고
    // Resource 매니저가 UIBase를 가지고 있는지의 여부는 알 이유가 없다.
    
    // 씬이 시작될 때, 씬 트리거 같은 씬에서 필요한 모든 오브젝트를 동기적으로 불러올 수 있는 구조를 만드렁두고
    // 리소스매니저한테 지금 어떤 라벨의 데이터가 필요하다, 이걸 너가 불러와서 메모리에 올려둠
    // 필요한 리소스가 생기면 받아옴 => GetResource
    // 제네릭 
    
    // 메모리에 프리팹을 올려두고 컬렉션으로 관리해도
    // 유아이에서 프리팹 필요하다고 하면 프리팹만 받아와서 -> InstantiateAsync
    
    // 이 부분은 묶을 수 있을 것 같은데 하는 생각이 드는 부분이 있으면 그때 리팩토링 하자
    // 구조에 집착하지 말쟈
    // 정리하면서 구조 정리하면 될 듯
    // 공통적인 부분들만 !
    #endregion
    
    #region 튜터링
    
        // private Dictionary<Type, Action<Object>> _handlers = new();
        //
        // void RegisterHandlers()
        // {
        //     _handlers[typeof(Sprite)] = obj => {
        //         var sprite = obj as Sprite;
        //         //스프라이트 로드 했을 때 뭐할지
        //     };
        //
        //     _handlers[typeof(GameObject)] = obj => {
        //         var go = obj as GameObject;
        //         //게임오브젝트 로드해서 뭐할지
        //     };
        // }
        //
        // public void LoadByLabel(string label)
        // {
        //     Addressables.LoadAssetsAsync<object>(label, obj =>
        //     {
        //         var type = obj.GetType();
        //         if (_handlers.TryGetValue(type, out var handler))
        //         {
        //             handler.Invoke(obj);
        //         }
        //         else
        //         {
        //             //오류처리
        //         }
        //     });
        // }
        
        #endregion
}
