using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map.Artwork;

/// <summary>Checks installed station icons against native discovery rules and OAM.</summary>
internal static class MapStationLayoutVerification
{
    /// <summary>Compares every discovery subset's station icons with native OAM.</summary>
    internal static void VerifyMapStationLayout(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        int combinations = 0;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            var rules = MapStationLayoutFile.Markers.Where(rule => rule.Area == area).ToArray();
            for (int mask = 0; mask < (1 << rules.Length); mask++)
            {
                var system = new SharedGameState();
                for (int i = 0; i < rules.Length; i++)
                    if ((mask & (1 << i)) != 0) system.MarkExploredMapTile(area, rules[i].Cell);
                var native = new FileSelectMapIcons(system, area);
                var installed = new FileSelectMapIcons(system, area);
                native.BindStations(original.Stations);
                native.BindLandmarks(original.Landmarks);
                native.BindSprites(MenuSheetFixtures.MapSprites(InstalledAreaMapPresentation.SpritesOf(original)));
                installed.BindStations(original.Stations);
                installed.BindLandmarks(original.Landmarks);
                installed.BindSprites(MenuSheetFixtures.MapSprites(InstalledAreaMapPresentation.SpritesOf(original)));
                AssertTrue(Draw(native).SequenceEqual(Draw(installed)), $"station OAM stock parity {area}/{mask}");
                combinations++;
            }
        }
        Console.WriteLine($"Station layout: {combinations} discovery subsets match native OAM.");

        static byte[] Draw(FileSelectMapIcons icons)
        {
            var oam = ScreenSpritesFixtures.Blank(); oam.BeginFrame(); icons.DrawBeforeMarker(oam, new(0, 0)); oam.FinalizeFrame();
            return oam.LowTable.ConcatToArray(oam.HighTable);
        }
    }

}
