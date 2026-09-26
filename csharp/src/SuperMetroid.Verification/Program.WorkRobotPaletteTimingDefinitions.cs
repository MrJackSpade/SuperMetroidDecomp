using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWorkRobotPaletteTimingDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (ushort record = 0;
             record < WorkRobotPaletteTimingDefinitions.RecordCount;
             record++)
        {
            int address = WorkRobotPaletteTimingDefinitions.NativeFirstTimerAddress +
                record * WorkRobotPaletteTimingDefinitions.RecordByteCount;
            ushort nativeDuration = ReadWord(rom, address);
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
            ReadWord(rom, WorkRobotPaletteTimingDefinitions.NativeTerminatorAddress),
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

        byte[] extracted = WorkRobotPaletteCycleExtractor.Extract(rom);
        WorkRobotPaletteCycle stock = WorkRobotPaletteCycle.Load(
            new MemoryStream(extracted, writable: false));
        for (int record = 0; record < WorkRobotPaletteTimingDefinitions.RecordCount; record++)
        for (int color = 0; color < WorkRobotPaletteRomData.ColorCount; color++)
        {
            int source = EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords +
                record * WorkRobotPaletteTimingDefinitions.RecordByteCount +
                color * sizeof(ushort);
            AssertEqual(ReadWord(rom, source), stock.Resolve(record, color),
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
        var guard = new WorkRobotPaletteTimingReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem
        {
            TileArtwork = new EnemyTileArtworkCatalog(
                new Dictionary<ushort, RoomCharacterAtlas>(),
                new Dictionary<ushort, EnemyPaletteSheet>(),
                workRobotPaletteCycle: edited),
        };
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
        type.GetField("_workRobotPaletteAnimationTimer", flags)!.SetValue(
            enemies, (ushort)1);
        type.GetField("_workRobotPaletteAnimationTableOffset", flags)!.SetValue(
            enemies, (ushort)0);
        type.GetField("_workRobotPaletteAnimationPaletteIndex", flags)!.SetValue(
            enemies, (ushort)0);
        var step = type.GetMethod("StepWorkRobotPaletteAnimation", flags)!
            .CreateDelegate<Action>(enemies);

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
                (ushort)type.GetField(
                    "_workRobotPaletteAnimationTableOffset", flags)!.GetValue(enemies)!,
                $"production Work Robot palette load {nextLoad} advances record");
            AssertEqual(
                WorkRobotPaletteTimingDefinitions.DurationForByteOffset(
                    unchecked((ushort)(record *
                        WorkRobotPaletteTimingDefinitions.RecordByteCount))),
                (ushort)type.GetField(
                    "_workRobotPaletteAnimationTimer", flags)!.GetValue(enemies)!,
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

        static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
            bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));
    }

    private sealed class WorkRobotPaletteTimingReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int relative = address -
                WorkRobotPaletteTimingDefinitions.NativeFirstTimerAddress;
            bool timerByte = relative >= 0 &&
                relative < WorkRobotPaletteTimingDefinitions.RecordCount *
                    WorkRobotPaletteTimingDefinitions.RecordByteCount &&
                relative % WorkRobotPaletteTimingDefinitions.RecordByteCount < 2;
            bool terminatorByte = address is
                WorkRobotPaletteTimingDefinitions.NativeTerminatorAddress or
                WorkRobotPaletteTimingDefinitions.NativeTerminatorAddress + 1;
            int colorRelative = address -
                EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords;
            bool colorByte = colorRelative >= 0 &&
                colorRelative < WorkRobotPaletteTimingDefinitions.RecordCount *
                    WorkRobotPaletteTimingDefinitions.RecordByteCount &&
                colorRelative % WorkRobotPaletteTimingDefinitions.RecordByteCount <
                    WorkRobotPaletteRomData.ColorCount * sizeof(ushort);
            if (timerByte || terminatorByte || colorByte)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Work Robot palette handler attempted control read ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
