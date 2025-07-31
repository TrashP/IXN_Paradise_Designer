using UnityEngine;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Demo;
using Polyperfect.Crafting.Integration;

public class WandSpellCaster : MonoBehaviour
{
    [Header("Equipment")]
    [SerializeField] EquippedSlot equippedSlot;
    [SerializeField] BaseItemObject wandItemObject;
    [SerializeField] Transform castPoint;
    [SerializeField] float castRange = 100f;

    [Header("Spell Module")]
    [SerializeField] MonoBehaviour spellModule; // should implement ISpellModule

    bool isHoldingWand = false;
    ISpellModule Module => spellModule as ISpellModule;

    void Start()
    {
        if (equippedSlot != null)
        {
            equippedSlot.OnContentsChanged.AddListener(HandleSlotChanged);
            HandleSlotChanged(equippedSlot.Slot?.Peek() ?? default);
        }
        else
        {
            Debug.LogWarning("[WandSpellCaster] No EquippedSlot assigned.");
        }
    }

    void Update()
    {
        if (!isHoldingWand || Module == null)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("[WandSpellCaster] Right click triggered.");
            Cast(Module.OnRightClick);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[WandSpellCaster] Left click triggered.");
            Cast(Module.OnLeftClick);
        }
    }

    void Cast(System.Action<RaycastHit, WandSpellCaster> action)
    {
        Ray ray = new Ray(castPoint.position, castPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, castRange))
        {
            action?.Invoke(hit, this);
        }
        else
        {
            Debug.Log("[WandSpellCaster] Raycast missed.");
        }
    }

    void HandleSlotChanged(ItemStack newItem)
    {
        isHoldingWand = newItem.ID == wandItemObject.ID;
        Debug.Log("[WandSpellCaster] Wand status: " + isHoldingWand);
    }
}
