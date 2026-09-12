using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidMouthHitboxes(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        for (int cursor = 0x96d2; cursor < 0x9788;)
        {
            ushort command = Word(0xa70000 | cursor);
            if ((command & 0x8000) != 0) { cursor += 2; continue; }
            foreach (int offset in new[] { 4, 6 })
            {
                ushort pointer = Word(0xa70000 | (cursor + offset));
                if (pointer != ushort.MaxValue) _ = KraidMouthHitboxes.Resolve(pointer);
            }
            cursor += 8;
        }
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        var overlaps = typeof(RoomEnemySystem).GetMethod("KraidMouthHitboxOverlapsShot", flags)!
            .CreateDelegate<Func<RoomEnemySlot, ushort, SamusProjectileSlot, bool>>(enemies);
        var body = enemies.Slots[0];
        var shot = new SamusProjectileSystem().Slots[0];
        body.XPosition = 256;
        body.YPosition = 256;
        shot.XRadius = 4;
        shot.YRadius = 4;
        for (int entry = 0; entry < 8; entry++)
        {
            ushort pointer = (ushort)(0x9788 + entry * 8);
            int address = 0xa70000 | pointer;
            short left = (short)Word(address), top = (short)Word(address + 2), right = (short)Word(address + 4), bottom = (short)Word(address + 6);
            AssertEqual((left, top, right, bottom), KraidMouthHitboxes.Resolve(pointer), "All native mouth geometry words");
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            for (int edge = -1; edge <= 1; edge++)
            {
                shot.YPosition = (ushort)raw;
                shot.XPosition = (ushort)(body.XPosition + left - shot.XRadius + edge);
                bool expected = raw - shot.YRadius - 1 < body.YPosition + bottom &&
                    raw + shot.YRadius >= body.YPosition + top && edge >= 0;
                AssertEqual(expected, overlaps(body, pointer, shot), "Actual mouth collision preserves vertical bounds and inclusive left edge");
            }
        }
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, rom);
        shot.XPosition = 260;
        shot.YPosition = 256;
        foreach (ushort pointer in new ushort[] { 0, 2, 0x1ff8, 0x9789 })
        {
            int address = 0xa70000 | pointer;
            short left = (short)Word(address), top = (short)Word(address + 2), bottom = (short)Word(address + 6);
            bool expected = shot.YPosition - shot.YRadius - 1 < body.YPosition + bottom &&
                shot.YPosition + shot.YRadius >= body.YPosition + top && shot.XPosition + shot.XRadius >= body.XPosition + left;
            AssertEqual(expected, overlaps(body, pointer, shot), "Non-catalog pointers preserve address-space reads");
        }
        Console.WriteLine("Kraid mouth geometry: all head-program pointers, 32 native words and 1572864 authored collision probes reject bus access; non-catalog reads preserved.");
    }
}
