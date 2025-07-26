using UnityEngine;
using UnityEngine.UI;

public class ColorPickerController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Slider redSlider, greenSlider, blueSlider;
    public Image preview;
    public Button applyButton, cancelButton;

    [Header("Drawing Reference")]
    public DrawingCanvas drawingCanvas;

    void Start()
    {
        panel.SetActive(false);

        redSlider.onValueChanged.AddListener(UpdatePreview);
        greenSlider.onValueChanged.AddListener(UpdatePreview);
        blueSlider.onValueChanged.AddListener(UpdatePreview);

        applyButton.onClick.AddListener(ApplyColor);
        cancelButton.onClick.AddListener(ClosePanel);
    }

    public void OpenPanel()
    {
        panel.SetActive(true);
        UpdatePreview(0);
        Debug.Log("Panel status: " + panel.name + " | Active? " + panel.activeSelf);
        Debug.Log("Turn on the palette");
    }

    void UpdatePreview(float _)
    {
        Color c = new Color(redSlider.value, greenSlider.value, blueSlider.value, 1f);
        preview.color = c;
    }

    void ApplyColor()
    {
        Color chosen = new Color(redSlider.value, greenSlider.value, blueSlider.value, 1f);
        drawingCanvas.SetColor(chosen);
        ClosePanel();
    }

    void ClosePanel()
    {
        panel.SetActive(false);
    }
}
