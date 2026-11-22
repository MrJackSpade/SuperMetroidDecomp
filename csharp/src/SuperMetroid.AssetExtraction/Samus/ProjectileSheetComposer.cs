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
internal static partial class ProjectileSheetComposer
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

    /// <summary>
    /// Each projectile sheet with the context it composes in: each combination's own gameplay sheet, and the intro's
    /// flashbacks, which show the power beam.
    /// </summary>
    private static readonly IReadOnlyDictionary<SpriteSheetId, ProjectileSheetContext> Contexts =
        Combinations.Select(beams => (ProjectileSheetContext)new GameplaySheet(beams))
            .Append(new IntroFlashbackSheet())
            .ToDictionary(context => context.Sheet);

    /// <summary>Every projectile sheet this composer builds.</summary>
    internal static IEnumerable<SpriteSheetId> Sheets => Contexts.Keys;

    /// <summary>The trail poses a projectile sheet holds, in its order, each listed once (#2567).</summary>
    private static readonly IReadOnlyList<ProjectileTrailFrame> TrailFrames = Ensure.EveryMemberOnce(ProjectileSheetDefinitions.TrailFrames);

    /// <summary>The flare compositions a projectile sheet holds: every flare and spark composition.</summary>
    internal static IEnumerable<ProjectileSpriteId> FlareCompositions => ChargeFlareSpriteDefinitions.All;

    /// <summary>
    /// Every frame projectile sheet <paramref name="sheet"/> holds: each composition its beam combination's projectiles
    /// show through <paramref name="bindings"/>, pictured or, when it inks no pixel over the combination's page, held as
    /// showing nothing (#1513), each flare composition and each trail pose. The empty composition an invisible projectile
    /// shows draws nothing, so no sheet holds it, and a trail sparkle that has shown no pose draws nothing (#1625).
    /// </summary>
    internal static IEnumerable<string> Frames(SpriteSheetId sheet, InstalledProjectileFrameBindings bindings) =>
        Compositions(Contexts[sheet].Beams, bindings).Select(ProjectileSheetFrames.ProjectileFrame)
            .Concat(FlareCompositions.Select(ProjectileSheetFrames.FlareFrame))
            .Concat(TrailFrames.Select(ProjectileSheetFrames.TrailFrame))
            .Concat(HitBursts(Contexts[sheet]).Select(ProjectileSheetFrames.HitBurstFrame));

    /// <summary>
    /// The compositions of the burst a hit shows at an enemy it hurts that a sheet composed in <paramref name="context"/>
    /// holds (#1713): every one where its shots hurt enemies, as the burst draws from the sheet of the beams Samus has
    /// equipped, and none in the intro's flashbacks, which hurt no enemy.
    /// </summary>
    private static IEnumerable<ProjectileSpriteId> HitBursts(ProjectileSheetContext context) =>
        context.HurtsEnemies ? HitBurstSpriteDefinitions.All : [];

    /// <summary>
    /// The compositions <paramref name="beams"/>'s projectiles show, in storage order: those of every record the
    /// combination's programs reach, as <paramref name="bindings"/> bind them; an unpictured record draws none.
    /// </summary>
    private static IEnumerable<ProjectileSpriteId> Compositions(SamusBeamCombination beams, InstalledProjectileFrameBindings bindings)
    {
        Ensure.NotNull(bindings);
        HashSet<ProjectileSpriteId> shown = [.. EquippedBeamProjectilePrograms.Records(beams).Select(bindings.Resolve).OfType<ProjectileSpriteId>()];
        return ProjectileSpriteDefinitions.All.Where(shown.Contains);
    }

    /// <summary>What one projectile sheet composes: its named frames over the context's OBJ page, and the sprite rows' colours.</summary>
    /// <param name="inputs">The installed stock artwork the sheet composes from.</param>
    /// <param name="sheet">A projectile sheet.</param>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs, SpriteSheetId sheet)
    {
        Ensure.NotNull(inputs);
        ImportProjectileArtwork artwork = inputs.Projectiles;
        InstalledProjectilePresentation projectiles = artwork.Presentation;
        ProjectileSheetContext context = Contexts[sheet];
        SamusBeamCombination beams = context.Beams;
        RgbColor[] colours = context.Colours(inputs);
        SpriteArtwork page = new ProjectileObjectPages(context.BasePage(inputs), artwork.BeamTiles, artwork.TrailTiles).For(beams);

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
        // A trail sparkle's picture is placed by its centre, where the sparkle is (#2573).
        foreach (ProjectileTrailFrame pose in TrailFrames)
            frames.Add(new(ProjectileSheetFrames.TrailFrame(pose), SpritePartFrames.CentredSmallObject(artwork.Compositions.Trails.Resolve(pose), 0), page));
        // The burst is one of Samus's weapon effects: it is pictured over the combination's page in her beams' colours, and
        // shows her beams' current colouring through the sheet's shades as her other beam pictures do.
        foreach (ProjectileSpriteId composition in HitBursts(context))
            frames.Add(new(ProjectileSheetFrames.HitBurstFrame(composition), artwork.Compositions.HitBursts.Frame(composition), page));
        return new SpriteSheetSources(frames, colours, ObjectSelectionCodec.GameplaySizes, Shades: inputs.Shades, Blank: blank);
    }

    /// <summary>The context a projectile sheet composes in: the beams its projectiles show, its OBJ page and its colours.</summary>
    /// <param name="Beams">The beam combination whose projectiles the sheet shows.</param>
    private abstract partial record ProjectileSheetContext(SamusBeamCombination Beams)
    {
        /// <summary>The sheet composed in this context.</summary>
        internal abstract SpriteSheetId Sheet { get; }

        /// <summary>Whether the context's shots hurt enemies, so its sheet holds the burst a hit shows (#1713).</summary>
        internal abstract bool HurtsEnemies { get; }

        /// <summary>The OBJ page the context's beam characters are bound over.</summary>
        internal abstract SpriteArtwork BasePage(SamusSheetInputs inputs);

        /// <summary>The sprite rows' colours as the context leaves them.</summary>
        internal abstract RgbColor[] Colours(SamusSheetInputs inputs);
    }

    /// <summary>A combination's gameplay sheet: the standard OBJ page, coloured by a room load and the combination's beam palette.</summary>
    private sealed partial record GameplaySheet(SamusBeamCombination Beams) : ProjectileSheetContext(Beams)
    {
        /// <inheritdoc/>
        internal override SpriteSheetId Sheet => ProjectileSheetFrames.For(Beams);

        /// <inheritdoc/>
        internal override bool HurtsEnemies => true;

        /// <inheritdoc/>
        internal override SpriteArtwork BasePage(SamusSheetInputs inputs) => inputs.StandardObjects;

        /// <inheritdoc/>
        internal override RgbColor[] Colours(SamusSheetInputs inputs) => inputs.WithBeams(inputs.RoomLoadSpriteColours(), Beams);
    }

    /// <summary>The intro's flashbacks: the power beam over the intro's OBJ page, in the intro's own palette.</summary>
    private sealed partial record IntroFlashbackSheet() : ProjectileSheetContext(SamusBeamCombination.Power)
    {
        /// <inheritdoc/>
        internal override SpriteSheetId Sheet => SpriteSheetId.IntroProjectiles;

        /// <inheritdoc/>
        internal override bool HurtsEnemies => false;

        /// <inheritdoc/>
        internal override SpriteArtwork BasePage(SamusSheetInputs inputs) => IntroObjectPage.Of(inputs.Intro);

        /// <summary>The intro's colours: it loads the beam palette, then its whole palette over it.</summary>
        internal override RgbColor[] Colours(SamusSheetInputs inputs) => [.. inputs.Intro.Palette.SpriteColours];
    }
}
