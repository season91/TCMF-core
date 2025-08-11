using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP에서 화면 Clear가 제대로 되지 않을 때,
/// 매 프레임 렌더링 시작 전에 화면을 강제로 클리어해주는 Renderer Feature
/// </summary>
public class ForceClearFeature : ScriptableRendererFeature
{
    #region Before

    /// <summary>
    /// 실제 클리어 작업을 수행할 RenderPass
    /// </summary>
    class ClearPass : ScriptableRenderPass
    {
        /// <summary>
        /// ScriptableRenderContext: GPU에 명령을 전달할 수 있는 컨텍스트
        /// RenderingData: 카메라, 조명, 시간 등 현재 프레임의 렌더링 정보
        /// </summary>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 커맨드 버퍼 생성 (렌더 명령 모음)
            CommandBuffer cmd = CommandBufferPool.Get("ForceClear");

            // Render Target을 강제로 클리어함
            // 첫 번째 인자: 깊이 버퍼 지움 여부 (true)
            // 두 번째 인자: 컬러 버퍼 지움 여부 (true)
            // 세 번째 인자: 클리어할 색상 (여기선 검정색)
            cmd.ClearRenderTarget(true, true, Color.black);

            // 커맨드 버퍼 실행
            context.ExecuteCommandBuffer(cmd);

            // 커맨드 버퍼 반환 (메모리 풀에 다시 넣음)
            CommandBufferPool.Release(cmd);
        }
    }
    
    // 내부에서 사용할 클리어 패스 인스턴스
    ClearPass clearPass;

    /// <summary>
    /// Renderer Feature 초기화 시 호출됨
    /// 여기서 RenderPass를 생성하고 언제 실행될지 정의함
    /// </summary>
    public override void Create()
    {
        clearPass = new ClearPass
        {
            // 렌더링 순서 지정:
            // BeforeRendering → 씬, UI, 포스트프로세싱 렌더링 전에 실행
            renderPassEvent = RenderPassEvent.BeforeRendering
        };
    }

    /// <summary>
    /// 실제로 이 Feature가 렌더러에 등록될 때 호출됨
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 클리어 패스를 렌더링 큐에 추가함
        renderer.EnqueuePass(clearPass);
    }
    #endregion
}
