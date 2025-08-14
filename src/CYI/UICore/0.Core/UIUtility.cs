using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIUtility
{
    public static Vector2 WorldToCanvasPosition(Canvas canvas, Vector3 worldPos, Camera cam)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 screenPoint = cam.WorldToScreenPoint(worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            canvasRect,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }
}
