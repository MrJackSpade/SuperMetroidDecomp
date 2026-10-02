using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidBodyContour(SuperMetroidAddressSpace rom)
    {
        VerifyKraidBodyContourCases(rom);
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        var overlaps = typeof(RoomEnemySystem).GetMethod("KraidOuterBodyOverlapsShot", flags)!
            .CreateDelegate<Func<RoomEnemySlot, KraidEnemyState, SamusProjectileSlot, bool>>(enemies);
        var body = enemies.Slots[0];
        var state = new KraidEnemyState { VulnerableMouthHitbox = 0 };
        var shot = new SamusProjectileSystem().Slots[0];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            short y = unchecked((short)raw);
            short expected = NativeKraidBodyLeftEdge(rom, y);
            body.YPosition = (ushort)raw;
            shot.YPosition = unchecked((ushort)(raw + y));
            foreach (ushort bodyX in new ushort[] { 256, 32768, 65520 })
            foreach (ushort radius in new ushort[] { 0, 4, 16 })
            for (int delta = -1; delta <= 1; delta++)
            {
                body.XPosition = bodyX;
                shot.XRadius = radius;
                shot.XPosition = unchecked((ushort)(bodyX + expected - radius + delta));
                bool reference = shot.XPosition + radius > bodyX + expected;
                AssertEqual(reference, overlaps(body, state, shot), "Actual contour collision edge, translation and strict inequality");
            }
        }
        Console.WriteLine("Kraid body contour: every signed Y and 1769472 actual collision boundary probes pass with ROM reads forbidden.");
    }
}
