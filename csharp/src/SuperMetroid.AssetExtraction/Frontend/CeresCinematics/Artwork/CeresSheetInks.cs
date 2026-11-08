using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Where the Ceres cinematics' colours lie among their sheets' inks (#1512): the sprite set each Ceres actor colouring draws
/// in, and the background inks the gunship engine's flicker recolours on the gunship's floor. A colour is its set's place
/// times sixteen plus its colour within the set, as the Ceres flight loads its palette.
/// </summary>
internal static class CeresSheetInks
{
    /// <summary>The colours of one set.</summary>
    internal const int SetColours = 16;

    /// <summary>The sprite set each actor colouring draws in.</summary>
    private static readonly Dictionary<CeresSpriteColouring, int> Sets = new()
    {
        [CeresSpriteColouring.SpaceObjects] = 4,
        [CeresSpriteColouring.Station] = 6,
        [CeresSpriteColouring.Zebes] = 7,
        [CeresSpriteColouring.ZebesTitle] = 0,
        [CeresSpriteColouring.Explosions] = 5,
    };

    /// <summary>The background ink the gunship engine's flicker writes first: the last colour of the sixth background set.</summary>
    internal const int EngineFirst = 5 * SetColours + 15;

    /// <summary>The ink after the last the engine's flicker may write: the end of its background set.</summary>
    internal const int EngineEnd = 6 * SetColours;

    /// <summary>The sprite set <paramref name="colouring"/> draws in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring is not a Ceres actor colouring.</exception>
    internal static int SpriteSet(CeresSpriteColouring colouring) => Sets.TryGetValue(colouring, out int set)
        ? set
        : throw new ArgumentOutOfRangeException(nameof(colouring), colouring, "Not a Ceres actor colouring.");

    /// <summary>
    /// The shade of every record the Ceres destruction's gunship engine flicker shows, coloured from
    /// <paramref name="colours"/> over the background inks it recolours, which the gunship's floor shows it in.
    /// </summary>
    /// <exception cref="InvalidDataException">A record has no installed colour, or writes past the engine's set.</exception>
    internal static IReadOnlyList<SheetShade> EngineShades(IPaletteFxColorSource colours) =>
        PaletteFxScreenShades.Of([CeresDestructionSceneDefinitions.GunshipEngineFlicker], PaletteFxColours.CutsceneGunshipEngine,
            new InkRange(EngineFirst, EngineEnd), Ensure.NotNull(colours));

    /// <summary>
    /// The drawn shade of every Ceres actor colouring (#1513): its sprite set's colours from <paramref name="sprites"/> over
    /// that set's own inks, which every frame the colouring draws is coloured in, so the shade shows each such frame as the
    /// sheet pictures it and carries whether its draws take part in colour math.
    /// </summary>
    internal static IReadOnlyList<SheetShade> DrawnShades(RgbColor[] sprites)
    {
        Ensure.NotNull(sprites);
        return [.. Enum.GetValues<CeresSpriteColouring>().Select(colouring => SheetShade.DrawnRun(CeresShades.Drawn(colouring),
            SpriteSet(colouring) * SetColours, sprites.AsSpan(SpriteSet(colouring) * SetColours, SetColours), SpriteSet(colouring)))];
    }
}
