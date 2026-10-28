using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed stock artwork Samus's projectile, grapple, body and overlay sheets compose from (#1392), loaded once for all of them.
/// </summary>
/// <param name="Projectiles">Compositions, beam, trail and grapple characters, beam palettes and trail and grapple appearances.</param>
/// <param name="StandardObjects">The standard OBJ sheet as a page.</param>
/// <param name="Palettes">The starting and common sprite colours a room load copies.</param>
/// <param name="Intro">The intro's characters and palette.</param>
/// <param name="SamusBody">Samus's body definitions, frame selections and spritemaps.</param>
/// <param name="DeathTiles">The characters of Samus's suit explosion.</param>
/// <param name="SuitColors">The three suits' colours.</param>
internal sealed record SamusSheetInputs(InstalledProjectilePresentation Projectiles, SpriteArtwork StandardObjects,
    GameplayBasePalette Palettes, IntroCinematicArtwork Intro, InstalledSamusBodyArtwork SamusBody,
    SamusDeathTileAtlas DeathTiles, InstalledSamusSuitColors SuitColors)
{
    /// <summary>Loads the stock artwork from installed content.</summary>
    internal static SamusSheetInputs Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        string samusBody = Path.Combine(content, GameInstallationLayout.SamusBodyDirectoryName);
        return new(
            ProjectilePresentationFiles.Load(Path.Combine(content, GameInstallationLayout.ProjectileDirectoryName), null),
            ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
                StandardObjectArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.StandardObjectDirectoryName), null).Characters.Span),
            GameplayBasePaletteFiles.Load(Path.Combine(content, GameInstallationLayout.GameplayBasePaletteDirectoryName), null),
            IntroCinematicArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.IntroCinematicDirectoryName), null),
            SamusBodyArtworkFiles.Load(samusBody, null),
            SamusDeathTileArtworkFiles.Load(samusBody, null),
            LoadSuitColors(Path.Combine(content, GameInstallationLayout.MapDirectoryName, SamusSuitColorFormat.FileName)));
    }

    /// <summary>The stock suit colours, which the validated map installation holds.</summary>
    private static InstalledSamusSuitColors LoadSuitColors(string path)
    {
        using FileStream json = File.OpenRead(path);
        return SamusSuitColorFile.Read(json);
    }

    /// <summary>The Power Suit's place in the suit colour tables.</summary>
    private const ushort PowerSuit = 0;

    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>
    /// The sprite colours Samus is drawn with in the Power Suit with the power beam; the other suits and palette effects
    /// recolour her sheets through their palette maps.
    /// </summary>
    internal RgbColor[] SuitedColours()
    {
        ScreenPalette cgram = RoomLoadColours();
        SuitColors.Apply(cgram.SpriteRow(SamusColourDefinitions.SuitInkRow), PowerSuit);
        SamusProjectileSystem.LoadBeamPalette(cgram.SpriteRow(SamusColourDefinitions.BeamInkRow), SamusBeamCombination.Power, Projectiles.BeamTiles.Palettes);
        return cgram.Colors[SpriteColours..].ToArray();
    }

    /// <summary>CGRAM as a room load leaves it: the starting image with the common and enemy-projectile sprite rows.</summary>
    internal ScreenPalette RoomLoadColours() => RoomLoadPalettes.Colours(Palettes);
}
