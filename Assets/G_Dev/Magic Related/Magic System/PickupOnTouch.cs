using UnityEngine;
using Polyperfect.Crafting.Integration;
using Polyperfect.Crafting.Framework;

//[RequireComponent(typeof(Collider))]
public class PickupOnTouch : MonoBehaviour
{
    [Tooltip("The item to give when player touches this object.")]
    [SerializeField] ObjectItemStack itemToGive;

    [Tooltip("Destroy the object after giving the item.")]
    [SerializeField] bool destroyOnPickup = true;

    void Reset()
    {
        // Make sure the collider is a trigger
        var col = GetComponent<Collider>();
        if (col)
        {
            col.isTrigger = true;
        }
    }

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col)
        {
            Debug.LogWarning("[PickupOnTouch] No collider found. This component requires a trigger collider to function.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning("[PickupOnTouch] Collider is not a trigger. Setting it to trigger.");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var inventory = other.GetComponentInChildren<IInsert<ItemStack>>();
        if (inventory == null)
            return;

        if (itemToGive?.ID.IsDefault() == false && itemToGive.Value > 0)
        {
            var stack = new ItemStack(itemToGive.ID, itemToGive.Value);
            var leftover = inventory.RemainderIfInserted(stack);

            if (leftover.Value < stack.Value)
            {
                inventory.InsertCompletely(stack); 
                Debug.Log($"[PickupOnTouch] Gave {stack.Value - leftover.Value}x {itemToGive.ID}");
                if (destroyOnPickup)
                    Destroy(gameObject);
            }
        }
    }
}
