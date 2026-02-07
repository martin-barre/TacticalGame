using System.ComponentModel;
using UnityEngine;

public enum RectTransformPosition
{
    Top,
    TopLeft,
    TopRight,
    Bottom,
    BottomLeft,
    BottomRight,
    Left,
    Right
}

public static class RectTransformExtensions
{
    public static Vector3 GetScreenRectTransformPosition(this RectTransform source, RectTransformPosition rectTransformPosition)
    {
        Vector3[] corners = new Vector3[4];
        source.GetWorldCorners(corners);

        // 0 = bottom left
        // 1 = top left
        // 2 = top right
        // 3 = bottom right
            
        Vector3 position = rectTransformPosition switch
        {
            RectTransformPosition.Top => (corners[1] + corners[2]) * 0.5f,
            RectTransformPosition.TopLeft => corners[1],
            RectTransformPosition.TopRight => corners[2],
            RectTransformPosition.Bottom => (corners[0] + corners[3]) * 0.5f,
            RectTransformPosition.BottomLeft => corners[0],
            RectTransformPosition.BottomRight => corners[3],
            RectTransformPosition.Left => (corners[0] + corners[1]) * 0.5f,
            RectTransformPosition.Right => (corners[2] + corners[3]) * 0.5f,
            _ => throw new InvalidEnumArgumentException()
        };

        // Position en pixels dans l’écran
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, position);
        
        return screenPos;
    }
}
