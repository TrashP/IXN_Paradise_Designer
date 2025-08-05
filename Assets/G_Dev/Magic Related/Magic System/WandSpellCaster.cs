using Polyperfect.Crafting.Demo;
using Polyperfect.Crafting.Integration;
using UnityEngine;
using UnityEngine.UI;

public class WandSpellCaster : MonoBehaviour
{
    [Header("Equipment")]
    [SerializeField] EquippedSlot equippedSlot;
    [SerializeField] BaseItemObject wandItemObject;
    [SerializeField] Transform castPoint;
    [SerializeField] float castRange = 100f;

    [Header("Spell Module")]
    [SerializeField] MonoBehaviour spellModule;

    [Header("Crosshair Settings")]
    [SerializeField] Object crosshairTexture;
    [SerializeField] Vector2 crosshairSize = new Vector2(32, 32);
    [SerializeField] Color crosshairColor = Color.white;

    private GameObject crosshairGO;
    private Image crosshairImage;

    bool isHoldingWand = false;
    ISpellModule Module => spellModule as ISpellModule;

    void Start()
    {
        SetupCrosshair();

        if (equippedSlot != null)
        {
            equippedSlot.OnContentsChanged.AddListener(HandleSlotChanged);
            HandleSlotChanged(equippedSlot.Slot?.Peek() ?? default);
        }
    }

    void Update()
    {
        if (!isHoldingWand || Module == null)
            return;

        if (Input.GetMouseButtonDown(1))
            Cast(Module.OnRightClick);

        if (Input.GetMouseButtonDown(0))
            Cast(Module.OnLeftClick);
    }

    void Cast(System.Action<RaycastHit, WandSpellCaster> action)
    {
        Ray ray = new Ray(castPoint.position, castPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, castRange))
            action?.Invoke(hit, this);
    }

    void HandleSlotChanged(ItemStack newItem)
    {
        isHoldingWand = newItem.ID == wandItemObject.ID;
        if (crosshairGO)
            crosshairGO.SetActive(isHoldingWand);
    }

    void SetupCrosshair()
    {
        if (!crosshairTexture)
            return;

        Sprite spriteToUse = null;

        // Convert Texture2D to Sprite if needed
        if (crosshairTexture is Texture2D tex2D)
        {
            spriteToUse = Sprite.Create(
                tex2D,
                new Rect(0, 0, tex2D.width, tex2D.height),
                new Vector2(0.5f, 0.5f),
                100f // default pixelsPerUnit
            );
        }
        else if (crosshairTexture is Sprite sprite)
        {
            spriteToUse = sprite;
        }

        if (!spriteToUse)
        {
            Debug.LogWarning("[WandSpellCaster] Crosshair texture is invalid.");
            return;
        }

        // Create Canvas
        var canvasGO = new GameObject("CrosshairCanvas", typeof(Canvas));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        DontDestroyOnLoad(canvasGO);

        // Create Crosshair Image
        crosshairGO = new GameObject("Crosshair", typeof(Image));
        crosshairGO.transform.SetParent(canvas.transform);
        crosshairImage = crosshairGO.GetComponent<Image>();
        crosshairImage.sprite = spriteToUse;
        crosshairImage.color = crosshairColor;
        crosshairImage.rectTransform.sizeDelta = crosshairSize;
        crosshairImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairImage.rectTransform.anchoredPosition = Vector2.zero;

        crosshairGO.SetActive(false);
    }

}
