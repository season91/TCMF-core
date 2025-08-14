using UnityEngine;
 
// 해상도 고정 카메라 스크립트
public class CameraResolution : MonoBehaviour
{
    void Start()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = new GameObject("Main Camera").AddComponent<Camera>();
        }
        Rect rect = mainCam.rect;
        float fixScreenRatio = (float)19 / 9; // 고정 화면비
        float deviceScreenRatio = (float)Screen.width / Screen.height; // 현재 디바이스의 화면비
        float rectHeight = deviceScreenRatio / fixScreenRatio;
        float rectWidth = 1f / rectHeight;
 
        // 상 하 공백
        if (rectHeight < 1)
        {
            rect.height = rectHeight;
            rect.y = (1f - rectHeight) / 2f;
        }
        // 좌 우 공백
        else
        {
            rect.width = rectWidth;
            rect.x = (1f - rectWidth) / 2f;
        }
        mainCam.rect = rect;
    }
}