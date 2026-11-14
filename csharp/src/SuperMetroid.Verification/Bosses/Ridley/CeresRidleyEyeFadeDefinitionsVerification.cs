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
    /// <summary>Compares all 64 rows and the terminator while forbidding live schedule reads.</summary>
    internal static void VerifyCeresRidleyEyeFadeDefinitions(CartridgeImportAddressSpace rom)
    {
        for (ushort offset = 0;
             offset < CeresRidleyEyeFadeDefinitions.PaletteStepCount;
             offset++)
        {
            CeresRidleyEyeFadeStep compiled = CeresRidleyEyeFade.Get(offset);
            AssertTrue(!compiled.IsComplete,
                $"Ceres Ridley eye-fade offset {offset} remains active");
            AssertEqual(
                rom.ReadCartridgeByte(CeresRidleyEyeFadeDefinitions.NativeScheduleAddress + offset),
                compiled.Shade,
                $"Ceres Ridley eye-fade row {offset}");
        }

        AssertEqual((byte)0xff,
            rom.ReadCartridgeByte(CeresRidleyEyeFadeDefinitions.NativeScheduleAddress +
                CeresRidleyEyeFadeDefinitions.PaletteStepCount),
            "Ceres Ridley eye-fade native terminator");
        AssertTrue(
            CeresRidleyEyeFade.Get(
                CeresRidleyEyeFadeDefinitions.PaletteStepCount).IsComplete,
            "Ceres Ridley eye-fade compiled terminator");
        AssertThrows<InvalidDataException>(
            () => CeresRidleyEyeFade.Get(
                CeresRidleyEyeFadeDefinitions.PaletteStepCount + 1),
            "Ceres Ridley eye fade rejects restored offsets beyond the authored schedule");

        var guard = NewCeresRidleyEyeFadeReadGuard(rom);
        CeresRidleyColorCatalog colours = RetailImportPresentationFixture().CeresRidleyColors;
        IReadOnlyList<SuperMetroid.AssetExtraction.Artwork.SheetShade> shades = SuperMetroid.AssetExtraction.Bosses.RidleySheetShades.CeresSprites(colours);
        Action<RidleyEnemyState> tick = RidleyFadeIn.TickEyeFadeIn;

        var state = new RidleyEnemyState
        {
            Phase = RidleyPhase.FadeInEyes,
            MovementAnimationEnabled = false,
        };
        for (ushort offset = 0;
             offset < CeresRidleyEyeFadeDefinitions.PaletteStepCount;
             offset++)
        {
            state.FadeStep = offset;
            state.PhaseTimer = 0;
            tick(state);

            CeresRidleyEyeFadeStep expected = CeresRidleyEyeFade.Get(offset);
            // The colours the encounter shows are the import's shades of its installed colours, laid as the fade laid them (#1512).
            ushort[] cgram = RidleyLaidColours.Of(state, shades, [], 0);
            AssertEqual(unchecked((ushort)(offset + 1)), state.FadeStep,
                $"production Ceres eye-fade offset {offset} advances");
            AssertEqual(RidleyPhase.FadeInEyes, state.Phase,
                $"production Ceres eye-fade offset {offset} remains in fade phase");
            for (int color = 0; color < 3; color++)
            {
                int source = EnemyRomTablePointers.Ceres.RidleyEyeFadePaletteRows +
                    expected.Shade * 6 + color * 2;
                ushort expectedColor = CartridgeImportSource.Require(rom).ReadCartridgeWord(source);
                AssertEqual(expectedColor, cgram[252 + color],
                    $"production Ceres eye-fade offset {offset} color {color}");
            }
        }

        state.FadeStep = CeresRidleyEyeFadeDefinitions.PaletteStepCount;
        state.PhaseTimer = 0;
        tick(state);
        AssertEqual(0, state.FadeStep,
            "production Ceres eye-fade terminator resets its offset");
        AssertEqual(RidleyPhase.FadeInBody, state.Phase,
            "production Ceres eye-fade terminator enters body fade");
        AssertTrue(state.MovementAnimationEnabled,
            "production Ceres eye-fade terminator enables movement animation");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Ceres eye fade performs no schedule reads");

        Console.WriteLine(
            "Ceres Ridley eye fade: all 64 row selections, the terminal handoff and " +
            "the complete production schedule pass with selector ROM reads forbidden.");
    }

    /// <summary>Rejects runtime access to the migrated bank-$A6 eye-fade selector bytes.</summary>
    private static CartridgeReadGuard NewCeresRidleyEyeFadeReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address >= CeresRidleyEyeFadeDefinitions.NativeScheduleAddress &&
                address <= CeresRidleyEyeFadeDefinitions.NativeScheduleAddress +
                    CeresRidleyEyeFadeDefinitions.PaletteStepCount,
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
