using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed map landmarks against native OAM.</summary>
internal static class MapLandmarksVerification
{
    /// <summary>Checks boss and elevator visibility across area state against native OAM.</summary>
    internal static void VerifyMapLandmarks(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        var guard = NewLandmarkGuard(bus);
        var iconGuard = NewLandmarkGuard(bus, blockGunship: true);
        int cases = 0;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        foreach (bool downloaded in new[] { false, true })
        for (int bits = 0; bits <= (int)BossBitMasks.AllKnown; bits++)
        {
            // Every combination of the named boss flags; no routine tests any other (#1475).
            var system = new SharedGameState();
            system.Progress.SetDefeatedBosses(area, (BossBits)bits);
            if (downloaded) system.SetAreaMapAcquired(area);
            var native = new FileSelectMapIcons(system, area);
            var installed = new FileSelectMapIcons(system, area);
            native.BindLandmarks(original.Landmarks);
            native.BindSprites(MenuSheetFixtures.MapSprites(InstalledAreaMapPresentation.SpritesOf(original)));
            installed.BindLandmarks(original.Landmarks);
            installed.BindSprites(MenuSheetFixtures.MapSprites(InstalledAreaMapPresentation.SpritesOf(original)));
            AssertTrue(Draw(native, area).SequenceEqual(Draw(installed, area)), $"landmark OAM {area}/{bits}/{downloaded}");
            AssertEqual((BossBits)bits, system.GetBossBits(area), "landmark drawing preserves boss state");
            AssertEqual(downloaded, system.HasAreaMap(area), "landmark drawing preserves map acquisition");
            cases++;
        }
        Console.WriteLine($"Map landmarks: {cases} boss/download states match native OAM.");

        static byte[] Draw(FileSelectMapIcons icons, AreaId area)
        {
            var oam = ScreenSpritesFixtures.Blank(); oam.BeginFrame(); icons.DrawBossMarkers(oam, new(24, 8));
            if (area != AreaId.Ceres) icons.DrawAfterMarker(oam, new(24, 8));
            oam.FinalizeFrame(); return oam.LowTable.ConcatToArray(oam.HighTable);
        }
    }

    /// <summary>Blocks native landmark coordinate reads after their installed presentation is bound.</summary>
    private static MutableCartridgeReadGuard NewLandmarkGuard(ISnesAddressSpace source, bool blockGunship = false)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var blocked = new HashSet<int>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            AddList(MapSourceDefinitions.Landmarks.BossLists, area, 4);
            if (area != AreaId.Ceres) AddList(MapSourceDefinitions.Landmarks.ElevatorLists, area, 6);
        }
        // Full room rendering still reads selected-save coordinates. The direct
        // icon comparison can additionally block the shared gunship coordinate.
        if (blockGunship)
        {
            int table = (int)FrontendSourceDefinitions.SavePointMapPointers;
            int pointer = RomDataReader.ReadWordFixedBank(cartridge, table);
            blocked.Add(table); blocked.Add(table + 1);
            for (int i = 0; i < 4; i++) blocked.Add(MapSourceDefinitions.Menu.ObjectBank | (pointer + i));
        }
        void AddList(int table, AreaId area, int stride)
        {
            int entry = table + AreaIds.ToIndex(area) * 2;
            blocked.Add(entry); blocked.Add(entry + 1);
            int list = RomDataReader.ReadWordFixedBank(cartridge, entry);
            if (list == 0) return;
            for (int record = 0; ; record++)
            {
                int address = MapSourceDefinitions.Menu.ObjectBank | (list + record * stride);
                blocked.Add(address); blocked.Add(address + 1);
                if (RomDataReader.ReadWordFixedBank(cartridge, address) == ushort.MaxValue) break;
                for (int i = 2; i < stride; i++) blocked.Add(address + i);
            }
        }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(blocked.Contains, _ => "Installed landmarks read boss/elevator ROM definitions.");
        return guard;
    }
}
