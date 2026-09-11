using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyNativeBossMarkers(string path)
    {
        AssertEqual("B6A536857F129A6F1F8BC1C92EFDECCA8C58EBAF9AC626DC2EC6E3FB5EDC50DD",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))), "native boss-map trace");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var trace = File.OpenText(path);
        AssertEqual("area,map,bits,bytes,low,high", trace.ReadLine(), "boss-map schema");
        int cases = 0;
        for (int area = 0; area < 6; area++)
        for (int map = 0; map < 2; map++)
        for (int bits = 0; bits < 256; bits++)
        {
            var system = new Bank80SystemState();
            if (map != 0) system.SetAreaMapAcquired(area);
            byte[] bosses = new byte[8];
            bosses[area] = (byte)bits;
            system.LoadBossBytes(bosses);
            var oam = new OamBuffer();
            oam.BeginFrame();
            new FileSelectMapIcons(bus, system, (AreaId)area).DrawBossMarkers(oam, 64, 16);
            string actual = $"{area},{map},{bits},{oam.NextByteOffset},{Convert.ToHexString(oam.LowTable[..oam.NextByteOffset])},{Convert.ToHexString(oam.HighTable)}";
            AssertEqual(trace.ReadLine(), actual, "native boss-marker OAM positions/artwork/palette/order/visibility");
            cases++;
        }
        AssertTrue(trace.ReadLine() is null, "native boss-map trace consumed");
        Console.WriteLine($"Boss-marker native OAM: {cases} area/download/boss-byte combinations match.");
    }

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
                AssertEqual(0, record, "retail authored boss markers use area-boss bit zero");
                for (int other = 0; other < 6; other++)
                    if (other != area) system.SetBossBits(other, BossBits.AreaBoss);
                AssertTrue(alive.SequenceEqual(pause.Render()), "other-area defeats do not change this map");
                system.SetBossBits(area, BossBits.AreaBoss);
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
                // A newly opened pause page must observe the same live progression
                // byte, not an icon cached before the boss died.
                AssertTrue(dead.SequenceEqual(CenteredPause(bus, system).Render()),
                    "reopening pause preserves defeated marker pixels");

                // Exercise the production snapshot, SRAM checksum/slot codec, JSON
                // format and restore path, entirely in disposable memory.
                var source = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
                new SuperMetroidSaveRam(source).SaveSlot(0,
                    SuperMetroidSaveSnapshot.Capture(new SamusState(), system, (ushort)area, 0));
                string json = GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(source));
                var restored = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
                GameSaveJsonCodec.Apply(GameSaveJsonCodec.Deserialize(json), restored);
                var slot = new SuperMetroidSaveRam(restored).ReadSlot(0) ??
                    throw new InvalidDataException("Boss-marker fixture lost its save slot.");
                var restoredSystem = new Bank80SystemState();
                slot.ApplyTo(new SamusState(), restoredSystem);
                AssertTrue(dead.SequenceEqual(CenteredPause(restored, restoredSystem).Render()),
                    "JSON/SRAM reload preserves the complete defeated pause image");
                // The existing-save menu creates its own progression owner from the
                // slot. Compare that actual constructor path with the live source.
                var savedMenu = new FileSelectMapMenuState(restored, new SuperMetroid.Core.Audio.CartridgeAudioState(), slot, 0);
                var savedGraphics = (FileSelectRoomMapGraphics)typeof(FileSelectMapMenuState)
                    .GetField("roomGraphics", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .GetValue(savedMenu)!;
                var liveGraphics = new FileSelectRoomMapGraphics(bus, system, (AreaId)area);
                var marker = new FileSelectStationMarker(bus, (AreaId)area, 0);
                ushort scrollX = unchecked((ushort)(x - 128)), scrollY = unchecked((ushort)(y - 112));
                AssertTrue(liveGraphics.Render(scrollX, scrollY, marker).SequenceEqual(
                    savedGraphics.Render(scrollX, scrollY, marker)),
                    "existing-save menu restores exact boss-marker pixels with its slot-owned progression");
                var runtime = new SuperMetroidRuntime(restored);
                runtime.InitializeHud(HudSnapshot.CeresDebug);
                runtime.RunNmi(0, true);
                runtime.InitializeStartingCeresRoom();
                runtime.InitializeCeresStartSamus();
                slot.ApplyTo(runtime.Samus!, runtime.System);
                ushort room = LoadStationEntry.Load(restored, (AreaId)area, 0).RoomPointer;
                for (int entry = 0; entry < 2; entry++)
                {
                    runtime.LoadCartridgeRoomForDebug(room, 0, 0);
                    AssertTrue(dead.SequenceEqual(CenteredPause(restored, runtime.System).Render()),
                        "ordinary room load/reload preserves defeated marker and acquired map");
                }

                PauseMenuState CenteredPause(ISnesAddressSpace addressSpace, Bank80SystemState state)
                {
                    var reopened = new PauseMenuState(addressSpace, new SamusState(), state, (AreaId)area, 0, 0);
                    typeof(PauseMenuState).GetField("mapHorizontalScroll", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                        .SetValue(reopened, unchecked((ushort)(x - 128)));
                    typeof(PauseMenuState).GetField("mapVerticalScroll", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                        .SetValue(reopened, unchecked((ushort)(y - 112)));
                    return reopened;
                }
                markers++;
            }
        }
        AssertTrue(markers > 0, "retail boss lists contain markers");
        Console.WriteLine($"Boss markers: {markers} defeat transitions, pause reopenings and JSON/SRAM/file-select pixel round trips pass.");
    }
}
