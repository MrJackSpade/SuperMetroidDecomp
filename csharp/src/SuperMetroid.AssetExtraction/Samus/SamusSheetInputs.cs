using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Palettes;

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
/// <param name="Overreads">The decoded glitch reads, which hold the Chainsaw beam's colours.</param>
internal sealed record SamusSheetInputs(InstalledProjectilePresentation Projectiles, SpriteArtwork StandardObjects,
    GameplayBasePalette Palettes, IntroCinematicArtwork Intro, SamusBodyCharacterArtwork SamusBody,
    SamusDeathTileAtlas DeathTiles, SamusShadeSources ShadeSources, SamusOverreadServices Overreads)
{
    /// <summary>The shades every one of her sheets is pre-coloured in, computed once.</summary>
    private IReadOnlyList<SheetShade>? shades;

    /// <summary>Loads the stock artwork and colour tables from installed content.</summary>
    internal static SamusSheetInputs Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        string samusBody = Path.Combine(content, GameInstallationLayout.SamusBodyDirectoryName);
        string map = Path.Combine(content, GameInstallationLayout.MapDirectoryName);
        InstalledProjectilePresentation projectiles = ProjectilePresentationFiles.Load(Path.Combine(content, GameInstallationLayout.ProjectileDirectoryName), null);
        GameplayBasePalette palettes = GameplayBasePaletteFiles.Load(Path.Combine(content, GameInstallationLayout.GameplayBasePaletteDirectoryName), null);
        IntroCinematicArtwork intro = IntroCinematicArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.IntroCinematicDirectoryName), null);
        SamusOverreadServices overreads = SamusOverreadFiles.Load(Path.Combine(content, GameInstallationLayout.SamusOverreadDirectoryName));
        var sources = new SamusShadeSources(
            palettes.starting,
            intro.Palette.Colours.SpriteSet(SamusSheetInks.Suit / SamusSheetInks.GroupColours).ToArray(),
            intro.Palette.Colours.SpriteSet(SamusSheetInks.Beams / SamusSheetInks.GroupColours).ToArray(),
            Read(map, SamusSuitColorFile.FileName, SamusSuitColorFile.Read),
            Read(map, SamusChargeColorFormat.FileName, json => SamusChargeColorFile.Read(json).ToColours()),
            Read(map, SamusHyperBeamColorFormat.FileName, json => SamusHyperBeamColorFile.Read(json).ToColours()),
            Read(map, SamusFullBodyCycleColorFormat.FileName, json => SamusFullBodyCycleColorFile.Read(json).ToColours()),
            overreads.SpeedBoosterColours,
            Read(map, SamusHurtColorFile.FileName, SamusHurtColorFile.Read),
            Read(map, SamusVisorColorFile.FileName, SamusVisorColorFile.Read),
            projectiles.BeamTiles.Palettes ?? throw new InvalidDataException("Samus's sheets require the installed beam palettes."),
            overreads.ChainsawColours,
            Read(map, CrystalFlashColorFile.FileName, CrystalFlashColorFile.Read),
            projectiles.BeamTiles.HyperBeamFxColors ?? throw new InvalidDataException("Samus's sheets require the installed Hyper Beam colours."),
            SamusDeathPaletteArtworkFiles.Load(samusBody, null),
            MapPresentationFiles.Load(map, null).RoomPaletteFx);
        return new(
            projectiles,
            ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
                StandardObjectArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.StandardObjectDirectoryName), null).Characters.Span),
            palettes,
            intro,
            SamusBodyArtworkFiles.LoadCharacters(samusBody, null),
            SamusDeathTileArtworkFiles.Load(samusBody, null),
            sources,
            overreads);
    }

    /// <summary>Reads one validated colour table the map installation holds.</summary>
    private static T Read<T>(string directory, string fileName, Func<Stream, T> read)
    {
        using FileStream json = File.OpenRead(Path.Combine(directory, fileName));
        return read(json);
    }

    /// <summary>The shades every one of Samus's sheets is pre-coloured in (#1512).</summary>
    internal IReadOnlyList<SheetShade> Shades => shades ??= SamusSheetShades.Of(ShadeSources);

    /// <summary>
    /// The sprite colours Samus is drawn with in the Power Suit with the power beam; the other suits and her colour
    /// effects show through the sheet's shades.
    /// </summary>
    internal RgbColor[] SuitedColours() =>
        WithBeams(WithRun(RoomLoadSpriteColours(), SamusSheetInks.Suit,
            [.. Enumerable.Range(0, SamusColourDefinitions.SuitColours).Select(colour => ShadeSources.Suits.Resolve(SamusSuit.Power, colour))]),
            SamusBeamCombination.Power);

    /// <summary><paramref name="colours"/>, sprite colours, with <paramref name="combination"/>'s beam colours in her beams' place.</summary>
    internal RgbColor[] WithBeams(RgbColor[] colours, SamusBeamCombination combination) =>
        WithRun(colours, SamusSheetInks.Beams, SamusBeamColours.Of(combination, ShadeSources.BeamPalettes, ShadeSources.Chainsaw));

    /// <summary>The sprite colours as a room load leaves them: the starting colours with the common and enemy-projectile groups.</summary>
    internal RgbColor[] RoomLoadSpriteColours() => RoomLoadPalettes.Colours(Palettes).SpriteSets.All.Colors.ToArray();

    /// <summary><paramref name="colours"/> with <paramref name="run"/> laid from ink <paramref name="first"/>.</summary>
    private static RgbColor[] WithRun(RgbColor[] colours, int first, ReadOnlySpan<RgbColor> run)
    {
        run.CopyTo(colours.AsSpan(first, run.Length));
        return colours;
    }
}
