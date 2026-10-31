using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Palettes;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Crystal Flash body-palette durations and the full production color cycle.</summary>
internal static class CrystalFlashPaletteTimingDefinitionsVerification
{
    /// <summary>Compares ten native timer words and checks 100 palette updates without runtime timer reads.</summary>
    internal static void VerifyCrystalFlashPaletteTimingDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (ushort record = 0;
             record < CrystalFlashPaletteTimingDefinitions.RecordCount;
             record++)
        {
            int address = CrystalFlashPaletteTimingDefinitions.NativeFirstTimerAddress +
                record * CrystalFlashPaletteTimingDefinitions.RecordByteCount;
            ushort nativeDuration = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            ushort byteOffset = unchecked((ushort)(
                record * CrystalFlashPaletteTimingDefinitions.RecordByteCount));
            AssertEqual(nativeDuration,
                CrystalFlashPaletteTiming.DurationForByteOffset(byteOffset),
                $"Crystal Flash body-palette duration {record}");
        }

        AssertThrows<InvalidDataException>(
            () => CrystalFlashPaletteTiming.DurationForByteOffset(1),
            "Crystal Flash timing rejects an unaligned restored offset");
        AssertThrows<InvalidDataException>(
            () => CrystalFlashPaletteTiming.DurationForByteOffset(
                CrystalFlashPaletteTimingDefinitions.RecordCount *
                    CrystalFlashPaletteTimingDefinitions.RecordByteCount),
            "Crystal Flash timing rejects an offset beyond the authored cycle");

        var samus = new SamusState(InstalledFixtures.AnimationContinuation)
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            Health = 1,
            MaxHealth = 99,
            Missiles = 10,
            SuperMissiles = 10,
            PowerBombs = 10,
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.HorizontalSpeed.SpecialPaletteTimer = WrappingCountdown.Start(0);
        AssertTrue(
            samus.CrystalFlash.TryBegin(
                samus, controllerInput: 0, inputCheck: CrystalFlashInputCheck.Skipped),
            "Crystal Flash timing production fixture begins");

        var guard = NewCrystalFlashPaletteTimingReadGuard(rom);
        var cgram = new SamusColourRig();
        var colors = RetailPresentationFixture().CrystalFlashColors;
        int callCount = CrystalFlashPaletteTimingDefinitions.RecordCount *
            CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
        for (int call = 0; call < callCount; call++)
        {
            AssertTrue(samus.CrystalFlash.UpdatePalette(cgram.Samus, samus, colors: colors),
                $"Crystal Flash timing call {call} retains palette ownership");

            ushort expectedTimer = unchecked((ushort)(
                CrystalFlashPaletteTimingDefinitions.AuthoredDuration -
                call % CrystalFlashPaletteTimingDefinitions.AuthoredDuration));
            int loadedRecord = call / CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
            ushort expectedOffset = unchecked((ushort)(
                (loadedRecord + 1) % CrystalFlashPaletteTimingDefinitions.RecordCount *
                CrystalFlashPaletteTimingDefinitions.RecordByteCount));
            AssertEqual(expectedTimer, samus.CrystalFlash.CrystalPaletteTimer.RawValue,
                $"production Crystal Flash body timer call {call}");
            AssertEqual(expectedOffset, samus.CrystalFlash.BodyPaletteRecordOffset,
                $"production Crystal Flash body record call {call}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Crystal Flash performs no body-duration ROM reads");
        Console.WriteLine(
            "Crystal Flash palette timing: all ten native durations and the complete " +
            "100-call production cycle pass with timer-word reads forbidden.");
    }

    /// <summary>Blocks the timer word of each migrated body-palette record during updates.</summary>
    private static CartridgeReadGuard NewCrystalFlashPaletteTimingReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int relative = address -
                    CrystalFlashPaletteTimingDefinitions.NativeFirstTimerAddress;
                return relative >= 0 &&
                    relative < CrystalFlashPaletteTimingDefinitions.RecordCount *
                        CrystalFlashPaletteTimingDefinitions.RecordByteCount &&
                    relative % CrystalFlashPaletteTimingDefinitions.RecordByteCount < 2;
            },
            address => $"Crystal Flash palette handler attempted timer read ${address:X6}.");

    /// <summary>Runs the Crystal Flash palette timing verification command.</summary>
    [VerificationCommand("--crystal-flash-palette-timing-definitions")]
    internal static int CrystalFlashPaletteTimingDefinitionsCommand()
    {
        Suite(nameof(VerifyCrystalFlashPaletteTimingDefinitions), () => VerifyCrystalFlashPaletteTimingDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
