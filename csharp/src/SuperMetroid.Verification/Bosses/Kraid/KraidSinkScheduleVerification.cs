using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Kraid's sinking Y schedule, platform PLM callbacks, and spawned rocks against the native program.</summary>
internal static class KraidSinkScheduleVerification
{

    /// <summary>Checks callback selection for every possible 16-bit body Y coordinate.</summary>
    private static void VerifyKraidSinkCallbackSelection(Dictionary<ushort, ushort> callbacks)
    {
        AssertEqual(28, callbacks.Count, "Native sinking row count");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            KraidSinkCallback? expected = callbacks.TryGetValue((ushort)raw, out ushort callback) ? KraidNativeListAddresses.NativeSinkCallbacks[callback] : null;
            AssertEqual(expected, KraidSinkSchedule.CallbackAt((ushort)raw), "Native sinking Y/callback record");
        }
    }

    /// <summary>Checks platform column immediates encoded in each nonempty sink callback.</summary>
    private static void VerifyKraidSinkPlmColumns(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks) =>
        Suite(nameof(VerifyKraidSinkPlmField), () => VerifyKraidSinkPlmField(rom, callbacks, 17, request => request.Block.X));
    /// <summary>Checks platform row immediates encoded in each nonempty sink callback.</summary>
    private static void VerifyKraidSinkPlmRows(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks) =>
        Suite(nameof(VerifyKraidSinkPlmField), () => VerifyKraidSinkPlmField(rom, callbacks, 18, request => request.Block.Y));
    /// <summary>Checks native PLM header words selected by the sink callbacks.</summary>
    private static void VerifyKraidSinkPlmHeaders(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks) =>
        Suite(nameof(VerifyKraidSinkPlmField), () => VerifyKraidSinkPlmField(rom, callbacks, 19, request => PlmNativeWords.Word(request.Header)));

    /// <summary>Compares one inline PLM field across the six mutating callbacks and excludes the empty RTS.</summary>
    private static void VerifyKraidSinkPlmField(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks,
        int offset, Func<PlmSpawnRequest, ushort> field)
    {
        int count = 0;
        foreach (ushort callback in callbacks.Distinct())
        {
            int code = 0xa70000 | callback;
            var request = KraidPlmDefinitions.ForSinkCallback(KraidNativeListAddresses.NativeSinkCallbacks[callback]);
            if (KraidSinkPlmFieldRomByte(rom, code) == 0x60)
            {
                AssertEqual<PlmSpawnRequest?>(null, request, "Native empty RTS emits no request");
                continue;
            }
            ushort expected = offset == 19
                ? SnesLittleEndian.Word(KraidSinkPlmFieldRomByte(rom, code + offset), KraidSinkPlmFieldRomByte(rom, code + offset + 1))
                : KraidSinkPlmFieldRomByte(rom, code + offset);
            AssertEqual(expected, field((request ?? throw new InvalidOperationException("Verification requires request."))), "Native sink callback inline field");
            count++;
        }
        AssertEqual(6, count, "Six distinct platform mutations");
        AssertEqual(7, KraidNativeListAddresses.NativeSinkCallbacks.Count, "Every native sink callback has a typed member");
        AssertTrue(KraidNativeListAddresses.NativeSinkCallbacks.Values.Distinct().Count() == Enum.GetValues<KraidSinkCallback>().Length,
            "The typed callbacks are exactly the native routines");
    }

    /// <summary>Reads a native callback byte for the independent PLM argument oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid sink PLM field")]
    private static byte KraidSinkPlmFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Exercises each production callback and checks its rock X, PLM request count, and event count.</summary>
    private static void VerifyKraidSinkRockPlacement(SuperMetroidAddressSpace rom,
        Dictionary<ushort, ushort> callbacks)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState();
        enemies.VerificationServices().ReadRandom = () => 0;
        Action<RoomEnemySlot, KraidEnemyState> step = enemies.KraidBehavior.Death.ProcessKraidSinkTable;
        var requests = enemies.BossFrameResults.KraidPlms;
        foreach (var entry in callbacks.DistinctBy(entry => entry.Value))
        {
            foreach (var occupied in enemies.Projectiles.EnemyProjectiles) occupied.Clear();
            requests.Clear();
            state.SinkTableEventCount = 0;
            enemies.SlotTable.Slots[0].Position = enemies.SlotTable.Slots[0].Position.WithWholeY(entry.Key);
            step(enemies.SlotTable.Slots[0], state);
            int code = 0xa70000 | entry.Value;
            bool empty = KraidSinkRockPlacementRomByte(rom, code) == 0x60;
            AssertEqual(1, state.SinkTableEventCount, "Every callback including RTS counts once");
            AssertEqual(empty ? 0 : 1, requests.Count, "Sinking callback mutation count");
            AssertEqual(empty ? 0 : 1, enemies.Projectiles.EnemyProjectiles.Count(p => p.Kind != RoomEnemyProjectileKind.None),
                "Sinking callback rock count");
            if (empty) continue;
            ushort expectedX = SnesLittleEndian.Word(KraidSinkRockPlacementRomByte(rom, code + 1), KraidSinkRockPlacementRomByte(rom, code + 2));
            AssertEqual(expectedX, enemies.Projectiles.EnemyProjectiles[^1].Position.X.Whole, "Production sinking rock matches native immediate");
        }
    }

    /// <summary>Reads a native rock-placement immediate for comparison with the spawned projectile.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid sink rock placement")]
    private static byte KraidSinkRockPlacementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks native schedule rows and optionally runs all 65,536 body Y values through production sinking behavior.</summary>
    internal static void VerifyKraidSinkSchedule(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        ushort Word(int a) => SnesLittleEndian.Word(KraidSinkScheduleRomByte(rom, a), KraidSinkScheduleRomByte(rom, a + 1));
        var callbacks = new Dictionary<ushort, ushort>();
        for (int address = 0xa7c5e7; ; address += 6)
        {
            ushort y = Word(address);
            if ((y & 0x8000) != 0) break;
            callbacks.Add(y, Word(address + 4));
        }
        Suite(nameof(VerifyKraidSinkCallbackSelection), () => VerifyKraidSinkCallbackSelection(callbacks));
        Suite(nameof(VerifyKraidSinkPlmColumns), () => VerifyKraidSinkPlmColumns(rom, callbacks.Values));
        Suite(nameof(VerifyKraidSinkPlmRows), () => VerifyKraidSinkPlmRows(rom, callbacks.Values));
        Suite(nameof(VerifyKraidSinkPlmHeaders), () => VerifyKraidSinkPlmHeaders(rom, callbacks.Values));
        Suite(nameof(VerifyKraidSinkRockPlacement), () => VerifyKraidSinkRockPlacement(rom, callbacks));
        if (definitionsOnly)
        {
            Console.WriteLine("Kraid sinking: full Y selection, native PLM fields and all seven production rock/mutation callbacks pass.");
            return;
        }
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState();
        enemies.VerificationServices().ReadRandom = () => 0;
        Action<RoomEnemySlot, KraidEnemyState> step = enemies.KraidBehavior.Death.ProcessKraidSinkTable;
        var requests = enemies.BossFrameResults.KraidPlms;
        var body = enemies.SlotTable.Slots[0];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            foreach (var occupied in enemies.Projectiles.EnemyProjectiles) occupied.Clear();
            requests.Clear();
            state.SinkTableEventCount = 0;
            body.Position = body.Position.WithWholeY((ushort)raw);
            ushort? expected = callbacks.TryGetValue((ushort)raw, out ushort callback) ? callback : null;
            step(body, state);
            AssertEqual(expected.HasValue ? 1 : 0, state.SinkTableEventCount, "Only scheduled rows count, including empty RTS");
            bool crumble = expected.HasValue && KraidNativeListAddresses.NativeSinkCallbacks[callback] != KraidSinkCallback.NoOperation;
            AssertEqual(crumble ? 1 : 0, requests.Count, "Only native crumble callbacks mutate platforms");
            AssertEqual(crumble ? 1 : 0, enemies.Projectiles.EnemyProjectiles.Count(p => p.Kind != RoomEnemyProjectileKind.None), "Exact native rock count");
            if (!crumble) continue;
            int code = 0xa70000 | callback;
            AssertEqual(new PlmSpawnRequest(new(KraidSinkScheduleRomByte(rom, code + 17), KraidSinkScheduleRomByte(rom, code + 18)), PlmNativeWords.Header(Word(code + 19))), requests[0], "Native callback inline PLM arguments");
        }
        Console.WriteLine("Kraid sink schedule: all 65536 Y coordinates, native callbacks, emitted rocks and inline PLM arguments match with schedule reads forbidden.");
    }

    /// <summary>Reads the bank-$A7 schedule and inline callback data for the cartridge oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid sink schedule")]
    private static byte KraidSinkScheduleRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

}
