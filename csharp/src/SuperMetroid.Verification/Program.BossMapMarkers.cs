using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyPauseBossMarkers()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int markers = 0;
        for (int area = 0; area < 6; area++)
        {
            ushort list = RomDataReader.ReadWordFixedBank(bus, FileSelectMapIconRomData.BossLists + area * 2);
            if (list == 0) continue;
            for (int record = 0; record < 8; record++)
            {
                ushort x = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.MenuObjectBank | (list + record * 4));
                if (x == ushort.MaxValue) break;
                if (x == ushort.MaxValue - 1) continue;
                ushort y = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.MenuObjectBank | (list + record * 4 + 2));
                var system = new Bank80SystemState();
                system.SetAreaMapAcquired(area);
                var pause = new PauseMenuState(bus, new SamusState(), system, (AreaId)area, 0, 0);
                // Center the marker under test without changing its authored position.
                typeof(PauseMenuState).GetField("mapHorizontalScroll", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .SetValue(pause, unchecked((ushort)(x - 128)));
                typeof(PauseMenuState).GetField("mapVerticalScroll", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .SetValue(pause, unchecked((ushort)(y - 112)));
                var alive = pause.Render();
                byte[] bosses = new byte[8];
                bosses[area] = (byte)(1 << record);
                system.LoadBossBytes(bosses);
                var dead = pause.Render();
                int changed = 0;
                for (int pixel = 0; pixel < alive.Length; pixel++)
                {
                    if (alive[pixel] == dead[pixel]) continue;
                    AssertTrue(Math.Abs(pixel % 256 - 128) <= 16 && Math.Abs(pixel / 256 - 112) <= 16,
                        "defeating one boss changes only its centered map marker");
                    changed++;
                }
                AssertTrue(changed > 0, $"area {area} boss bit {record} must visibly change on defeat");
                markers++;
            }
        }
        AssertTrue(markers > 0, "retail boss lists contain markers");
        Console.WriteLine($"Pause boss markers: {markers} authored markers visibly change on defeat at their coordinates.");
    }
}
