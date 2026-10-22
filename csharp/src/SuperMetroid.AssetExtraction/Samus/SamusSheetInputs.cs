using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed stock artwork Samus's projectile and grapple sheets compose from (#1392), loaded once for all of them.
/// </summary>
/// <param name="Projectiles">Compositions, beam, trail and grapple characters, beam palettes and trail and grapple appearances.</param>
/// <param name="StandardObjects">The standard OBJ sheet as a page.</param>
/// <param name="Palettes">The starting and common sprite colours a room load copies.</param>
/// <param name="Intro">The intro's characters and palette.</param>
internal sealed record SamusSheetInputs(InstalledProjectilePresentation Projectiles, SpriteArtwork StandardObjects,
    GameplayBasePaletteCatalog Palettes, IntroCinematicArtworkCatalog Intro)
{
    /// <summary>Loads the stock artwork from installed content.</summary>
    internal static SamusSheetInputs Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        return new(
            ProjectilePresentationFiles.Load(Path.Combine(content, GameInstallationLayout.ProjectileDirectoryName), null),
            ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
                StandardObjectArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.StandardObjectDirectoryName), null).Transfer.Span),
            GameplayBasePaletteFiles.Load(Path.Combine(content, GameInstallationLayout.GameplayBasePaletteDirectoryName), null),
            IntroCinematicArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.IntroCinematicDirectoryName), null));
    }

    /// <summary>CGRAM as a room load leaves it: the starting image with the common and enemy-projectile sprite rows.</summary>
    internal SnesCgram RoomLoadColours()
    {
        var cgram = new SnesCgram();
        Palettes.LoadInitial(cgram);
        Palettes.LoadCommonSprites(cgram, RoomLoadingRomData.CommonGameplaySpritePaletteCgramIndex);
        Palettes.LoadEnemyProjectileSprites(cgram, RoomLoadingRomData.InitialEnemyProjectilePaletteCgramIndex);
        return cgram;
    }
}
