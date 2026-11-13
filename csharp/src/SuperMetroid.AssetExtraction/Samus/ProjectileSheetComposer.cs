using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes Samus's projectile sheets (#1392) from the installed stock artwork: each projectile and bomb composition the
/// context's beam combination shows (#1513), each flare composition, trail pose and the unshown trail, over the OBJ page the context's beam combination binds, coloured from
/// the sprite rows as that context leaves them: a room load and the combination's beam palette in gameplay, the intro's
/// own palette in its flashbacks.
/// </summary>
internal static class ProjectileSheetComposer
{
    /// <summary>
    /// The combinations with a sheet: every one with beam artwork of its own. Spazer with Plasma, with or without Ice
    /// and Wave, has none; its projectiles show the sheet of the shot its uncharged beam fires
    /// (<see cref="ProjectileSheetDefinitions.Sheets"/>, #1586).
    /// </summary>
    internal static IReadOnlyList<SamusBeamCombination> Combinations { get; } =
    [
        SamusBeamCombination.Power, SamusBeamCombination.Wave, SamusBeamCombination.Ice, SamusBeamCombination.IceWave,
        SamusBeamCombination.Spazer, SamusBeamCombination.SpazerWave, SamusBeamCombination.SpazerIce, SamusBeamCombination.SpazerIceWave,
        SamusBeamCombination.Plasma, SamusBeamCombination.PlasmaWave, SamusBeamCombination.PlasmaIce, SamusBeamCombination.PlasmaIceWave,
        SamusBeamCombination.SpazerPlasmaWave, SamusBeamCombination.SpazerPlasmaIce,
    ];

    /// <summary>The flare compositions a projectile sheet holds: those the flare's selectors name.</summary>
    internal static IEnumerable<ProjectileSpriteId> FlareCompositions => ChargeFlareSpriteSelection.Selectors.Distinct();

    /// <summary>
    /// Every frame projectile sheet <paramref name="sheet"/> holds: each composition its beam combination's projectiles
    /// show through <paramref name="bindings"/>, pictured or, when it inks no pixel over the combination's page, held as
    /// showing nothing (#1513), each flare composition, each trail pose and the unshown trail. The empty composition an
    /// invisible projectile shows draws nothing, so no sheet holds it.
    /// </summary>
    internal static IEnumerable<string> Frames(SpriteSheetId sheet, InstalledProjectileFrameBindings bindings) =>
        Compositions(Combination(sheet), bindings).Select(ProjectileSheetFrames.ProjectileFrame)
            .Concat(FlareCompositions.Select(ProjectileSheetFrames.FlareFrame))
            .Concat(Enum.GetValues<ProjectileTrailFrame>().Select(ProjectileSheetFrames.TrailFrame))
            .Append(ProjectileSheetDefinitions.UnshownTrailFrame);

    /// <summary>
    /// The compositions <paramref name="beams"/>'s projectiles show, in storage order: those of every record the
    /// combination's programs reach, as <paramref name="bindings"/> bind them, the empty composition excepted.
    /// </summary>
    private static IEnumerable<ProjectileSpriteId> Compositions(SamusBeamCombination beams, InstalledProjectileFrameBindings bindings)
    {
        Ensure.NotNull(bindings);
        HashSet<ProjectileSpriteId> shown = [.. EquippedBeamProjectilePrograms.Records(beams).Select(bindings.Resolve)];
        return ProjectileSpriteDefinitions.All.Where(composition => !composition.DrawsNothing && shown.Contains(composition));
    }

    /// <summary>What one projectile sheet composes: its named frames over the context's OBJ page, and the sprite rows' colours.</summary>
    /// <param name="inputs">The installed stock artwork the sheet composes from.</param>
    /// <param name="sheet">A projectile sheet.</param>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs, SpriteSheetId sheet)
    {
        Ensure.NotNull(inputs);
        ImportProjectileArtwork artwork = inputs.Projectiles;
        InstalledProjectilePresentation projectiles = artwork.Presentation;
        RgbColor[] colours;
        SpriteArtwork basePage;
        SamusBeamCombination beams;
        if (sheet == SpriteSheetId.IntroProjectiles)
        {
            ImportIntroCinematicArtwork intro = inputs.Intro;
            basePage = IntroObjectPage.Of(intro);
            beams = SamusBeamCombination.Power;
            // The intro loads the beam palette, then its whole palette over it.
            colours = [.. intro.Palette.SpriteColours];
        }
        else
        {
            basePage = inputs.StandardObjects;
            beams = Combination(sheet);
            colours = inputs.WithBeams(inputs.RoomLoadSpriteColours(), beams);
        }
        SpriteArtwork page = new ProjectileObjectPages(basePage, artwork.BeamTiles, artwork.TrailTiles).For(beams);

        var frames = new List<SheetFrameSource>();
        var blank = new List<string>();
        foreach (ProjectileSpriteId composition in Compositions(beams, projectiles.FrameBindings))
        {
            // A composition inking no pixel over the combination's page shows nothing: it has no picture, and the
            // projectile showing it draws nothing (#1513). The glitched combinations' pages leave their shots' compositions
            // without a character that inks anything.
            var frame = new SheetFrameSource(ProjectileSheetFrames.ProjectileFrame(composition), projectiles.Catalog.Frame(composition), page);
            if (frame.ShowsAnything())
                frames.Add(frame);
            else
                blank.Add(frame.Name);
        }
        foreach (ProjectileSpriteId composition in FlareCompositions)
        {
            int selector = checked((ushort)ChargeFlareSpriteSelection.Selectors.ToList().IndexOf(composition));
            frames.Add(new(ProjectileSheetFrames.FlareFrame(composition), projectiles.FlareCompositions.Frame(selector), page));
        }
        foreach (ProjectileTrailFrame pose in Enum.GetValues<ProjectileTrailFrame>())
            frames.Add(new(ProjectileSheetFrames.TrailFrame(pose), SpritePartFrames.SmallObject(projectiles.Trails.Resolve(pose), 0), page));
        frames.Add(new(ProjectileSheetDefinitions.UnshownTrailFrame, SpritePartFrames.SmallObject(default, 0), page));
        return new SpriteSheetSources(frames, colours, ObjectSelectionCodec.GameplaySizes, Shades: inputs.Shades, Blank: blank);
    }

    /// <summary>
    /// The beam combination whose projectiles <paramref name="sheet"/> shows: the power beam for the intro's flashbacks,
    /// otherwise the combination whose gameplay sheet it is.
    /// </summary>
    private static SamusBeamCombination Combination(SpriteSheetId sheet) => sheet == SpriteSheetId.IntroProjectiles
        ? SamusBeamCombination.Power
        : Combinations.Single(beams => ProjectileSheetFrames.For(beams) == sheet);
}
