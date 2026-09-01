using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    /// <summary>
    /// WRAM <c>$0AAA</c>: one on a selection change, then two after the selection remains
    /// stable for another frame. The arm-cannon cover animation uses this two-frame gate.
    /// </summary>
    public ushort HudItemChangedThisFrame { get; set; }

    /// <summary>
    /// Ports <c>HandleSwitchingHudSelection</c> at <c>$90:C4E7</c> for the retail default
    /// item-switch and item-cancel bindings.
    /// </summary>
    /// <remarks>
    /// The selected item is a six-entry index, not a bit field: zero is beams/nothing,
    /// one missiles, two Super Missiles, three Power Bombs, four Grapple, and five X-ray.
    /// A new Select edge advances through that ring, calling each cartridge handler to
    /// skip unavailable ammo/equipment. Y cancels to zero. Holding Y while pressing Select
    /// records the chosen item for native auto-cancel behavior.
    /// </remarks>
    /// <returns>True when the selected item index changed on this frame.</returns>
    public bool HandleHudSelection(
        ushort controllerInput,
        ushort controllerNewInput,
        SnesButton itemSwitchButton = SnesButton.Select,
        SnesButton itemCancelButton = SnesButton.Y)
    {
        ushort previousSelection = SelectedHudItem;
        ushort switchMask = (ushort)itemSwitchButton;
        ushort cancelMask = (ushort)itemCancelButton;

        if ((controllerNewInput & cancelMask) != 0)
        {
            AutoCancelHudItemIndex = 0;
            SelectedHudItem = 0;
        }
        else if ((controllerNewInput & switchMask) != 0)
        {
            // `$90:C51D-$C53A` wraps after X-ray and repeatedly invokes the selected
            // handler while it returns carry set. The loop always terminates because
            // entry zero is the unconditional Nothing handler.
            do
            {
                SelectedHudItem++;
                if (SelectedHudItem >= 6)
                    SelectedHudItem = 0;
            }
            while (!IsHudItemAvailable(SelectedHudItem));

            AutoCancelHudItemIndex = (controllerInput & cancelMask) != 0
                ? SelectedHudItem
                : (ushort)0;
        }

        bool changed = SelectedHudItem != previousSelection;
        if (changed)
        {
            HudItemChangedThisFrame = 1;
        }
        else
        {
            // Native saturates this word at two; it is a short stabilization counter,
            // never a general elapsed-time counter.
            HudItemChangedThisFrame = HudItemChangedThisFrame >= 2
                ? (ushort)2
                : unchecked((ushort)(HudItemChangedThisFrame + 1));
        }
        return changed;
    }

    private bool IsHudItemAvailable(ushort index) => index switch
    {
        0 => true,
        1 => Missiles != 0,
        2 => SuperMissiles != 0,
        3 => PowerBombs != 0,
        4 => EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam),
        5 => EquippedItems.HasAny(SamusEquipmentFlags.XrayScope),
        _ => throw new InvalidDataException(
            $"HUD item index {index} escaped the cartridge's six-entry switch table."),
    };
}
