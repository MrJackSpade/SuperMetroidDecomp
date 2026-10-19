using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Samus.Hud;

/// <summary>Checks the bank $90 HUD selection cycle, availability skipping, and cancel-edge handling.</summary>
internal static class SamusHudSelectionVerification
{
    /// <summary>Runs one selector frame; true when it moved the selected index.</summary>
    internal static bool SelectionChanged(SamusState samus, SnesButton controllerInput, SnesButton controllerNewInput)
    {
        HudItem before = samus.SelectedHudItem;
        samus.HudSelection.HandleHudSelection(controllerInput, controllerNewInput);
        return samus.SelectedHudItem != before;
    }

    /// <summary>
    /// Verifies the six-entry bank-$90 HUD selector independently of weapon production.
    /// This is the controller path used to equip the first Missile before Flyway's red door.
    /// </summary>
    internal static void VerifySamusHudSelection()
    {
        const SnesButton select = SnesButton.Select;
        const SnesButton cancel = SnesButton.Y;

        var samus = new SamusState();
        AssertTrue(!SelectionChanged(samus, select, select),
            "Select with no available HUD item wraps to Nothing");
        AssertEqual(HudItem.Beam, samus.SelectedHudItem,
            "unavailable six-entry cycle retains Nothing");
        AssertEqual(1, samus.HudSelection.HudItemChangedThisFrame,
            "unchanged first selector frame advances stabilization counter");

        samus.Missiles = 5;
        AssertTrue(SelectionChanged(samus, select, select),
            "Select chooses available Missiles");
        AssertEqual(HudItem.Missile, samus.SelectedHudItem,
            "Missiles occupy cartridge HUD index one");
        AssertEqual(1, samus.HudSelection.HudItemChangedThisFrame,
            "selection change resets native cover-animation counter");
        AssertTrue(!SelectionChanged(samus, 0, 0),
            "stable HUD selection reports no change");
        AssertEqual(2, samus.HudSelection.HudItemChangedThisFrame,
            "stable selection saturates cover-animation counter at two");

        samus.SuperMissiles = 5;
        AssertTrue(SelectionChanged(samus, select, select),
            "second Select advances from Missiles to Super Missiles");
        AssertEqual(HudItem.SuperMissile, samus.SelectedHudItem,
            "Super Missiles occupy cartridge HUD index two");

        // Power Bombs are absent, so the next handler returns carry and the native loop
        // continues to the first equipped non-ammo item instead of stopping at index three.
        samus.EquippedItems = SamusEquipmentFlags.GrappleBeam;
        AssertTrue(SelectionChanged(samus, select, select),
            "selector skips unavailable Power Bombs");
        AssertEqual(HudItem.Grapple, samus.SelectedHudItem,
            "Grapple occupies cartridge HUD index four");

        samus.EquippedItems |= SamusEquipmentFlags.XrayScope;
        AssertTrue(SelectionChanged(samus, select, select),
            "selector advances from Grapple to equipped X-ray");
        AssertEqual(HudItem.XRay, samus.SelectedHudItem,
            "X-ray occupies cartridge HUD index five");
        AssertTrue(SelectionChanged(samus, select, select),
            "selector wraps after X-ray");
        AssertEqual(HudItem.Beam, samus.SelectedHudItem,
            "six-entry selector wraps to Nothing");

        // Y is held but is not a new edge here. Native therefore performs Select's cycle
        // and remembers the chosen item; a later new Y edge cancels both words together.
        AssertTrue(SelectionChanged(samus,
                (select | cancel),
                select),
            "held item-cancel records Select auto-cancel target");
        AssertEqual(HudItem.Missile, samus.SelectedHudItem,
            "held-cancel Select still equips Missiles");
        AssertEqual(HudItem.Missile, samus.AutoCancelHudItemIndex,
            "held-cancel Select stores equipped index");
        AssertTrue(SelectionChanged(samus, cancel, cancel),
            "new item-cancel edge deselects current item");
        AssertEqual(HudItem.Beam, samus.SelectedHudItem,
            "item-cancel returns to Nothing");
        AssertEqual(HudItem.Beam, samus.AutoCancelHudItemIndex,
            "item-cancel clears auto-cancel index");

        // $90:C4C7 runs the Nothing handler for any new Y edge, so a cancel with nothing
        // selected changes no index but still clears the charge.
        HudItem beforeIdleCancel = samus.SelectedHudItem;
        HudSelectionOutcome idleCancel = samus.HudSelection.HandleHudSelection(cancel, cancel);
        AssertTrue(samus.SelectedHudItem == beforeIdleCancel && idleCancel.ItemHandlerAccepted,
            "item-cancel with nothing selected runs the Nothing handler");
        AssertTrue(!samus.HudSelection.HandleHudSelection(cancel, 0).ItemHandlerAccepted,
            "held item-cancel without a new edge runs no handler");

        Console.WriteLine(
            "  Samus HUD selection: availability skipping, wrap, counters, and auto-cancel agree.");
    }
}
