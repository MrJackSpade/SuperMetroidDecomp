using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed stock artwork Samus's projectile, grapple, body and overlay sheets compose from (#1392), loaded once for
/// all of them, with the colour tables every one of her sheets is pre-coloured from (#1512).
/// </summary>
/// <param name="Projectiles">Compositions, beam, trail and grapple characters, beam palettes and trail and grapple appearances.</param>
/// <param name="StandardObjects">The standard OBJ sheet as a page.</param>
/// <param name="Palettes">The starting and common sprite colours a room load copies.</param>
/// <param name="Intro">The intro's characters and palette.</param>
/// <param name="SamusBody">Samus's body characters, bound to the body's frame selections and spritemaps.</param>
/// <param name="DeathTiles">The characters of Samus's suit explosion.</param>
/// <param name="ShadeSources">The colour tables her shades are pre-coloured from.</param>
/// <param name="AnimationContinuation">What an animation frame past a pose's program reads, which decides the frames her body can show.</param>
internal sealed record SamusSheetInputs(InstalledProjectilePresentation Projectiles, SpriteArtwork StandardObjects,
    GameplayBasePalette Palettes, IntroCinematicSourceArtwork Intro, SamusBodyCharacterArtwork SamusBody,
    SamusDeathTileAtlas DeathTiles, SamusShadeSources ShadeSources, LaidOutProgramContinuation<SamusPoseId, SamusAnimationProgramStep> AnimationContinuation)
{
    /// <summary>Loads the stock artwork and colour tables from installed content.</summary>
    internal static SamusSheetInputs Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        string samusBody = Path.Combine(content, GameInstallationLayout.SamusBodyDirectoryName);
        string map = Path.Combine(content, GameInstallationLayout.MapDirectoryName);
        InstalledProjectilePresentation projectiles = ProjectilePresentationFiles.Load(Path.Combine(content, GameInstallationLayout.ProjectileDirectoryName));
        GameplayBasePalette palettes = GameplayBasePaletteFiles.Load(Path.Combine(content, GameInstallationLayout.GameplayBasePaletteDirectoryName));
        ImportIntroCinematicArtwork intro = IntroCinematicArtworkFiles.LoadForImport(Path.Combine(content, GameInstallationLayout.IntroCinematicDirectoryName));
        // Her colour tables are the ones the map presentation was read with, edits included.
        ImportAreaMapPresentation maps = MapPresentationFiles.LoadForImport(map);
        SamusMapColourTables tables = maps.SamusColours;
        string overreads = Path.Combine(content, GameInstallationLayout.SamusOverreadDirectoryName);
        SamusOverrunColours overruns = SamusOverreadFiles.LoadColours(overreads);
        var sources = new SamusShadeSources(
            palettes.Starting,
            [.. intro.Palette.SpriteColours.AsSpan(SamusSheetInks.Suit, SamusSheetInks.GroupColours)],
            [.. intro.Palette.SpriteColours.AsSpan(SamusSheetInks.Beams, SamusSheetInks.GroupColours)],
            tables.Suits,
            tables.Charge,
            tables.HyperBeam,
            tables.FullBodyCycles,
            overruns.SpeedBooster,
            tables.Hurt,
            tables.Visor,
            projectiles.BeamTiles.Palettes ?? throw new InvalidDataException("Samus's sheets require the installed beam palettes."),
            overruns.Chainsaw,
            tables.CrystalFlash,
            projectiles.BeamTiles.HyperBeamFxColors ?? throw new InvalidDataException("Samus's sheets require the installed Hyper Beam colours."),
            SamusDeathPaletteArtworkFiles.Load(samusBody),
            maps.RoomPaletteFx);
        return new(
            projectiles,
            LoadStandardObjects(content),
            palettes,
            intro,
            SamusBodyArtworkFiles.LoadCharacters(samusBody),
            SamusDeathTileArtworkFiles.Load(samusBody),
            sources,
            SamusOverreadFiles.LoadAnimationContinuation(overreads));
    }

    /// <summary>The standard OBJ sheet installed under <paramref name="content"/>, as a page.</summary>
    internal static SpriteArtwork LoadStandardObjects(string content) =>
        ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
            StandardObjectArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.StandardObjectDirectoryName)).Characters.Span);

    /// <summary>The shades every one of Samus's sheets is pre-coloured in (#1512), computed once.</summary>
    internal IReadOnlyList<SheetShade> Shades => field ??= ShadeSources.Shades();

    /// <summary>
    /// The sprite colours Samus is drawn with in the Power Suit with the power beam; the other suits and her colour
    /// effects show through the sheet's shades.
    /// </summary>
    internal RgbColor[] SuitedColours() =>
        WithBeams(WithRun(RoomLoadSpriteColours(), SamusSheetInks.Suit,
            [.. Enumerable.Range(0, SamusColourSets.ColoursPerSet).Select(colour => ShadeSources.Suits.Resolve(SamusSuit.Power, colour))]),
            SamusBeamCombination.Power);

    /// <summary><paramref name="colours"/>, sprite colours, with <paramref name="combination"/>'s beam colours in her beams' place.</summary>
    internal RgbColor[] WithBeams(RgbColor[] colours, SamusBeamCombination combination) =>
        WithRun(colours, SamusSheetInks.Beams, SamusBeamColours.Of(combination, ShadeSources.BeamPalettes, ShadeSources.Chainsaw));

    /// <summary>The sprite colours as a room load leaves them: the starting colours with the common and enemy-projectile groups.</summary>
    internal RgbColor[] RoomLoadSpriteColours() => RoomLoadPalettes.SpriteColours(Palettes);

    /// <summary><paramref name="colours"/> with <paramref name="run"/> laid from ink <paramref name="first"/>.</summary>
    private static RgbColor[] WithRun(RgbColor[] colours, int first, ReadOnlySpan<RgbColor> run)
    {
        run.CopyTo(colours.AsSpan(first, run.Length));
        return colours;
    }
}
