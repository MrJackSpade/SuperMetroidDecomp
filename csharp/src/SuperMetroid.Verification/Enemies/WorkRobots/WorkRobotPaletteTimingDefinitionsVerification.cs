using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Enemies.WorkRobots;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Work Robot's palette record timing and installed colors against bank A3.</summary>
internal static class WorkRobotPaletteTimingDefinitionsVerification
{
    /// <summary>Compares six native timing records and colors, then follows a full edited-color cycle through CGRAM.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Work Robot palette timing definitions")]
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
                WorkRobotPaletteTimingDefinitions.DurationForByteOffset(byteOffset),
                $"Work Robot palette duration {record}");
            AssertEqual(byteOffset,
                WorkRobotPaletteTimingDefinitions.NormalizeByteOffset(byteOffset),
                $"Work Robot palette offset {record} remains selected");
        }
        AssertEqual((ushort)0xffff,
            CartridgeImportSource.Require(rom).ReadCartridgeWord(WorkRobotPaletteTimingDefinitions.NativeTerminatorAddress),
            "Work Robot palette native terminator");
        AssertEqual((ushort)0,
            WorkRobotPaletteTimingDefinitions.NormalizeByteOffset(
                WorkRobotPaletteTimingDefinitions.RecordCount *
                    WorkRobotPaletteTimingDefinitions.RecordByteCount),
            "Work Robot palette terminator wraps to record zero");
        AssertThrows<InvalidDataException>(
            () => WorkRobotPaletteTimingDefinitions.NormalizeByteOffset(1),
            "Work Robot palette rejects unaligned restored offsets");
        AssertThrows<InvalidDataException>(
            () => WorkRobotPaletteTimingDefinitions.NormalizeByteOffset(70),
            "Work Robot palette rejects offsets beyond the native terminator");
        foreach (ushort invalid in new ushort[] { 1, 9, 51, 59, 60, 70, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(
                () => WorkRobotPaletteTimingDefinitions.DurationForByteOffset(invalid),
                $"Work Robot duration rejects non-record offset {invalid}");
        }

        byte[] extracted = WorkRobotPaletteCycleExtractor.Extract(rom);
        WorkRobotPaletteCycle stock = WorkRobotPaletteCycle.Load(
            new MemoryStream(extracted, writable: false));
        for (int record = 0; record < WorkRobotPaletteTimingDefinitions.RecordCount; record++)
        for (int color = 0; color < WorkRobotPaletteRomData.ColorCount; color++)
        {
            int source = EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords +
                record * WorkRobotPaletteTimingDefinitions.RecordByteCount +
                color * sizeof(ushort);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(source), stock.Resolve(record, color),
                $"Work Robot color record {record} slot {color} preserves ROM RGB5");
        }
        WorkRobotPaletteCycleDocument visual =
            JsonSerializer.Deserialize<WorkRobotPaletteCycleDocument>(extracted,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Work Robot color extraction is null.");
        PaletteRgb5 prior = visual.Frames[1][0];
        visual.Frames[1][0] = prior with
        {
            Blue = prior.Blue == 31 ? 30 : prior.Blue + 1,
        };
        WorkRobotPaletteCycle edited = WorkRobotPaletteCycle.Load(
            new MemoryStream(WorkRobotPaletteCycle.Write(visual), writable: false));

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewWorkRobotPaletteTimingReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), WorkRobotPaletteCycle = edited }),
        };
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        enemies.Presentation._cgram = cgram;
        enemies.WorkRobotBehavior._workRobotPaletteAnimationTimer = NativeCountdown.Start((ushort)1);
        enemies.WorkRobotBehavior._workRobotPaletteAnimationTableOffset = (ushort)0;
        enemies.WorkRobotBehavior._workRobotPaletteAnimationPaletteIndex = (ushort)0;
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
            ushort expectedOffset = unchecked((ushort)(
                (record + 1) * WorkRobotPaletteTimingDefinitions.RecordByteCount));
            AssertEqual(expectedOffset,
                (ushort)enemies.WorkRobotBehavior._workRobotPaletteAnimationTableOffset,
                $"production Work Robot palette load {nextLoad} advances record");
            AssertEqual(
                WorkRobotPaletteTimingDefinitions.DurationForByteOffset(
                    unchecked((ushort)(record *
                        WorkRobotPaletteTimingDefinitions.RecordByteCount))),
                (ushort)enemies.WorkRobotBehavior._workRobotPaletteAnimationTimer.RawValue,
                $"production Work Robot palette load {nextLoad} timer");
            for (int color = 0; color < WorkRobotPaletteRomData.ColorCount; color++)
            {
                AssertEqual(edited.Resolve(record, color), cgram.Colors[137 + color],
                    $"production Work Robot palette load {nextLoad} color {color}");
            }
            nextLoad++;
        }

        AssertEqual(loadCalls.Length, nextLoad,
            "production Work Robot palette reaches every record and wraps");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Work Robot palette performs no color, timing or terminator ROM reads");
        Console.WriteLine(
            "Work Robot palette: all six native durations and 24 colors, the terminator and " +
            "the complete 193-call edited-color production cycle pass with ROM reads forbidden.");
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
                        WorkRobotPaletteRomData.ColorCount * sizeof(ushort);
                return timerByte || terminatorByte || colorByte;
            },
            address => $"Work Robot palette handler attempted control read ${address:X6}.");

    /// <summary>Runs the Work Robot palette timing check with the retail cartridge as its oracle.</summary>
    [VerificationCommand("--work-robot-palette-timing-definitions")]
    internal static int WorkRobotPaletteTimingDefinitionsCommand()
    {
        Suite(nameof(VerifyWorkRobotPaletteTimingDefinitions), () => VerifyWorkRobotPaletteTimingDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
