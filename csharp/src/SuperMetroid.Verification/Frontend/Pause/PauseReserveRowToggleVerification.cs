using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>
/// Confirms the first reserve row's same-frame equipment toggle (#1505): it repaints the last spread bomb's whole
/// vertical speed exactly as the cartridge routine does, and changes no equipment.
/// </summary>
internal static class PauseReserveRowToggleVerification
{
    /// <summary>The label sizes the dispatching handlers pass: the weapons column's, then the suit and boots columns'.</summary>
    private static readonly int[] HandlerLabelBytes = [0x0A, 0x12];

    /// <summary>
    /// Compares the service with the cartridge routine, transcribed from <c>$82:B5CA</c> (the unequip branch the reserve
    /// tables' null entries take) and <c>$82:A29D</c> (the palette loop through the cleared scratch pointer), over a
    /// low work-RAM image, for each handler's label size and a range of whole speeds.
    /// </summary>
    internal static void VerifyReserveRowToggleMatchesCartridgeRoutine()
    {
        foreach (int labelBytes in HandlerLabelBytes)
        foreach (short speed in new short[] { 0, 2, 5, -3, -1, 0x1234, 0x1C00, short.MinValue, short.MaxValue })
        {
            ushort[] lowRam = new ushort[0x1000];
            const int lastBombVerticalSpeed = 0x0C02 / 2;
            lowRam[0] = 0x00F2;
            lowRam[lastBombVerticalSpeed] = unchecked((ushort)speed);
            // The equipment word and its mask are both read through the null table entries: the scratch word itself.
            ushort equipment = lowRam[0], mask = lowRam[0];
            AssertTrue((equipment & mask) != 0, "the row's label offset tests as equipped, taking the unequip branch");
            lowRam[0] = (ushort)(equipment & ~mask);
            for (int offset = 0, remaining = labelBytes; remaining != 0; offset += 2, remaining -= 2)
            {
                // The loop re-reads its pointer from the scratch word on every write, so its first write moves it.
                int word = (lowRam[0] + offset) / 2;
                lowRam[word] = (ushort)((lowRam[word] & 0xE3FF) | 0x0C00);
            }
            short expected = unchecked((short)lowRam[lastBombVerticalSpeed]);

            AssertEqual(expected, ReserveRowToggleSpreadBombSpeed.Repaint(speed),
                $"last spread bomb's whole speed {speed} after a {labelBytes}-byte toggle matches the cartridge");
        }
    }

    /// <summary>
    /// Drives the pause screen through the toggle itself: from the top beam, Up and A on one frame move into the
    /// reserve column's first row and toggle there. It repaints the last bomb's speed, changes no equipment, and no
    /// longer stops the game.
    /// </summary>
    internal static void VerifyReserveRowToggleThroughPauseScreen()
    {
        var catalog = RetailPresentationFixture();
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            CollectedBeams = SamusBeamFlags.Charge, EquippedBeams = SamusBeamFlags.Charge, Health = 99, MaxHealth = 99,
            MaxReserveEnergy = 100, ReserveEnergy = 100, ReserveTankMode = ReserveTankMode.Auto,
        };
        var menu = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = catalog, SpriteSheets = MenuSheetFixtures.For(catalog) });
        EnterPauseEquipment(menu);
        AssertEqual(PauseEquipmentCategory.Reserves, menu.SelectedCategory, "with a reserve tank the cursor starts in the reserve column");
        menu.Step(0, ControllerButton.Down);
        AssertEqual(PauseEquipmentCategory.Beams, menu.SelectedCategory, "Down from the automatic reserve mode moves to the beams");
        AssertEqual(0, menu.SelectedItem, "the cursor is on the top beam");
        var bombs = new SamusBombProjectileSystem();
        foreach (SamusBombProjectileSlot slot in bombs.Slots)
            slot.BombSpreadVerticalVelocity = SignedSixteenSixteen.FromParts(2, 0x4321);

        menu.Step(0, ControllerButton.Up | ControllerButton.A, null, 0, bombs);

        AssertEqual(PauseEquipmentCategory.Reserves, menu.SelectedCategory, "Up from the top beam moves into the reserve column");
        AssertEqual(0, menu.SelectedItem, "the move keeps the first row");
        AssertEqual(SignedSixteenSixteen.FromParts(ReserveRowToggleSpreadBombSpeed.Repaint(2), 0x4321), bombs.Slots[^1].BombSpreadVerticalVelocity,
            "the toggle repaints the last spread bomb's whole speed and keeps its fraction");
        for (int index = 0; index < bombs.Slots.Count - 1; index++)
            AssertEqual(SignedSixteenSixteen.FromParts(2, 0x4321), bombs.Slots[index].BombSpreadVerticalVelocity, $"bomb slot {index} is untouched");
        AssertEqual(SamusBeamFlags.Charge, samus.EquippedBeams, "the toggle changes no beam");
        AssertEqual(SamusEquipmentFlags.None, samus.EquippedItems, "the toggle changes no item");
    }
}
