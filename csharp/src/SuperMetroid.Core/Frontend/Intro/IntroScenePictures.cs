using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening cinematic's scene pictures (#1467): each scene page, Samus's portrait and each eye frame drawn over it,
/// which the import composes once under the cinematic's palette. The cinematic places the pictures its scene state shows
/// on a plane the size of the cinematic's tilemaps.
/// </summary>
public static class IntroScenePictures
{
    /// <summary>The cells along each side of the scene plane.</summary>
    internal const int PlaneCells = IntroCinematicDefinitions.Planes.Width;

    /// <summary>The name of the portrait's picture.</summary>
    public const string Portrait = "portrait";

    /// <summary>The name of scene page <paramref name="page"/>'s picture.</summary>
    internal static string Page(IntroScenePage page) => $"page-{page}";

    /// <summary>The name of eye frame <paramref name="frame"/>'s picture.</summary>
    internal static string Eye(IntroEyeFrame frame) => $"eye-{frame}";

    /// <summary>The name of every picture the sheet holds.</summary>
    public static IEnumerable<string> FrameNames =>
        [.. Enum.GetValues<IntroScenePage>().Select(Page), Portrait, .. Enum.GetValues<IntroEyeFrame>().Select(Eye)];

    /// <summary>The plane showing scene page <paramref name="page"/>.</summary>
    internal static PicturedPlane PagePlane(InstalledSpriteSheets sheets, IntroScenePage page) =>
        Plane([new PlacedPlanePicture(Sheet(sheets).Frame(Page(page)), default)]);

    /// <summary>The plane showing the portrait, with <paramref name="eye"/>'s picture over its eye once the eye has shown a frame.</summary>
    internal static PicturedPlane PortraitPlane(InstalledSpriteSheets sheets, IntroEyeFrame? eye)
    {
        SpriteSheet sheet = Sheet(sheets);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        placed.Add(new(sheet.Frame(Portrait), default));
        if (eye is { } frame)
            placed.Add(new(sheet.Frame(Eye(frame)), new Position16<TilePlanePixels>(
                checked((ushort)(IntroEyeAnimationDefinitions.PortraitCell.X * ScreenGeometry.BackgroundTileSizePixels)),
                checked((ushort)(IntroEyeAnimationDefinitions.PortraitCell.Y * ScreenGeometry.BackgroundTileSizePixels)))));
        return Plane(placed.ToImmutable());
    }

    /// <summary>The installed scene picture sheet.</summary>
    private static SpriteSheet Sheet(InstalledSpriteSheets sheets) => Ensure.NotNull(sheets).Get(SpriteSheetId.IntroScenePictures);

    /// <summary>A scene plane holding <paramref name="placed"/>.</summary>
    private static PicturedPlane Plane(ImmutableArray<PlacedPlanePicture> placed) =>
        new(new Extent16<TilemapCells>(PlaneCells, PlaneCells), placed);
}
