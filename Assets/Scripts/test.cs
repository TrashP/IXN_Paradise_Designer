using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonDebug : MonoBehaviour, IPointerClickHandler
{
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        Vector2 localPoint;
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, null, out localPoint);
        Vector2 size = rectTransform.sizeDelta;
        Vector2 min = rectTransform.rect.min;
        Vector2 max = rectTransform.rect.max;
        Debug.Log($"Conversion success: {success} | Clicked at local position: {localPoint} | Screen position: {eventData.position} " +
                  $"| Button size: {size} | Rect min: {min}, max: {max}");
    }
}