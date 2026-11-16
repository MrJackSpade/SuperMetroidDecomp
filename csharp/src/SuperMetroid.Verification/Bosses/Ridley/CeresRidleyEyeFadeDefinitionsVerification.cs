using SuperMetroid.Core.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Ridley;

using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>Checks Ceres Ridley's compiled eye-fade row schedule and body-fade handoff.</summary>
internal static class CeresRidleyEyeFadeDefinitionsVerification
{
    /// <summary>
    /// Plays the whole eye fade, its sixteen shades and its hold (#2247), against the cartridge's per-update schedule and its
    /// terminator while forbidding live schedule reads.
    /// </summary>
    internal static void VerifyCeresRidleyEyeFadeDefinitions(CartridgeImportAddressSpace rom)
    {
        // The cartridge's schedule names the eye shade each update shows and ends with a terminator after the hold.
        byte[] nativeSchedule = [.. Enumerable.Range(0, ScheduleUpdates)
            .Select(update => rom.ReadCartridgeByte(RidleyEyeFadeDefinitions.NativeScheduleAddress + update))];
        AssertEqual((byte)0xff,
            rom.ReadCartridgeByte(RidleyEyeFadeDefinitions.NativeScheduleAddress + ScheduleUpdates),
            "Ceres Ridley eye-fade native terminator follows the shades and the hold");

        var guard = NewCeresRidleyEyeFadeReadGuard(rom);
        CeresRidleyColorCatalog colours = RetailImportPresentationFixture().CeresRidleyColors;
        IReadOnlyList<SuperMetroid.AssetExtraction.Artwork.SheetShade> shades = SuperMetroid.AssetExtraction.Bosses.RidleySheetShades.CeresSprites(colours);
        Action<CeresRidleyState> tick = RidleyReveal.TickEyeFadeIn;

        var state = new CeresRidleyState(Position16<RoomPixels>.Zero) { RevealPhase = RidleyRevealPhase.FadeInEyes, MovementAnimationEnabled = false };
        for (int update = 0; update < ScheduleUpdates; update++)
        {
            tick(state);

            // The colours the encounter shows are the import's shades of its installed colours, laid as the fade laid them (#1512).
            ushort[] cgram = RidleyLaidColours.Of(state, shades, [], 0);
            AssertEqual(RidleyRevealPhase.FadeInEyes, state.RevealPhase,
                $"production Ceres eye-fade update {update} remains in fade phase");
            for (int color = 0; color < 3; color++)
            {
                int source = EnemyRomTablePointers.Ceres.RidleyEyeFadePaletteRows +
                    nativeSchedule[update] * 6 + color * 2;
                ushort expectedColor = CartridgeImportSource.Require(rom).ReadCartridgeWord(source);
                AssertEqual(expectedColor, cgram[252 + color],
                    $"production Ceres eye-fade update {update} color {color}");
            }
        }
        AssertTrue(state.EyeFade.IsComplete, "production Ceres eye fade completes after its shades and its hold");

        tick(state);
        // Each fade keeps its own progress (#2228): the completed eye fade stays complete, and the body fade starts at black.
        AssertTrue(state.EyeFade.IsComplete,
            "production Ceres eye-fade completion keeps the fade complete");
        AssertEqual(0, state.BodyFade.Shade, "production Ceres body fade starts from black");
        AssertEqual(RidleyRevealPhase.FadeInBody, state.RevealPhase,
            "production Ceres eye-fade terminator enters body fade");
        AssertTrue(state.MovementAnimationEnabled,
            "production Ceres eye-fade terminator enables movement animation");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Ceres eye fade performs no schedule reads");

        Console.WriteLine(
            "Ceres Ridley eye fade: its sixteen shades and 48-update hold match all 64 native row selections, and the " +
            "terminator and body-fade handoff pass with selector ROM reads forbidden.");
    }

    /// <summary>The updates the fade's shades and hold last, which the cartridge's schedule lists one by one.</summary>
    private const int ScheduleUpdates = RidleyEyeFadeDefinitions.ShadeCount + RidleyEyeFadeDefinitions.HoldUpdates;

    /// <summary>Rejects runtime access to the migrated bank-$A6 eye-fade selector bytes.</summary>
    private static CartridgeReadGuard NewCeresRidleyEyeFadeReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address >= RidleyEyeFadeDefinitions.NativeScheduleAddress &&
                address <= RidleyEyeFadeDefinitions.NativeScheduleAddress + ScheduleUpdates,
                address => $"Ceres Ridley eye fade attempted schedule read ${address:X6}.");

    /// <summary>Runs the eye-fade schedule comparison with the retail cartridge.</summary>
    [VerificationCommand("--ceres-ridley-eye-fade-definitions")]
    internal static int CeresRidleyEyeFadeDefinitionsCommand()
    {
        Suite(nameof(VerifyCeresRidleyEyeFadeDefinitions), () => VerifyCeresRidleyEyeFadeDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
