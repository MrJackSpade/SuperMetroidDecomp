using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks boss-map marker pixels as area-boss progress crosses save and room-load boundaries.</summary>
internal static class BossMapMarkersVerification
{
    /// <summary>Checks native marker placement, defeat changes, pause reopen, SRAM/JSON restore, and file-select rendering.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause-map boss marker coordinate lists")]
    internal static void VerifyPauseBossMarkers()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int markers = 0;
        for (int area = 0; area < 6; area++)
        {
            ushort list = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapIconRomData.BossLists + area * 2);
            if (list == 0) continue;
            for (int record = 0; record < 8; record++)
            {
                ushort x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.MenuObjectBank | (list + record * 4));
                if (x == ushort.MaxValue) break;
                if (x == ushort.MaxValue - 1) continue;
                ushort y = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.MenuObjectBank | (list + record * 4 + 2));
                var system = new Bank80SystemState();
                system.SetAreaMapAcquired(area);
                var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = new SamusState(), System = system, AreaIndex = (AreaId)area, RoomMapPosition = new(0, 0) });
                // Center the marker under test without changing its authored position.
                pause.mapScrollPosition = new Position16<MapPixels>(unchecked((ushort)(x - 128)), unchecked((ushort)(y - 112)));
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
                AssertTrue(dead.SequenceEqual(CenteredPause(system).Render()),
                    "reopening pause preserves defeated marker pixels");

                // Exercise the production snapshot, SRAM checksum/slot codec, JSON
                // format and restore path, entirely in disposable memory.
                var source = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
                source.SaveSlot(0,
                    SuperMetroidSaveSnapshot.Capture(new SamusState(), system, new SaveSnapshotContext { Area = (AreaId)area, SaveStation = 0 }));
                string json = GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(source));
                var restored = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
                var slot = GameSaveJsonCodec.ToStore(GameSaveJsonCodec.Deserialize(json), RetailPresentationFixture()).ReadSlot(0) ??
                    throw new InvalidDataException("Boss-marker fixture lost its save slot.");
                var restoredSystem = new Bank80SystemState();
                slot.ApplyTo(new SamusState(), restoredSystem);
                AssertTrue(dead.SequenceEqual(CenteredPause(restoredSystem).Render()),
                    "JSON/SRAM reload preserves the complete defeated pause image");
                // The existing-save menu creates its own progression owner from the
                // slot. Compare that actual constructor path with the live source.
                var savedMenu = new FileSelectMapMenuState(new SuperMetroid.Core.Audio.CartridgeAudioState(), slot, 0,
                    RetailPresentationFixture());
                var savedGraphics = savedMenu.roomGraphics;
                var liveGraphics = new FileSelectRoomMapGraphics(system, (AreaId)area, mapPresentation: RetailPresentationFixture());
                var marker = new FileSelectStationMarker((AreaId)area, 0, RetailPresentationFixture().SaveMarkers);
                ushort scrollX = unchecked((ushort)(x - 128)), scrollY = unchecked((ushort)(y - 112));
                AssertTrue(liveGraphics.Render(new(scrollX, scrollY), marker).SequenceEqual(
                    savedGraphics.Render(new(scrollX, scrollY), marker)),
                    "existing-save menu restores exact boss-marker pixels with its slot-owned progression");
                var runtime = CreateRetailRuntimeFixture();
                runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
                runtime.FrameBoundary.BeginAcceptedFrame(0);
                runtime.RoomLoader.InitializeStartingCeresRoom();
                runtime.RoomLoader.InitializeCeresStartSamus();
                slot.ApplyTo(runtime.Player.IntroducedSamus, runtime.System);
                ushort room = SuperMetroid.TestSupport.VerificationImporters.LoadStationEntryImporter.Load(restored, (AreaId)area, 0).RoomPointer;
                for (int entry = 0; entry < 2; entry++)
                {
                    runtime.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0));
                    AssertTrue(dead.SequenceEqual(CenteredPause(runtime.System).Render()),
                        "ordinary room load/reload preserves defeated marker and acquired map");
                }

                PauseMenuState CenteredPause(Bank80SystemState state)
                {
                    var reopened = CreateRetailPauseFixture(new PauseMenuEntry { Samus = new SamusState(), System = state, AreaIndex = (AreaId)area, RoomMapPosition = new(0, 0) });
                    reopened.mapScrollPosition = new Position16<MapPixels>(unchecked((ushort)(x - 128)), unchecked((ushort)(y - 112)));
                    return reopened;
                }
                markers++;
            }
        }
        AssertTrue(markers > 0, "retail boss lists contain markers");
        Console.WriteLine($"Boss markers: {markers} defeat transitions, pause reopenings and JSON/SRAM/file-select pixel round trips pass.");
    }

    /// <summary>Runs the pause and saved-game boss-marker verification.</summary>
    [VerificationCommand("--pause-boss-markers")]
    internal static int PauseBossMarkersCommand()
    {
        Suite(nameof(VerifyPauseBossMarkers), () => VerifyPauseBossMarkers());
        return 0;
    }
}
