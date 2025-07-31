using UnityEngine;

public interface ISpellModule
{
    void OnRightClick(RaycastHit hit, WandSpellCaster context);
    void OnLeftClick(RaycastHit hit, WandSpellCaster context);
}
