using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidSinkSchedule(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        var callbacks = new Dictionary<ushort, ushort>();
        for (int address = 0xa7c5e7; ; address += 6)
        {
            ushort y = Word(address);
            if ((y & 0x8000) != 0) break;
            callbacks.Add(y, Word(address + 4));
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
            AssertEqual(expected, KraidSinkSchedule.CallbackAt((ushort)raw), "Native sinking Y/callback record");
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

    private sealed class KraidSinkReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa7c5e7 and <= 0xa7c690
            ? throw new InvalidOperationException("Unexpected migrated Kraid sink schedule read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected sink bus write.");
    }
}
