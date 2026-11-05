using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.WorkRobots;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Checks the Work Robot's palette record timing and installed colors against bank A3.</summary>
internal static class WorkRobotPaletteTimingDefinitionsVerification
{
    /// <summary>Compares six native timing records and colors, then follows a full production cycle through its glow shades.</summary>
    internal static void VerifyWorkRobotPaletteTimingDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (ushort record = 0;
             record < WorkRobotPaletteTimingDefinitions.RecordCount;
             record++)
        {
            int address = WorkRobotPaletteTimingDefinitions.NativeFirstTimerAddress +
                record * WorkRobotPaletteTimingDefinitions.RecordByteCount;
            ushort nativeDuration = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            ushort byteOffset = unchecked((ushort)(
                record * WorkRobotPaletteTimingDefinitions.RecordByteCount));
            AssertEqual(nativeDuration,
                WorkRobotPaletteTiming.DurationForByteOffset(byteOffset),
                $"Work Robot palette duration {record}");
            AssertEqual(byteOffset,
                WorkRobotPaletteTimingDefinitionsConstants.NormalizeByteOffset(byteOffset),
                $"Work Robot palette offset {record} remains selected");
        }
        AssertEqual((ushort)0xffff,
            CartridgeImportSource.Require(rom).ReadCartridgeWord(WorkRobotPaletteTimingDefinitions.NativeTerminatorAddress),
            "Work Robot palette native terminator");
        AssertEqual((ushort)0,
            WorkRobotPaletteTimingDefinitionsConstants.NormalizeByteOffset(
                checked((ushort)(WorkRobotPaletteTimingDefinitions.RecordCount *
                    WorkRobotPaletteTimingDefinitions.RecordByteCount))),
            "Work Robot palette terminator wraps to record zero");
        AssertThrows<InvalidDataException>(
            () => WorkRobotPaletteTimingDefinitionsConstants.NormalizeByteOffset(1),
            "Work Robot palette rejects unaligned restored offsets");
        AssertThrows<InvalidDataException>(
            () => WorkRobotPaletteTimingDefinitionsConstants.NormalizeByteOffset(70),
            "Work Robot palette rejects offsets beyond the native terminator");
        foreach (ushort invalid in new ushort[] { 1, 9, 51, 59, 60, 70, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(
                () => WorkRobotPaletteTiming.DurationForByteOffset(invalid),
                $"Work Robot duration rejects non-record offset {invalid}");
        }

        byte[] extracted = WorkRobotPaletteCycleExtractor.Extract(rom);
        WorkRobotPaletteCycle stock = WorkRobotPaletteCycleFile.Read(
            new MemoryStream(extracted, writable: false));
        for (int record = 0; record < WorkRobotPaletteTimingDefinitions.RecordCount; record++)
        for (int color = 0; color < WorkRobotPaletteDefinitions.ColorCount; color++)
        {
            int source = EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords +
                record * WorkRobotPaletteTimingDefinitions.RecordByteCount +
                color * sizeof(ushort);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(source), stock.Resolve(record, color),
                $"Work Robot color record {record} slot {color} preserves ROM RGB5");
        }
        var guard = NewWorkRobotPaletteTimingReadGuard(rom);
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), WorkRobotPaletteCycle = stock }),
        };
        Type type = typeof(RoomEnemySystem);
        enemies.WorkRobotBehavior.WorkRobotPaletteAnimationTimer = WrappingCountdown.Start(1);
        enemies.WorkRobotBehavior.WorkRobotPaletteAnimationRecord = 0;
        var step = new Action(enemies.WorkRobotBehavior.StepWorkRobotPaletteAnimation);

        int[] loadCalls = [0, 64, 80, 96, 160, 176, 192];
        int nextLoad = 0;
        for (int call = 0; call <= loadCalls[^1]; call++)
        {
            step();
            if (call != loadCalls[nextLoad])
                continue;

            int record = nextLoad == WorkRobotPaletteTimingDefinitions.RecordCount
                ? 0
                : nextLoad;
            AssertEqual(record + 1,
                enemies.WorkRobotBehavior.WorkRobotPaletteAnimationRecord,
                $"production Work Robot palette load {nextLoad} advances record");
            AssertEqual(
                WorkRobotPaletteTiming.DurationForByteOffset(
                    unchecked((ushort)(record *
                        WorkRobotPaletteTimingDefinitions.RecordByteCount))),
                enemies.WorkRobotBehavior.WorkRobotPaletteAnimationTimer.RawValue,
                $"production Work Robot palette load {nextLoad} timer");
            // The robots draw in the record's glow shade, which the import pre-colours from the cycle's colours for the
            // record (#1512); those colours are compared with bank $A3 above.
            AssertEqual(PictureShading.Shaded(WorkRobotShades.Glow(record)), enemies.WorkRobotBehavior.WorkRobotGlow,
                $"production Work Robot palette load {nextLoad} shows the record's glow shade");
            nextLoad++;
        }

        AssertEqual(loadCalls.Length, nextLoad,
            "production Work Robot palette reaches every record and wraps");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Work Robot palette performs no color, timing or terminator ROM reads");
        Console.WriteLine(
            "Work Robot palette: all six native durations and 24 colors, the terminator and " +
            "the complete 193-call production cycle pass with ROM reads forbidden.");
    }

    /// <summary>Rejects live reads of Work Robot palette timer, terminator, and color bytes.</summary>
    private static CartridgeReadGuard NewWorkRobotPaletteTimingReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int relative = address -
                    WorkRobotPaletteTimingDefinitions.NativeFirstTimerAddress;
                bool timerByte = relative >= 0 &&
                    relative < WorkRobotPaletteTimingDefinitions.RecordCount *
                        WorkRobotPaletteTimingDefinitions.RecordByteCount &&
                    relative % WorkRobotPaletteTimingDefinitions.RecordByteCount < 2;
                bool terminatorByte = address is
                    WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress or
                    WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress + 1;
                int colorRelative = address -
                    EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords;
                bool colorByte = colorRelative >= 0 &&
                    colorRelative < WorkRobotPaletteTimingDefinitions.RecordCount *
                        WorkRobotPaletteTimingDefinitions.RecordByteCount &&
                    colorRelative % WorkRobotPaletteTimingDefinitions.RecordByteCount <
                        WorkRobotPaletteDefinitions.ColorCount * sizeof(ushort);
                return timerByte || terminatorByte || colorByte;
            },
            address => $"Work Robot palette handler attempted control read ${address:X6}.");

    /// <summary>Runs the Work Robot palette timing check with the retail cartridge as its oracle.</summary>
    [VerificationCommand("--work-robot-palette-timing-definitions")]
    internal static int WorkRobotPaletteTimingDefinitionsCommand()
    {
        Suite(nameof(VerifyWorkRobotPaletteTimingDefinitions), () => VerifyWorkRobotPaletteTimingDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
