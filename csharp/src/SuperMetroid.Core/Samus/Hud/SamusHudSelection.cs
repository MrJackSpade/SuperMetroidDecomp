using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>One frame of HUD selection switching: HUD item cycling and availability.</summary>
internal static class SamusHudSelection
{
    /// <summary>
    /// Switches the HUD selection for the retail default
    /// item-switch and item-cancel bindings.
    /// </summary>
    /// <remarks>
    /// The selected item is a six-entry index, not a bit field: zero is beams/nothing,
    /// one missiles, two Super Missiles, three Power Bombs, four Grapple, and five X-ray.
    /// A new Select edge advances through that ring, calling each item's handler to
    /// skip unavailable ammo/equipment. Y cancels to zero. Holding Y while pressing Select
    /// records the chosen item for auto-cancel behavior.
    /// </remarks>
    /// <returns>Whether an item's switched-to handler ran.</returns>
    public static HudSelectionOutcome HandleHudSelection(SamusState samus,
        ControllerButton controllerInput,
        ControllerButton controllerNewInput,
        ControllerButton itemSwitchButton = ControllerButton.Select,
        ControllerButton itemCancelButton = ControllerButton.Y)
    {
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
            // The selection ring wraps after X-ray and keeps advancing while the selected
            // item is unavailable. The loop always terminates because the first item,
            // Nothing, is always available.
            do
            {
                samus.SelectedHudItem = HudItems.Next(samus.SelectedHudItem);
            }
            while (!IsHudItemAvailable(samus, samus.SelectedHudItem));

            samus.AutoCancelHudItemIndex = (controllerInput & cancelMask) != ControllerButton.None
                ? samus.SelectedHudItem
                : HudItem.Beam;
        }
        return new HudSelectionOutcome(itemHandlerAccepted);
    }

    /// <summary>
    /// Ends the use of an item chosen for auto-cancel (Select pressed while Item Cancel is held): the HUD returns to the
    /// beams and the auto-cancel is spent; an item chosen without Item Cancel held stays selected. The grapple's
    /// cancellation, drop, wall jump and release from a swing do this as the grapple ends, and X-ray as its beam ends.
    /// </summary>
    /// <param name="samus">Samus, whose selection and auto-cancel the use ends.</param>
    internal static void ReturnAutoCancelledItem(SamusState samus)
    {
        if (samus.AutoCancelHudItemIndex == HudItem.Beam)
            return;
        samus.SelectedHudItem = HudItem.Beam;
        samus.AutoCancelHudItemIndex = HudItem.Beam;
    }

    /// <summary>Applies the switched-to handlers' ammo and equipment admission rules.</summary>
    private static bool IsHudItemAvailable(SamusState samus, HudItem item) => item switch
    {
        HudItem.Beam => true,
        HudItem.Missile => samus.Missiles != 0,
        HudItem.SuperMissile => samus.SuperMissiles != 0,
        HudItem.PowerBomb => samus.PowerBombs != 0,
        HudItem.Grapple => samus.EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam),
        HudItem.XRay => samus.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope),
        _ => throw new InvalidDataException(
            $"HUD item {item} escaped the six-entry switch table."),
    };
}
