using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static RetailPresentationFixtureVerification;

/// <summary>Represents ceres ridley eye fade definitions verification.</summary>
internal static class CeresRidleyEyeFadeDefinitionsVerification
{
    /// <summary>Verifies ceres ridley eye fade definitions.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres Ridley eye fade definitions")]
    internal static void VerifyCeresRidleyEyeFadeDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0;
             offset < CeresRidleyEyeFadeDefinitions.PaletteStepCount;
             offset++)
        {
            CeresRidleyEyeFadeStep compiled = CeresRidleyEyeFadeDefinitions.Get(offset);
            AssertTrue(!compiled.IsComplete,
                $"Ceres Ridley eye-fade offset {offset} remains active");
            AssertEqual(
                rom.ReadCartridgeByte(CeresRidleyEyeFadeDefinitions.NativeScheduleAddress + offset),
                compiled.PaletteRow,
                $"Ceres Ridley eye-fade row {offset}");
        }

        AssertEqual((byte)0xff,
            rom.ReadCartridgeByte(CeresRidleyEyeFadeDefinitions.NativeScheduleAddress +
                CeresRidleyEyeFadeDefinitions.PaletteStepCount),
            "Ceres Ridley eye-fade native terminator");
        AssertTrue(
            CeresRidleyEyeFadeDefinitions.Get(
                CeresRidleyEyeFadeDefinitions.PaletteStepCount).IsComplete,
            "Ceres Ridley eye-fade compiled terminator");
        AssertThrows<InvalidDataException>(
            () => CeresRidleyEyeFadeDefinitions.Get(
                CeresRidleyEyeFadeDefinitions.PaletteStepCount + 1),
            "Ceres Ridley eye fade rejects restored offsets beyond the authored schedule");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewCeresRidleyEyeFadeReadGuard(rom);
        var enemies = new RoomEnemySystem { Presentation = { CeresRidleyColors = RetailPresentationFixture().CeresRidleyColors } };
        enemies.BindFixtureBus(guard);
        var cgram = new SnesCgram();
        enemies.Presentation._cgram = cgram;
        Action<RidleyEnemyState> tick = enemies.CeresRidleyBehavior.TickCeresRidleyEyeFade;

        var state = new RidleyEnemyState
        {
            Function = RidleyAiFunction.FadeInEyes,
            MovementAnimationEnabled = 0,
        };
        for (ushort offset = 0;
             offset < CeresRidleyEyeFadeDefinitions.PaletteStepCount;
             offset++)
        {
            state.FadePaletteOffset = offset;
            state.FunctionTimer = 0;
            tick(state);

            CeresRidleyEyeFadeStep expected = CeresRidleyEyeFadeDefinitions.Get(offset);
            AssertEqual(unchecked((ushort)(offset + 1)), state.FadePaletteOffset,
                $"production Ceres eye-fade offset {offset} advances");
            AssertEqual(RidleyAiFunction.FadeInEyes, state.Function,
                $"production Ceres eye-fade offset {offset} remains in fade phase");
            for (int color = 0; color < 3; color++)
            {
                int source = EnemyRomTablePointers.Ceres.RidleyEyeFadePaletteRows +
                    expected.PaletteRow * 6 + color * 2;
                ushort expectedColor = CartridgeImportSource.Require(rom).ReadCartridgeWord(source);
                AssertEqual(expectedColor, cgram.Colors[252 + color],
                    $"production Ceres eye-fade offset {offset} color {color}");
            }
        }

        state.FadePaletteOffset = CeresRidleyEyeFadeDefinitions.PaletteStepCount;
        state.FunctionTimer = 0;
        tick(state);
        AssertEqual((ushort)0, state.FadePaletteOffset,
            "production Ceres eye-fade terminator resets its offset");
        AssertEqual(RidleyAiFunction.FadeInBody, state.Function,
            "production Ceres eye-fade terminator enters body fade");
        AssertEqual((ushort)1, state.MovementAnimationEnabled,
            "production Ceres eye-fade terminator enables movement animation");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Ceres eye fade performs no schedule reads");

        Console.WriteLine(
            "Ceres Ridley eye fade: all 64 row selections, the terminal handoff and " +
            "the complete production schedule pass with selector ROM reads forbidden.");
    }

    /// <summary>New ceres ridley eye fade read guard.</summary>
    private static CartridgeReadGuard NewCeresRidleyEyeFadeReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address >= CeresRidleyEyeFadeDefinitions.NativeScheduleAddress &&
                address <= CeresRidleyEyeFadeDefinitions.NativeScheduleAddress +
                    CeresRidleyEyeFadeDefinitions.PaletteStepCount,
                address => $"Ceres Ridley eye fade attempted schedule read ${address:X6}.");

    /// <summary>Ceres ridley eye fade definitions command.</summary>
    [VerificationCommand("--ceres-ridley-eye-fade-definitions")]
    internal static int CeresRidleyEyeFadeDefinitionsCommand()
    {
        Suite(nameof(VerifyCeresRidleyEyeFadeDefinitions), () => VerifyCeresRidleyEyeFadeDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
