using System.Collections;
using UnityEngine;

public class TouchEffectController : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private ParticleSystem psTouchEffect;
    [SerializeField] private ParticleSystem psDragEffect;
    
    public int touchPoolSize = 10;
    public int dragPoolSize = 1;

    private UIDynamicObjectPool<ParticleSystem> touchEffectPool;
    private UIDynamicObjectPool<ParticleSystem> dragEffectPool;

    private ParticleSystem dragEffectInstance;

    private Camera mainCamera
    {
        get
        {
            if (UIManager.Instance != null)
            {
                return UIManager.Instance.MainCamera;
            }
            else
            {
                return Camera.main;
            }
        }
    }

    private void Awake()
    {
        var ps = psTouchEffect.main;
        ps.useUnscaledTime = true;
        ps = psDragEffect.main;
        ps.useUnscaledTime = true;
        
        // 풀 초기화
        touchEffectPool = new UIDynamicObjectPool<ParticleSystem>(
            psTouchEffect, root, touchPoolSize
        );
        dragEffectPool = new UIDynamicObjectPool<ParticleSystem>(
            psDragEffect, root, dragPoolSize
        );
    }

    private void Update()
    {
#if UNITY_EDITOR
        // 마우스 테스트
        if (Input.GetMouseButtonDown(0))
        {
            SpawnTouchEffect(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }
        else if (Input.GetMouseButton(0))
        {
            UpdateDragEffect(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDragEffect();
        }
#else
        // 모바일 터치
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 pos = Camera.main.ScreenToWorldPoint(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    SpawnTouchEffect(pos);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    UpdateDragEffect(pos);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndDragEffect();
                    break;
            }
        }
#endif
    }

    private void SpawnTouchEffect(Vector2 pos)
    {
        var effect = touchEffectPool.Get();
        effect.transform.position = pos;
        effect.Play();

        // 파티클이 끝나면 자동 반환
        StartCoroutine(ReturnWhenFinished(effect, touchEffectPool));
    }

    private void UpdateDragEffect(Vector2 pos)
    {
        if (dragEffectInstance == null)
        {
            dragEffectInstance = dragEffectPool.Get();
            dragEffectInstance.transform.position = pos;
            dragEffectInstance.Play();
        }
        else
        {
            dragEffectInstance.transform.position = pos;
        }
    }

    private void EndDragEffect()
    {
        if (dragEffectInstance != null)
        {
            dragEffectInstance.Stop();
            StartCoroutine(ReturnWhenFinished(dragEffectInstance, dragEffectPool));
            dragEffectInstance = null;
        }
    }

    private IEnumerator ReturnWhenFinished(ParticleSystem ps, UIDynamicObjectPool<ParticleSystem> pool)
    {
        yield return new WaitWhile(() => ps.IsAlive(true));
        pool.Return(ps);
    }
}
