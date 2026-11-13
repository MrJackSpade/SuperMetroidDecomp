using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend;

/// <summary>
/// Holds the title's, the map sprites' and the ending's installed sprite sheets to the rule that a sheet frame is one
/// complete picture in the colours it is drawn in (#1513), read from Core's definitions and draw sites rather than from
/// the import:
/// <list type="bullet">
/// <item><b>Placeholder colours.</b> Every pixel a frame shows inks the sprite set of a colouring the game draws the frame
/// in. A pixel inked in another set is painted in a placeholder (the frame colour group's stock colours) that only a
/// runtime shade replaces.</item>
/// <item><b>Frames no actor draws.</b> An ending scene's sheet holds only the poses its actors draw from it. A pose the
/// scene never draws from that sheet is composed over characters its scene does not load for it, and shows with chunks
/// missing: the reward sheets held every reward pose of every reward, and the falling Samus composed over the reward's
/// characters.</item>
/// </list>
/// </summary>
internal static class FrontendSheetColouringsVerification
{
    /// <summary>The colours of one sprite set.</summary>
    private const int SetColours = 16;

    /// <summary>
    /// The colourings each map sprite is drawn in, as Core's draw sites name them: the scroll arrows in the highlight
    /// cycle's colouring (<c>MapHighlightAnimation</c>); a boss's marker live and under a defeated boss's cross; the station
    /// icons, gunship, cross and position indicator as markers (<c>FileSelectMapIcons</c>, <c>FileSelectStationMarker</c>,
    /// <c>PauseMenuState</c>); the elevator labels and the world map's title as labels; and each world-map area label
    /// lit when selected and dim otherwise (<c>FileSelectAreaMapGraphics</c>).
    /// </summary>
    private static readonly Dictionary<MapSpriteId, MapSpriteColouring[]> MapSpriteDraws = new()
    {
        [MapSpriteId.ArrowRight] = [MapSpriteColouring.Highlighted],
        [MapSpriteId.ArrowLeft] = [MapSpriteColouring.Highlighted],
        [MapSpriteId.ArrowUp] = [MapSpriteColouring.Highlighted],
        [MapSpriteId.ArrowDown] = [MapSpriteColouring.Highlighted],
        [MapSpriteId.MarkerBoss] = [MapSpriteColouring.Markers, MapSpriteColouring.DefeatedBossMarker],
        [MapSpriteId.StationEnergy] = [MapSpriteColouring.Markers],
        [MapSpriteId.StationMissile] = [MapSpriteColouring.Markers],
        [MapSpriteId.StationMap] = [MapSpriteColouring.Markers],
        [MapSpriteId.MarkerDefeatedBoss] = [MapSpriteColouring.Markers],
        [MapSpriteId.MarkerGunship] = [MapSpriteColouring.Markers],
        [MapSpriteId.IndicatorBacking] = [MapSpriteColouring.Markers],
        [MapSpriteId.IndicatorFrame0] = [MapSpriteColouring.Markers],
        [MapSpriteId.IndicatorFrame1] = [MapSpriteColouring.Markers],
        [MapSpriteId.IndicatorFrame2] = [MapSpriteColouring.Markers],
        [MapSpriteId.ElevatorCrateria] = [MapSpriteColouring.Labels],
        [MapSpriteId.ElevatorBrinstar] = [MapSpriteColouring.Labels],
        [MapSpriteId.ElevatorNorfair] = [MapSpriteColouring.Labels],
        [MapSpriteId.ElevatorWreckedShip] = [MapSpriteColouring.Labels],
        [MapSpriteId.ElevatorMaridia] = [MapSpriteColouring.Labels],
        [MapSpriteId.WorldTitle] = [MapSpriteColouring.Labels],
        [MapSpriteId.WorldCrateria] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldBrinstar] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldNorfair] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldWreckedShip] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldMaridia] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldTourian] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
    };

    /// <summary>The ending's sprite sheets.</summary>
    private static readonly SpriteSheetId[] EndingSheets =
    [
        SpriteSheetId.EndingCloudSprites, SpriteSheetId.EndingExplosionSprites, SpriteSheetId.EndingSuitedRewardSprites,
        SpriteSheetId.EndingSuitlessRewardSprites, SpriteSheetId.EndingShootingSprites,
    ];

    /// <summary>Fails, naming each title, map sprite or ending sheet frame with a pixel outside the sprite sets of its colourings.</summary>
    internal static void VerifyFrontendSheetFrameColourings()
    {
        var findings = new List<string>();
        int frames = 0;
        frames += CheckSheet(SpriteSheetId.Title, TitleFrameSets(), findings);
        frames += CheckSheet(SpriteSheetId.MapSprites, MapFrameSets(), findings);
        foreach (SpriteSheetId sheet in EndingSheets)
            frames += CheckSheet(sheet, EndingFrameSets(sheet), findings);
        AssertTrue(findings.Count == 0,
            $"{findings.Count} frontend sheet frames are painted outside their colourings:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Frontend sheets: each of {frames} title, map sprite and ending frames shows the colours it is drawn in.");
    }

    /// <summary>Fails, naming each ending sheet frame its scene's actors never draw from that sheet.</summary>
    internal static void VerifyEndingSheetsHoldDrawnFrames()
    {
        var findings = new List<string>();
        int frames = 0;
        foreach (SpriteSheetId id in EndingSheets)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
            Dictionary<string, HashSet<int>> drawn = EndingFrameSets(id);
            foreach (string name in sheet.frames.Keys.Concat(sheet.blanks).Order(StringComparer.Ordinal))
            {
                frames++;
                if (!drawn.ContainsKey(name))
                    findings.Add($"{id} frame {name} is drawn by no actor of its scene from this sheet");
            }
        }
        AssertTrue(findings.Count == 0,
            $"{findings.Count} ending sheet frames are drawn by no actor:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Ending sheets: each of {frames} frames is one its scene's actors draw from its sheet.");
    }

    /// <summary>Checks every frame of sheet <paramref name="id"/> against the sets <paramref name="expected"/> names for it, returning how many it checked.</summary>
    private static int CheckSheet(SpriteSheetId id, Dictionary<string, HashSet<int>> expected, List<string> findings)
    {
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
        RgbaPixel[] pixels = sheet.Image.Pixels.ToArray();
        int frames = 0;
        foreach ((string name, SpriteSheetFrame frame) in sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            frames++;
            HashSet<int> sets = expected.TryGetValue(name, out HashSet<int>? drawn) ? drawn : [];
            SortedSet<int> foreign = ForeignSets(pixels, sheet.Image, frame, sets);
            if (foreign.Count > 0)
            {
                findings.Add($"{id} frame {name} paints sprite sets {string.Join(", ", foreign)}, not its colourings' "
                    + $"{(sets.Count == 0 ? "(none)" : string.Join(", ", sets.Order()))}");
            }
        }
        return frames;
    }

    /// <summary>
    /// The sprite sets the title draws each frame in: each card's reveals in its card's colouring, the logo in the logo's
    /// and the copyright in the copyright's (<c>TitleSequenceState</c>'s draws).
    /// </summary>
    private static Dictionary<string, HashSet<int>> TitleFrameSets()
    {
        var sets = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
        foreach (TitleTextSequenceDefinition card in (TitleTextSequenceDefinition[])[TitleSequenceDefinitions.TextSequences.Year,
            TitleSequenceDefinitions.TextSequences.Nintendo, TitleSequenceDefinitions.TextSequences.Presents,
            TitleSequenceDefinitions.TextSequences.MetroidThree])
        {
            // Each card's reveals are drawn as the glyphs typed so far, each glyph in the card's colouring.
            int reveals = TitleCards.Steps(card.Phase).Where(step => step.Sprite is not null).Select(step => step.Sprite).Distinct().Count();
            for (int glyph = 0; glyph < reveals; glyph++)
                Add(sets, TitleSpriteDefinitions.GlyphFrameName(card.Phase, glyph), TitleSheetInks.SpriteSet(card.Colouring));
        }
        Add(sets, TitleSpriteDefinitions.FrameName(TitleSprite.SuperMetroidLogo), TitleSheetInks.SpriteSet(TitleSpriteColouring.Logo));
        Add(sets, TitleSpriteDefinitions.FrameName(TitleSprite.NintendoCopyright), TitleSheetInks.SpriteSet(TitleSpriteColouring.Copyright));
        return sets;
    }

    /// <summary>The sprite sets each map sprite frame is drawn in: those of the colourings its draw sites name.</summary>
    private static Dictionary<string, HashSet<int>> MapFrameSets()
    {
        var sets = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
        foreach ((MapSpriteId id, MapSpriteColouring[] colourings) in MapSpriteDraws)
        {
            foreach (MapSpriteColouring colouring in colourings)
                Add(sets, MapSpriteNames.Name(id), MapSpriteColouringSets.SpriteSet(colouring));
        }
        return sets;
    }

    /// <summary>
    /// The sprite sets each frame ending sheet <paramref name="sheet"/> is drawn in, by frame name, from Core's actor
    /// definitions: every pose each actor of the sheet's scene shows while its scene draws from that sheet, in the
    /// colouring the actor draws it in, and on the post-credits sheets each shooting star frame in the stars' colouring.
    /// </summary>
    private static Dictionary<string, HashSet<int>> EndingFrameSets(SpriteSheetId sheet)
    {
        var sets = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
        EndingSheetActors[sheet](sets);
        return sets;
    }

    /// <summary>Each ending sheet's recording of the frames its scene's actors draw from it.</summary>
    private static readonly Dictionary<SpriteSheetId, Action<Dictionary<string, HashSet<int>>>> EndingSheetActors = new()
    {
        [SpriteSheetId.EndingCloudSprites] = CloudActors,
        [SpriteSheetId.EndingExplosionSprites] = ExplosionActors,
        [SpriteSheetId.EndingSuitedRewardSprites] = SuitedRewardActors,
        [SpriteSheetId.EndingSuitlessRewardSprites] = SuitlessRewardActors,
        [SpriteSheetId.EndingShootingSprites] = ShootingActors,
    };

    /// <summary>The escapes' clouds, each in its definition's colouring.</summary>
    private static void CloudActors(Dictionary<string, HashSet<int>> sets)
    {
        foreach (EndingSpriteDefinition cloud in (EndingSpriteDefinition[])[EndingActorDefinitions.EscapeACloudRightTop,
            EndingActorDefinitions.EscapeACloudLeftTop, EndingActorDefinitions.EscapeACloudRightBottom,
            EndingActorDefinitions.EscapeACloudLeftBottom, EndingActorDefinitions.EscapeBCloudTopA,
            EndingActorDefinitions.EscapeBCloudTopB, EndingActorDefinitions.EscapeBCloudBottomA, EndingActorDefinitions.EscapeBCloudBottomB])
            AddProgram(sets, CinematicSpriteFamily.EndingCloud, cloud.Animation, cloud.Colouring);
    }

    /// <summary>The explosion's actors, the completion text, the clear time and the animals' escape pod.</summary>
    private static void ExplosionActors(Dictionary<string, HashSet<int>> sets)
    {
        foreach (EndingSpriteDefinition explosion in (EndingSpriteDefinition[])[EndingActorDefinitions.ExplodingZebes,
            EndingActorDefinitions.ExplosionLava, EndingActorDefinitions.ExplosionGlow, EndingActorDefinitions.ExplosionStars,
            EndingActorDefinitions.ExplosionSilhouette, EndingActorDefinitions.ExplosionStarsRight,
            EndingActorDefinitions.ExplosionStarsLeft, EndingActorDefinitions.ExplosionAfterglow])
            AddProgram(sets, CinematicSpriteFamily.EndingExplosion, explosion.Animation, explosion.Colouring);
        foreach (EndingSpriteDefinition text in (EndingSpriteDefinition[])[EndingActorDefinitions.OperationWasText,
            EndingActorDefinitions.CompletedSuccessfullyText, EndingActorDefinitions.ClearTimeText, EndingActorDefinitions.ClearTimeColon])
        {
            AddProgram(sets, CinematicSpriteFamily.EndingCompletionText, text.Animation, text.Colouring);
            // A typed text is drawn as the glyphs typed so far, each glyph in the text's colouring; the colon is drawn whole.
            if (text.Animation == EndingActorDefinitions.ClearTimeColon.Animation)
                continue;
            int glyphs = CinematicSpriteAnimations.Segments.Beats(text.Animation)
                .OfType<AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Frame>()
                .Select(frame => frame.Shown).Where(pose => pose != CinematicSpritePose.Blank).Distinct().Count();
            for (int glyph = 0; glyph < glyphs; glyph++)
                Add(sets, TypedTextSpriteSheet.GlyphFrameName(CinematicSpriteFamily.EndingCompletionText, text.Animation, glyph), EndingScreenInks.SpriteSet(text.Colouring));
        }
        foreach (CinematicSpriteSegment digit in EndingCreditsState.ClearTimeDigitAnimations)
            AddProgram(sets, CinematicSpriteFamily.EndingCompletionText, digit, EndingSpriteColouring.ClearTimeDigits);
        // The flyaway spawns the animals' escape pod in its own colouring (EndingFlyaway).
        AddProgram(sets, CinematicSpriteFamily.EndingAnimalEscape, CinematicSpriteSegment.EndingAnimalEscape, EndingSpriteColouring.AnimalEscape);
    }

    /// <summary>The suited rewards' actors and the stars.</summary>
    private static void SuitedRewardActors(Dictionary<string, HashSet<int>> sets)
    {
        // The armored and helmetless rewards: their idle Samus, their gestures and their jump until it falls.
        foreach (EndingSpriteDefinition idle in (EndingSpriteDefinition[])[EndingActorDefinitions.ArmoredRewardBody,
            EndingActorDefinitions.ArmoredRewardHead, EndingActorDefinitions.HelmetlessRewardHead])
            AddProgram(sets, CinematicSpriteFamily.EndingReward, idle.Animation, idle.Colouring);
        AddRewardActors(sets, EndingRewardActor.HelmetedHead, EndingRewardActor.HelmetlessHead, EndingRewardActor.SuitedBody,
            EndingRewardActor.SuitedArm, EndingRewardActor.JumpSuitedBody, EndingRewardActor.JumpHelmetedHead,
            EndingRewardActor.JumpHelmetlessHead);
        AddStars(sets);
    }

    /// <summary>The suitless reward's actors and the stars.</summary>
    private static void SuitlessRewardActors(Dictionary<string, HashSet<int>> sets)
    {
        foreach (EndingSpriteDefinition idle in (EndingSpriteDefinition[])[EndingActorDefinitions.SuitlessRewardBody,
            EndingActorDefinitions.SuitlessRewardLegs])
            AddProgram(sets, CinematicSpriteFamily.EndingReward, idle.Animation, idle.Colouring);
        AddRewardActors(sets, EndingRewardActor.SuitlessUpper, EndingRewardActor.SuitlessLower, EndingRewardActor.JumpSuitlessBody);
        AddStars(sets);
    }

    /// <summary>The falling and shooting Samus and the logo; the stars draw from the reward sheets (#1626).</summary>
    private static void ShootingActors(Dictionary<string, HashSet<int>> sets)
    {
        // The falling body falls and lands as falling Samus, and the shoot cue turns it to shooting Samus
        // (EndingRewardJump); the logo's four actors draw in the logo's colouring (EndingLogo).
        AddProgram(sets, CinematicSpriteFamily.EndingReward, CinematicSpriteSegment.Falling, EndingSpriteColouring.FallingSamus);
        EndingSpriteColouring landing = EndingSpriteColouring.FallingSamus;
        foreach (AnimationBeat<CinematicSpritePose, CinematicSpriteCue> beat in
            CinematicSpriteAnimations.Segments.Beats(CinematicSpriteSegment.Landing))
        {
            if (beat is AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Event { Raised.Kind: CinematicSpriteCueKind.Shoot })
                landing = EndingSpriteColouring.ShootingSamus;
            else if (beat is AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Frame shown
                && shown.Shown != CinematicSpritePose.Blank)
                Add(sets, $"{CinematicSpriteFamily.EndingReward}.{shown.Shown.Index}", EndingScreenInks.SpriteSet(landing));
        }
        for (int actor = 0; actor < EndingLogoDefinitions.ActorKinds.Length; actor++)
            AddProgram(sets, CinematicSpriteFamily.EndingLogo, EndingLogoActors.Actor(actor).Animation, EndingSpriteColouring.Logo);
    }

    /// <summary>Records each reward actor's poses in the colouring its initialization gives it.</summary>
    private static void AddRewardActors(Dictionary<string, HashSet<int>> sets, params EndingRewardActor[] actors)
    {
        foreach (EndingRewardActor actor in actors)
        {
            EndingRewardActorDefinition record = EndingRewardActors.Get(actor);
            AddProgram(sets, CinematicSpriteFamily.EndingReward, record.Animation, EndingRewardActors.GetInitialization(record.Initialization).Colouring);
        }
    }

    /// <summary>Records each shooting star frame in the stars' colouring.</summary>
    private static void AddStars(Dictionary<string, HashSet<int>> sets)
    {
        for (int frame = 1; frame <= EndingShootingStarDefinitions.FullStarFrame; frame++)
            Add(sets, EndingShootingStarFrames.SheetFrameName(frame), EndingScreenInks.SpriteSet(EndingSpriteColouring.ShootingStars));
    }

    /// <summary>Records every pose <paramref name="program"/> shows, through every program it goes or falls into, in <paramref name="colouring"/>.</summary>
    private static void AddProgram(Dictionary<string, HashSet<int>> sets, CinematicSpriteFamily family, CinematicSpriteSegment program,
        EndingSpriteColouring colouring)
    {
        foreach (CinematicSpritePose pose in CinematicSpriteAnimations.Segments.FramesOf(CinematicSpriteAnimations.StartingWith(program)))
        {
            if (pose != CinematicSpritePose.Blank)
                Add(sets, $"{family}.{pose.Index}", EndingScreenInks.SpriteSet(colouring));
        }
    }

    /// <summary>Records that frame <paramref name="name"/> is drawn in sprite set <paramref name="set"/>.</summary>
    private static void Add(Dictionary<string, HashSet<int>> sets, string name, int set)
    {
        if (!sets.TryGetValue(name, out HashSet<int>? frameSets))
        {
            frameSets = [];
            sets.Add(name, frameSets);
        }
        frameSets.Add(set);
    }

    /// <summary>The sprite sets outside <paramref name="sets"/> that <paramref name="frame"/>'s shown pixels ink, in rising order.</summary>
    private static SortedSet<int> ForeignSets(RgbaPixel[] pixels, SpriteImage image, SpriteSheetFrame frame, HashSet<int> sets)
    {
        var foreign = new SortedSet<int>();
        int width = image.Size.X;
        for (int row = 0; row < frame.Size.Y; row++)
        {
            int line = (frame.Corner.Y + row) * width + frame.Corner.X;
            for (int column = 0; column < frame.Size.X; column++)
            {
                if (pixels[line + column].A == 0)
                    continue;
                // The set the import coloured the pixel's colour from (#1544).
                int set = InstalledPictureColoursConstants.ComposedIndexAt(image, line + column) / SetColours;
                if (!sets.Contains(set))
                    foreign.Add(set);
            }
        }
        return foreign;
    }

    /// <summary>Runs the title's, map sprites' and ending's sheet colouring rules.</summary>
    [VerificationCommand("--frontend-sheet-colourings")]
    internal static int FrontendSheetColouringsCommand()
    {
        Suite(nameof(VerifyFrontendSheetFrameColourings), VerifyFrontendSheetFrameColourings);
        Suite(nameof(VerifyEndingSheetsHoldDrawnFrames), VerifyEndingSheetsHoldDrawnFrames);
        return 0;
    }
}
