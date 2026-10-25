using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>One frame of HandleSwitchingHudSelection ($90:C4B5): HUD item cycling and availability.</summary>
internal sealed class SamusHudSelection
{
    /// <summary>
    /// Selection stability: one on a selection change, then two after the selection remains stable for another
    /// frame, where it saturates; zero before the first update. The arm-cannon cover animation uses this
    /// two-frame gate.
    /// </summary>
    public int HudItemChangedThisFrame { get; set; }

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
    /// <returns>Whether an item's switched-to handler ran.</returns>
    public HudSelectionOutcome HandleHudSelection(SamusState samus, 
        ControllerButton controllerInput,
        ControllerButton controllerNewInput,
        ControllerButton itemSwitchButton = ControllerButton.Select,
        ControllerButton itemCancelButton = ControllerButton.Y)
    {
        HudItem previousSelection = samus.SelectedHudItem;
        ControllerButton switchMask = itemSwitchButton;
        ControllerButton cancelMask = itemCancelButton;
        bool itemHandlerAccepted = (controllerNewInput & (cancelMask | switchMask)) != ControllerButton.None;

        if ((controllerNewInput & cancelMask) != ControllerButton.None)
        {
            samus.AutoCancelHudItemIndex = HudItem.Beam;
            samus.SelectedHudItem = HudItem.Beam;
        }
        else if ((controllerNewInput & switchMask) != ControllerButton.None)
        {
            // `$90:C51D-$C53A` wraps after X-ray and repeatedly invokes the selected
            // handler while it returns carry set. The loop always terminates because
            // entry zero is the unconditional Nothing handler.
            do
            {
                samus.SelectedHudItem = HudItems.Next(samus.SelectedHudItem);
            }
            while (!IsHudItemAvailable(samus, samus.SelectedHudItem));

            samus.AutoCancelHudItemIndex = (controllerInput & cancelMask) != ControllerButton.None
                ? samus.SelectedHudItem
                : HudItem.Beam;
        }

        if (samus.SelectedHudItem != previousSelection)
        {
            HudItemChangedThisFrame = 1;
        }
        else
        {
            // Native saturates this word at two; it is a short stabilization counter,
            // never a general elapsed-time counter.
            HudItemChangedThisFrame = Math.Min(HudItemChangedThisFrame + 1, 2);
        }
        return new HudSelectionOutcome(itemHandlerAccepted);
    }

    /// <summary>Applies the native switched-to handlers' ammo and equipment admission rules.</summary>
    private static bool IsHudItemAvailable(SamusState samus, HudItem item) => item switch
    {
        HudItem.Beam => true,
        HudItem.Missile => samus.Missiles != 0,
        HudItem.SuperMissile => samus.SuperMissiles != 0,
        HudItem.PowerBomb => samus.PowerBombs != 0,
        HudItem.Grapple => samus.EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam),
        HudItem.XRay => samus.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope),
        _ => throw new InvalidDataException(
            $"HUD item {item} escaped the cartridge's six-entry switch table."),
    };
}
