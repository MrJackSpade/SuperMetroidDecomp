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
/// context's beam combination shows (#1513), each flare composition, trail pose and the unshown trail, and in gameplay the
/// burst her hit shows at an enemy (#1713), over the OBJ page the context's beam combination binds, coloured from
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

    /// <summary>The flare compositions a projectile sheet holds: every flare and spark composition.</summary>
    internal static IEnumerable<ProjectileSpriteId> FlareCompositions => ChargeFlareSpriteDefinitions.All;

    /// <summary>
    /// Every frame projectile sheet <paramref name="sheet"/> holds: each composition its beam combination's projectiles
    /// show through <paramref name="bindings"/>, pictured or, when it inks no pixel over the combination's page, held as
    /// showing nothing (#1513), each flare composition and each trail pose. The empty composition an invisible projectile
    /// shows draws nothing, so no sheet holds it, and a trail sparkle that has shown no pose draws nothing (#1625).
    /// </summary>
    internal static IEnumerable<string> Frames(SpriteSheetId sheet, InstalledProjectileFrameBindings bindings) =>
        Compositions(Combination(sheet), bindings).Select(ProjectileSheetFrames.ProjectileFrame)
            .Concat(FlareCompositions.Select(ProjectileSheetFrames.FlareFrame))
            .Concat(Enum.GetValues<ProjectileTrailFrame>().Select(ProjectileSheetFrames.TrailFrame))
            .Concat(HitBursts(sheet).Select(ProjectileSheetFrames.HitBurstFrame));

    /// <summary>
    /// The compositions of the burst a hit shows at an enemy it hurts that <paramref name="sheet"/> holds (#1713): every one on
    /// each gameplay sheet, as the burst draws from the sheet of the beams Samus has equipped, and none on the intro's, whose
    /// flashbacks hurt no enemy.
    /// </summary>
    private static IEnumerable<ProjectileSpriteId> HitBursts(SpriteSheetId sheet) =>
        sheet == SpriteSheetId.IntroProjectiles ? [] : HitBurstSpriteDefinitions.All;

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
            var frame = new SheetFrameSource(ProjectileSheetFrames.ProjectileFrame(composition), artwork.Compositions.Sprites.Frame(composition), page);
            if (frame.ShowsAnything())
                frames.Add(frame);
            else
                blank.Add(frame.Name);
        }
        foreach (ProjectileSpriteId composition in FlareCompositions)
        {
            frames.Add(new(ProjectileSheetFrames.FlareFrame(composition), artwork.Compositions.Flares.Frame(composition), page));
        }
        foreach (ProjectileTrailFrame pose in Enum.GetValues<ProjectileTrailFrame>())
            frames.Add(new(ProjectileSheetFrames.TrailFrame(pose), SpritePartFrames.SmallObject(artwork.Compositions.Trails.Resolve(pose), 0), page));
        // The burst is one of Samus's weapon effects: it is pictured over the combination's page in her beams' colours, and
        // shows her beams' current colouring through the sheet's shades as her other beam pictures do.
        foreach (ProjectileSpriteId composition in HitBursts(sheet))
            frames.Add(new(ProjectileSheetFrames.HitBurstFrame(composition), artwork.Compositions.HitBursts.Frame(composition), page));
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
