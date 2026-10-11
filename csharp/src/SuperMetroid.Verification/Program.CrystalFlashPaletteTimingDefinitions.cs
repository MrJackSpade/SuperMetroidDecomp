using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crystal Flash palette timing definitions")]
    private static void VerifyCrystalFlashPaletteTimingDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (ushort record = 0;
             record < CrystalFlashPaletteTimingDefinitions.RecordCount;
             record++)
        {
            int address = CrystalFlashPaletteTimingDefinitions.NativeFirstTimerAddress +
                record * CrystalFlashPaletteTimingDefinitions.RecordByteCount;
            ushort nativeDuration = unchecked((ushort)(
                rom.ReadCartridgeByte(address) | rom.ReadCartridgeByte(address + 1) << 8));
            ushort byteOffset = unchecked((ushort)(
                record * CrystalFlashPaletteTimingDefinitions.RecordByteCount));
            AssertEqual(nativeDuration,
                CrystalFlashPaletteTimingDefinitions.DurationForByteOffset(byteOffset),
                $"Crystal Flash body-palette duration {record}");
        }

        AssertThrows<InvalidDataException>(
            () => CrystalFlashPaletteTimingDefinitions.DurationForByteOffset(1),
            "Crystal Flash timing rejects an unaligned restored offset");
        AssertThrows<InvalidDataException>(
            () => CrystalFlashPaletteTimingDefinitions.DurationForByteOffset(
                CrystalFlashPaletteTimingDefinitions.RecordCount *
                    CrystalFlashPaletteTimingDefinitions.RecordByteCount),
            "Crystal Flash timing rejects an offset beyond the authored cycle");

        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            Health = 1,
            MaxHealth = 99,
            Missiles = 10,
            SuperMissiles = 10,
            PowerBombs = 10,
        };
        samus.RefreshCollisionRadii(rom);
        samus.HorizontalSpeed.SpecialPaletteTimer = 0;
        AssertTrue(
            samus.CrystalFlash.TryBegin(
                rom, samus, controllerInput: 0, skipInputCheck: true),
            "Crystal Flash timing production fixture begins");

        var guard = NewCrystalFlashPaletteTimingReadGuard(rom);
        var cgram = new SnesCgram();
        var colors = RetailPresentationFixture().CrystalFlashColors;
        int callCount = CrystalFlashPaletteTimingDefinitions.RecordCount *
            CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
        for (int call = 0; call < callCount; call++)
        {
            AssertTrue(samus.CrystalFlash.UpdatePalette(guard, cgram, samus, colors: colors),
                $"Crystal Flash timing call {call} retains palette ownership");

            ushort expectedTimer = unchecked((ushort)(
                CrystalFlashPaletteTimingDefinitions.AuthoredDuration -
                call % CrystalFlashPaletteTimingDefinitions.AuthoredDuration));
            int loadedRecord = call / CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
            ushort expectedOffset = unchecked((ushort)(
                (loadedRecord + 1) % CrystalFlashPaletteTimingDefinitions.RecordCount *
                CrystalFlashPaletteTimingDefinitions.RecordByteCount));
            AssertEqual(expectedTimer, samus.CrystalFlash.CrystalPaletteTimer,
                $"production Crystal Flash body timer call {call}");
            AssertEqual(expectedOffset, samus.CrystalFlash.CommonPaletteTimer,
                $"production Crystal Flash body record call {call}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Crystal Flash performs no body-duration ROM reads");
        Console.WriteLine(
            "Crystal Flash palette timing: all ten native durations and the complete " +
            "100-call production cycle pass with timer-word reads forbidden.");
    }

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
}
