using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks the installed file-select world screens.</summary>
internal static class MapScreensVerification
{
    /// <summary>
    /// Checks that selecting another area and back reloads each of the six areas' authored world background, as a freshly
    /// opened world map of that area shows it. Both sides are built from the one installed catalog; the world and room
    /// screens are not compared with a second screen built the same way, which would compare a screen with itself.
    /// </summary>
    internal static void VerifyMapScreens(AreaMapPresentation original)
    {
        for (int index = 0; index < MapScreenDefinitions.ZebesAreas; index++)
        {
            FileSelectAreaMapGraphics fresh = World(original, index);
            FileSelectAreaMapGraphics reselected = World(original, index);
            reselected.SelectArea(AreaIds.Zebes[(index + 1) % AreaIds.Zebes.Count]);
            reselected.SelectArea(AreaIds.Zebes[index]);
            AssertTrue(ScenePixels.Same(fresh.BackgroundsScene(), reselected.BackgroundsScene()), "reselection reloads authored world background");
        }
        Console.WriteLine("Map screen resources: reselecting each of the six areas reloads its authored world background.");

        static FileSelectAreaMapGraphics World(AreaMapPresentation catalog, int area) =>
            new(AreaIds.Zebes[area], catalog.Backdrops, MenuSheetFixtures.ForFileSelectMap(InstalledFixtures.ImportOf(catalog)));
    }
}
