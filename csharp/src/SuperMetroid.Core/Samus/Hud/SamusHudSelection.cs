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
    /// The selected item is one of the six HUD items, not a set of them. A new Select edge advances through the
    /// selection ring (the beam, Missiles, Super Missiles, Power Bombs, the Grapple Beam and the X-ray Scope),
    /// skipping ammunition Samus has run out of and equipment she has not equipped. Y cancels back to the beam.
    /// Holding Y while pressing Select records the chosen item for auto-cancel.
    /// </remarks>
    /// <returns>Whether a cancel or select edge took effect on the now-selected item.</returns>
    public static HudSelectionOutcome HandleHudSelection(SamusState samus,
        ControllerButton controllerInput,
        ControllerButton controllerNewInput,
        ControllerButton itemSwitchButton = ControllerButton.Select,
        ControllerButton itemCancelButton = ControllerButton.Y)
    {
        ControllerButton switchMask = itemSwitchButton;
        ControllerButton cancelMask = itemCancelButton;
        bool itemSelectionTookEffect = (controllerNewInput & (cancelMask | switchMask)) != ControllerButton.None;

        if ((controllerNewInput & cancelMask) != ControllerButton.None)
        {
            samus.AutoCancelledItem = HudItem.Beam;
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

            samus.AutoCancelledItem = (controllerInput & cancelMask) != ControllerButton.None
                ? samus.SelectedHudItem
                : HudItem.Beam;
        }
        return new HudSelectionOutcome(itemSelectionTookEffect);
    }

    /// <summary>
    /// Ends the use of an item chosen for auto-cancel (Select pressed while Item Cancel is held): the HUD returns to the
    /// beams and the auto-cancel is spent; an item chosen without Item Cancel held stays selected. The grapple's
    /// cancellation, drop, wall jump and release from a swing do this as the grapple ends, and X-ray as its beam ends.
    /// </summary>
    /// <param name="samus">Samus, whose selection and auto-cancel the use ends.</param>
    internal static void ReturnAutoCancelledItem(SamusState samus)
    {
        if (samus.AutoCancelledItem == HudItem.Beam)
            return;
        samus.SelectedHudItem = HudItem.Beam;
        samus.AutoCancelledItem = HudItem.Beam;
    }

    /// <summary>Whether Samus can select <paramref name="item"/>: the ammunition she has left and the equipment she has equipped.</summary>
    private static bool IsHudItemAvailable(SamusState samus, HudItem item) => item switch
    {
        HudItem.Beam => true,
        HudItem.Missile => samus.Missiles != 0,
        HudItem.SuperMissile => samus.SuperMissiles != 0,
        HudItem.PowerBomb => samus.PowerBombs != 0,
        HudItem.Grapple => samus.EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam),
        HudItem.XRay => samus.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope),
        _ => throw new InvalidDataException(
            $"HUD item {item} is not one of the six HUD items."),
    };
}
