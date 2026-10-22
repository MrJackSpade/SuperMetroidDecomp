using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes Samus's projectile sheets (#1392) from the installed stock artwork: every projectile, bomb and flare
/// composition, trail pose and the unshown trail, over the OBJ page the context's beam combination binds, coloured from
/// the sprite rows as that context leaves them: a room load and the combination's beam palette in gameplay, the intro's
/// own palette in its flashbacks.
/// </summary>
internal static class ProjectileSheetComposer
{
    /// <summary>The OBJ palette row of parts that do not choose their own, as the cartridge's projectile draws leave it.</summary>
    private const int DefaultPalette = 0;

    /// <summary>The priority of parts that do not choose their own, as the cartridge's projectile draws leave it.</summary>
    private const int DefaultPriority = 0;

    /// <summary>Composes one projectile sheet.</summary>
    /// <param name="inputs">The installed stock artwork the sheet composes from.</param>
    /// <param name="sheet">A projectile sheet.</param>
    internal static ComposedSpriteSheet Compose(ProjectileSheetInputs inputs, SpriteSheetId sheet)
    {
        ProjectileSheetSources sources = Sources(inputs, sheet);
        return SpriteSheetComposer.Compose(sources.Frames, sources.Page, sources.SpriteColours, DefaultPalette, DefaultPriority);
    }

    /// <summary>What one projectile sheet composes: its named frames, the OBJ page they index and the sprite rows' colours.</summary>
    /// <param name="inputs">The installed stock artwork the sheet composes from.</param>
    /// <param name="sheet">A projectile sheet.</param>
    internal static ProjectileSheetSources Sources(ProjectileSheetInputs inputs, SpriteSheetId sheet)
    {
        Ensure.NotNull(inputs);
        InstalledProjectilePresentation projectiles = inputs.Projectiles;
        var cgram = new SnesCgram();
        SpriteArtwork basePage;
        SamusBeamCombination beams;
        if (sheet == SpriteSheetId.IntroProjectiles)
        {
            IntroCinematicArtworkCatalog intro = inputs.Intro;
            basePage = intro.ObjectPage();
            beams = SamusBeamCombination.Power;
            // `$8B:A3AC` loads the beam palette, then the intro's whole palette over it.
            SamusProjectileSystem.LoadBeamPalette(cgram, beams, projectiles.BeamTiles.Palettes);
            intro.Palette.LoadTo(cgram);
        }
        else
        {
            basePage = inputs.StandardObjects;
            beams = Combination(sheet);
            GameplayBasePaletteCatalog palettes = inputs.Palettes;
            palettes.LoadInitial(cgram);
            palettes.LoadCommonSprites(cgram, RoomLoadingRomData.CommonGameplaySpritePaletteCgramIndex);
            palettes.LoadEnemyProjectileSprites(cgram, RoomLoadingRomData.InitialEnemyProjectilePaletteCgramIndex);
            SamusProjectileSystem.LoadBeamPalette(cgram, beams, projectiles.BeamTiles.Palettes);
        }
        SpriteArtwork page = new ProjectileObjectPages(basePage, projectiles.BeamTiles, projectiles.Trails.Tiles ??
            throw new InvalidDataException("The installed projectile trails have no characters.")).For(beams);

        var frames = new List<KeyValuePair<string, SpriteFrame>>();
        foreach (ProjectileSpriteId composition in ProjectileSpriteDefinitions.All)
            frames.Add(KeyValuePair.Create(ProjectileSheetDefinitions.ProjectileFrame(composition), projectiles.Catalog.Frame(composition)));
        foreach (ProjectileSpriteId composition in ProjectileSheetDefinitions.FlareCompositions)
        {
            ushort selector = checked((ushort)ChargeFlareSpriteDefinitions.Selectors.ToList().IndexOf(composition));
            frames.Add(KeyValuePair.Create(ProjectileSheetDefinitions.FlareFrame(composition), projectiles.FlareCompositions.Frame(selector)));
        }
        foreach (ProjectileTrailFrame pose in Enum.GetValues<ProjectileTrailFrame>())
            frames.Add(KeyValuePair.Create(ProjectileSheetDefinitions.TrailFrame(pose), ObjPartFrames.SmallObject(projectiles.Trails.Resolve(pose), 0)));
        frames.Add(KeyValuePair.Create(ProjectileSheetDefinitions.UnshownTrailFrame, ObjPartFrames.SmallObject(default, 0)));
        return new ProjectileSheetSources(frames, page, cgram.Colors[SpriteColours..].ToArray());
    }

    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What a projectile sheet composes.</summary>
    /// <param name="Frames">The frames by sheet name, in sheet order.</param>
    /// <param name="Page">The OBJ page their characters index.</param>
    /// <param name="SpriteColours">The eight sprite rows' colours.</param>
    internal sealed record ProjectileSheetSources(IReadOnlyList<KeyValuePair<string, SpriteFrame>> Frames, SpriteArtwork Page, Bgr555[] SpriteColours);

    /// <summary>The installed stock artwork every projectile sheet composes from, loaded once for all of them.</summary>
    /// <param name="Projectiles">Compositions, beam and trail characters, beam palettes and trail appearances.</param>
    /// <param name="StandardObjects">The standard OBJ sheet as a page.</param>
    /// <param name="Palettes">The starting and common sprite colours a room load copies.</param>
    /// <param name="Intro">The intro's characters and palette.</param>
    internal sealed record ProjectileSheetInputs(InstalledProjectilePresentation Projectiles, SpriteArtwork StandardObjects,
        GameplayBasePaletteCatalog Palettes, IntroCinematicArtworkCatalog Intro)
    {
        /// <summary>Loads the stock artwork from installed content.</summary>
        internal static ProjectileSheetInputs Load(string content)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(content);
            return new(
                ProjectilePresentationFiles.Load(Path.Combine(content, GameInstallationLayout.ProjectileDirectoryName), null),
                ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
                    StandardObjectArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.StandardObjectDirectoryName), null).Transfer.Span),
                GameplayBasePaletteFiles.Load(Path.Combine(content, GameInstallationLayout.GameplayBasePaletteDirectoryName), null),
                IntroCinematicArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.IntroCinematicDirectoryName), null));
        }
    }

    /// <summary>The beam combination whose gameplay sheet <paramref name="sheet"/> is.</summary>
    private static SamusBeamCombination Combination(SpriteSheetId sheet) =>
        ProjectileSheetDefinitions.Combinations.Single(beams => ProjectileSheetDefinitions.For(beams) == sheet);
}
