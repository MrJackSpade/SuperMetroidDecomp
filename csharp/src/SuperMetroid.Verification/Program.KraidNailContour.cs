using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidNailContour(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        var native = new ushort[64];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = Word(EnemyRomTablePointers.Kraid.NailPositionOffsetWords + i * 2);
            AssertEqual(native[i], KraidNailContour.Words[i], "Native contour and adjacent word window");
        }
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        var reflect = typeof(RoomEnemySystem).GetMethod("KraidBodyContourReflectsNail", flags)!
            .CreateDelegate<Func<RoomEnemySlot, bool>>(enemies);
        var body = enemies.Slots[0];
        var nail = enemies.Slots[6];
        nail.XRadius = 4;
        int overreadCases = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            body.XPosition = (ushort)raw;
            body.YPosition = 0x1234;
            nail.YPosition = unchecked((ushort)(body.YPosition + raw));
            int record = 0;
            while (record < 32 && unchecked((short)(native[record * 2 + 1] + body.YPosition - nail.YPosition)) >= 0) record++;
            if (record >= 4 && record < 32) overreadCases++;
            ushort edge = record < 32 ? unchecked((ushort)(body.XPosition + native[record * 2])) : (ushort)0;
            foreach (ushort velocity in new ushort[] { 0, 1, 0x7fff, 0x8000, 0xffff })
            for (int delta = -1; delta <= 1; delta++)
            {
                nail.XPosition = unchecked((ushort)(edge - nail.XRadius + delta));
                nail.VariableC = velocity;
                bool expected = record < 32 && unchecked((short)(nail.XPosition + nail.XRadius - edge)) >= 0 && (short)velocity >= 0;
                AssertEqual(expected, reflect(nail), "Actual contour reflection retains signed edge and velocity gates");
            }
        }
        AssertTrue(overreadCases > 0, "Fixture exercises adjacent instruction words, not just authored geometry");
        Console.WriteLine($"Kraid nail contour: 983040 actual edge/direction probes pass bus-free; {overreadCases} relative-Y words select adjacent records.");
    }
}
