using UnityEngine;
using ColorPicker;

public class ColorBridge : MonoBehaviour
{
    public ColorPicker.ColorPicker colorPicker;
    public DrawingCanvas drawingCanvas;

    void Start()
    {
        colorPicker.ColorSelectionChanged += OnColorChanged;
    }

    void OnColorChanged(Color newColor)
    {
        Debug.Log("Selected Color: " + newColor);
        drawingCanvas.SetColor(newColor);
    }
}