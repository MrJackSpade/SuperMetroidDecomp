using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares every possible Kraid Y position with the native sink-table callback selection.</summary>
    /// <param name="callbacks">The Y-coordinate to callback entries decoded from the retail schedule.</param>
    private static void VerifyKraidSinkCallbackSelection(Dictionary<ushort, ushort> callbacks)
    {
        AssertEqual(28, callbacks.Count, "Native sinking row count");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort? expected = callbacks.TryGetValue((ushort)raw, out ushort callback) ? callback : null;
            AssertEqual(expected, KraidSinkSchedule.CallbackAt((ushort)raw), "Native sinking Y/callback record");
        }
    }

    /// <summary>Checks that each nonempty native sink callback supplies the expected PLM block column.</summary>
    /// <param name="rom">The retail address space containing callback code and inline arguments.</param>
    /// <param name="callbacks">The distinct callback addresses selected by the native sink schedule.</param>
    private static void VerifyKraidSinkPlmColumns(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks) =>
        Suite(nameof(VerifyKraidSinkPlmField), () => VerifyKraidSinkPlmField(rom, callbacks, 17, request => request.BlockX));

    /// <summary>Checks that each nonempty native sink callback supplies the expected PLM block row.</summary>
    /// <param name="rom">The retail address space containing callback code and inline arguments.</param>
    /// <param name="callbacks">The distinct callback addresses selected by the native sink schedule.</param>
    private static void VerifyKraidSinkPlmRows(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks) =>
        Suite(nameof(VerifyKraidSinkPlmField), () => VerifyKraidSinkPlmField(rom, callbacks, 18, request => request.BlockY));

    /// <summary>Checks that each nonempty native sink callback supplies the expected PLM header.</summary>
    /// <param name="rom">The retail address space containing callback code and inline arguments.</param>
    /// <param name="callbacks">The distinct callback addresses selected by the native sink schedule.</param>
    private static void VerifyKraidSinkPlmHeaders(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks) =>
        Suite(nameof(VerifyKraidSinkPlmField), () => VerifyKraidSinkPlmField(rom, callbacks, 19, request => request.Header));

    /// <summary>Compares a selected inline PLM argument at each native callback with the compiled request field.</summary>
    /// <param name="rom">The retail address space containing native callback instructions and arguments.</param>
    /// <param name="callbacks">Callback addresses whose inline arguments are inspected.</param>
    /// <param name="offset">Byte offset from the callback entry for the selected field.</param>
    /// <param name="field">Selects the corresponding value from a decoded PLM request.</param>
    private static void VerifyKraidSinkPlmField(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks,
        int offset, Func<KraidPlmRequest, ushort> field)
    {
        int count = 0;
        foreach (ushort callback in callbacks.Distinct())
        {
            int code = 0xa70000 | callback;
            var request = KraidPlmDefinitions.ForSinkCallback(callback);
            if (rom.ReadByte(code) == 0x60)
            {
                AssertEqual<KraidPlmRequest?>(null, request, "Native empty RTS emits no request");
                continue;
            }
            ushort expected = offset == 19
                ? (ushort)(rom.ReadByte(code + offset) | rom.ReadByte(code + offset + 1) << 8)
                : rom.ReadByte(code + offset);
            AssertEqual(expected, field(request!.Value), "Native sink callback inline field");
            count++;
        }
        AssertEqual(6, count, "Six distinct platform mutations");
        foreach (ushort invalid in new ushort[] { 0, 0xc690, 0xc692, 0xc6a5, 0xc715, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => KraidPlmDefinitions.ForSinkCallback(invalid), "Unknown sink callback rejected");
    }

    /// <summary>Runs production sink callbacks and checks event counts, emitted rocks, and each rock's native X position.</summary>
    /// <param name="rom">The retail address space providing callback instructions and rock coordinates.</param>
    /// <param name="callbacks">The decoded Y-coordinate schedule used to exercise each distinct callback.</param>
    private static void VerifyKraidSinkRockPlacement(SuperMetroidAddressSpace rom,
        Dictionary<ushort, ushort> callbacks)
    {
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new KraidSinkReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => 0));
        var step = typeof(RoomEnemySystem).GetMethod("ProcessKraidSinkTable", flags)!
            .CreateDelegate<Action<RoomEnemySlot, KraidEnemyState>>(enemies);
        var requests = (List<KraidPlmRequest>)typeof(RoomEnemySystem).GetField("_kraidPlmRequests", flags)!.GetValue(enemies)!;
        foreach (var entry in callbacks.DistinctBy(entry => entry.Value))
        {
            foreach (var occupied in enemies.EnemyProjectiles) occupied.Clear();
            requests.Clear();
            state.SinkTableEventCount = 0;
            enemies.Slots[0].YPosition = entry.Key;
            step(enemies.Slots[0], state);
            int code = 0xa70000 | entry.Value;
            bool empty = rom.ReadByte(code) == 0x60;
            AssertEqual(1, state.SinkTableEventCount, "Every callback including RTS counts once");
            AssertEqual(empty ? 0 : 1, requests.Count, "Sinking callback mutation count");
            AssertEqual(empty ? 0 : 1, enemies.EnemyProjectiles.Count(p => p.Kind != RoomEnemyProjectileKind.None),
                "Sinking callback rock count");
            if (empty) continue;
            ushort expectedX = (ushort)(rom.ReadByte(code + 1) | rom.ReadByte(code + 2) << 8);
            AssertEqual(expectedX, enemies.EnemyProjectiles[^1].XPosition, "Production sinking rock matches native immediate");
        }
    }

    /// <summary>Decodes the retail Y-to-callback table and verifies its PLM arguments and production effects.</summary>
    /// <param name="rom">The retail address space containing the schedule and callback code.</param>
    /// <param name="definitionsOnly">When true, stops after definition and callback checks instead of sweeping all Y positions.</param>
    private static void VerifyKraidSinkSchedule(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
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
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new KraidSinkReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => 0));
        var step = typeof(RoomEnemySystem).GetMethod("ProcessKraidSinkTable", flags)!
            .CreateDelegate<Action<RoomEnemySlot, KraidEnemyState>>(enemies);
        var requests = (List<KraidPlmRequest>)typeof(RoomEnemySystem).GetField("_kraidPlmRequests", flags)!.GetValue(enemies)!;
        var body = enemies.Slots[0];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            foreach (var occupied in enemies.EnemyProjectiles) occupied.Clear();
            requests.Clear();
            state.SinkTableEventCount = 0;
            body.YPosition = (ushort)raw;
            ushort? expected = callbacks.TryGetValue((ushort)raw, out ushort callback) ? callback : null;
            step(body, state);
            AssertEqual(expected.HasValue ? 1 : 0, state.SinkTableEventCount, "Only scheduled rows count, including empty RTS");
            bool crumble = expected.HasValue && callback != KraidSinkCallbacks.NoOperation;
            AssertEqual(crumble ? 1 : 0, requests.Count, "Only native crumble callbacks mutate platforms");
            AssertEqual(crumble ? 1 : 0, enemies.EnemyProjectiles.Count(p => p.Kind != RoomEnemyProjectileKind.None), "Exact native rock count");
            if (!crumble) continue;
            int code = 0xa70000 | callback;
            AssertEqual(new KraidPlmRequest(rom.ReadByte(code + 17), rom.ReadByte(code + 18), Word(code + 19)), requests[0], "Native callback inline PLM arguments");
        }
        Console.WriteLine("Kraid sink schedule: all 65536 Y coordinates, native callbacks, emitted rocks and inline PLM arguments match with schedule reads forbidden.");
    }

    /// <summary>Rejects reads from the migrated Kraid sink schedule and rejects writes during the production probe.</summary>
    /// <param name="source">The underlying address space used for reads outside the migrated schedule.</param>
    private sealed class KraidSinkReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge reads through the migrated-schedule guard.</summary>
        /// <param name="address">The bus address to read.</param>
        /// <returns>The byte returned by the guarded address space when the read is allowed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the native sink schedule range and delegates other byte reads.</summary>
        /// <param name="address">The bus address to inspect and read.</param>
        /// <returns>The byte supplied by the underlying address space when the address is allowed.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to the migrated Kraid sink schedule.</exception>
        public byte ReadByte(int address) => address is >= 0xa7c5e7 and <= 0xa7c690
            ? throw new InvalidOperationException("Unexpected migrated Kraid sink schedule read.") : source.ReadByte(address);

        /// <summary>Rejects writes because the schedule verification probe only reads cartridge data.</summary>
        /// <param name="address">The bus address the probe attempted to write.</param>
        /// <param name="value">The byte the probe attempted to store.</param>
        /// <exception cref="InvalidOperationException">The read-only verification bus was asked to write.</exception>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected sink bus write.");
    }
}
