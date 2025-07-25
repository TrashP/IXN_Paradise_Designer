using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Color currentColor = Color.black;
    public Texture2D texture;
    public RawImage rawImage;
    public Slider brushSizeSlider;

    private bool isDrawing = false;
    private int brushSize = 1;

    void Start()
    {
        texture = new Texture2D(512, 512);
        rawImage.texture = texture;
        ClearCanvas();
    }

    void Update()
    {
        if (brushSizeSlider != null)
        {
            brushSize = Mathf.RoundToInt(brushSizeSlider.value);
        }
    }


    public void SetColor(Color color)
    {
        currentColor = color;
    }

    public void UseEraser()
    {
        currentColor = Color.white;
    }


    public void SaveImage()
    {
        byte[] bytes = texture.EncodeToPNG();

        string folderPath = Path.Combine(Application.persistentDataPath, "SavedImage");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = "drawing_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string filePath = Path.Combine(folderPath, fileName);

        File.WriteAllBytes(filePath, texture.EncodeToPNG());

        Debug.Log("Image saved to: " + filePath);

    }


    public void OnPointerDown(PointerEventData eventData)
    {
        isDrawing = true;
        Draw(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDrawing) Draw(eventData);
    }

    void Draw(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        Vector2 size = rawImage.rectTransform.rect.size;
        Vector2 pivot = rawImage.rectTransform.pivot;
        float x = (localPoint.x + size.x * pivot.x) / size.x;
        float y = (localPoint.y + size.y * pivot.y) / size.y;

        int px = Mathf.Clamp((int)(x * texture.width), 0, texture.width - 1);
        int py = Mathf.Clamp((int)(y * texture.height), 0, texture.height - 1);

        for (int dx = -brushSize; dx <= brushSize; dx++)
        {
            for (int dy = -brushSize; dy <= brushSize; dy++)
            {
                int nx = Mathf.Clamp(px + dx, 0, texture.width - 1);
                int ny = Mathf.Clamp(py + dy, 0, texture.height - 1);
                texture.SetPixel(nx, ny, currentColor);
            }
        }

        texture.Apply();
    }

    public void ClearCanvas()
    {
        Color32[] clearColors = new Color32[texture.width * texture.height];
        for (int i = 0; i < clearColors.Length; i++) clearColors[i] = Color.white;
        texture.SetPixels32(clearColors);
        texture.Apply();
    }
}
