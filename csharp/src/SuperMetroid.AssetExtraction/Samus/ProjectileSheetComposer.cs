using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes Samus's projectile sheets (#1392) from the installed stock artwork: every projectile, bomb and flare
/// composition, trail pose and the unshown trail, over the OBJ page the context's beam combination binds, coloured from
/// the sprite rows as that context leaves them: a room load and the combination's beam palette in gameplay, the intro's
/// own palette in its flashbacks.
/// </summary>
internal static class ProjectileSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What one projectile sheet composes: its named frames over the context's OBJ page, and the sprite rows' colours.</summary>
    /// <param name="inputs">The installed stock artwork the sheet composes from.</param>
    /// <param name="sheet">A projectile sheet.</param>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs, SpriteSheetId sheet)
    {
        Ensure.NotNull(inputs);
        InstalledProjectilePresentation projectiles = inputs.Projectiles;
        ScreenPalette cgram;
        SpriteArtwork basePage;
        SamusBeamCombination beams;
        if (sheet == SpriteSheetId.IntroProjectiles)
        {
            IntroCinematicArtworkCatalog intro = inputs.Intro;
            cgram = new ScreenPalette();
            basePage = intro.ObjectPage;
            beams = SamusBeamCombination.Power;
            // `$8B:A3AC` loads the beam palette, then the intro's whole palette over it.
            SamusProjectileSystem.LoadBeamPalette(cgram.SpriteRow(SamusColourDefinitions.BeamInkRow), beams, projectiles.BeamTiles.Palettes);
            intro.Palette.LoadTo(cgram);
        }
        else
        {
            cgram = inputs.RoomLoadColours();
            basePage = inputs.StandardObjects;
            beams = Combination(sheet);
            SamusProjectileSystem.LoadBeamPalette(cgram.SpriteRow(SamusColourDefinitions.BeamInkRow), beams, projectiles.BeamTiles.Palettes);
        }
        SpriteArtwork page = new ProjectileObjectPages(basePage, projectiles.BeamTiles, projectiles.Trails.Tiles ??
            throw new InvalidDataException("The installed projectile trails have no characters.")).For(beams);

        var frames = new List<SheetFrameSource>();
        foreach (ProjectileSpriteId composition in ProjectileSpriteDefinitions.All)
            frames.Add(new(ProjectileSheetDefinitions.ProjectileFrame(composition), projectiles.Catalog.Frame(composition), page));
        foreach (ProjectileSpriteId composition in ProjectileSheetDefinitions.FlareCompositions)
        {
            ushort selector = checked((ushort)ChargeFlareSpriteDefinitions.Selectors.ToList().IndexOf(composition));
            frames.Add(new(ProjectileSheetDefinitions.FlareFrame(composition), projectiles.FlareCompositions.Frame(selector), page));
        }
        foreach (ProjectileTrailFrame pose in Enum.GetValues<ProjectileTrailFrame>())
            frames.Add(new(ProjectileSheetDefinitions.TrailFrame(pose), ObjPartFrames.SmallObject(projectiles.Trails.Resolve(pose), 0), page));
        frames.Add(new(ProjectileSheetDefinitions.UnshownTrailFrame, ObjPartFrames.SmallObject(default, 0), page));
        return new SpriteSheetSources(frames, cgram.Colors[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection));
    }

    /// <summary>The beam combination whose gameplay sheet <paramref name="sheet"/> is.</summary>
    private static SamusBeamCombination Combination(SpriteSheetId sheet) =>
        ProjectileSheetDefinitions.Combinations.Single(beams => ProjectileSheetDefinitions.For(beams) == sheet);
}
