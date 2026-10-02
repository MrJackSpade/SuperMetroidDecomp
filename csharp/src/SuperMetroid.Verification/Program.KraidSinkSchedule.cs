using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidSinkCallbackSelection(Dictionary<ushort, ushort> callbacks)
    {
        AssertEqual(28, callbacks.Count, "Native sinking row count");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort? expected = callbacks.TryGetValue((ushort)raw, out ushort callback) ? callback : null;
            AssertEqual(expected, KraidSinkSchedule.CallbackAt((ushort)raw), "Native sinking Y/callback record");
        }
    }

    private static void VerifyKraidSinkPlmColumns(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks) =>
        VerifyKraidSinkPlmField(rom, callbacks, 17, request => request.BlockX);
    private static void VerifyKraidSinkPlmRows(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks) =>
        VerifyKraidSinkPlmField(rom, callbacks, 18, request => request.BlockY);
    private static void VerifyKraidSinkPlmHeaders(SuperMetroidAddressSpace rom, IEnumerable<ushort> callbacks) =>
        VerifyKraidSinkPlmField(rom, callbacks, 19, request => request.Header);

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
        VerifyKraidSinkCallbackSelection(callbacks);
        VerifyKraidSinkPlmColumns(rom, callbacks.Values);
        VerifyKraidSinkPlmRows(rom, callbacks.Values);
        VerifyKraidSinkPlmHeaders(rom, callbacks.Values);
        if (definitionsOnly)
        {
            Console.WriteLine("Kraid sinking: full Y selection domain, six native PLM records, empty RTS and rejection checks pass.");
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
            AssertEqual(Word(code + 1), enemies.EnemyProjectiles[^1].XPosition, "Rock X from native callback immediate");
            AssertEqual(new KraidPlmRequest(rom.ReadByte(code + 17), rom.ReadByte(code + 18), Word(code + 19)), requests[0], "Native callback inline PLM arguments");
        }
        Console.WriteLine("Kraid sink schedule: all 65536 Y coordinates, native callbacks, emitted rocks and inline PLM arguments match with schedule reads forbidden.");
    }

    private sealed class KraidSinkReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) => address is >= 0xa7c5e7 and <= 0xa7c690
            ? throw new InvalidOperationException("Unexpected migrated Kraid sink schedule read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected sink bus write.");
    }
}
