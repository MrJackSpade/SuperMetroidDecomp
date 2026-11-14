using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Intro.Artwork;

/// <summary>
/// Holds the opening's and the Ceres cinematics' installed sprite sheets to the rule that a sheet frame is one complete
/// picture in the colours it is drawn in (#1513), read from Core's actor definitions rather than from the import:
/// <list type="bullet">
/// <item><b>Placeholder colours.</b> Every pixel a frame shows inks the sprite set of the colouring its scene's actors draw
/// it in. A pixel inked in another set is painted in a placeholder (the frame colour group's stock colours: the Zebes title's
/// greys on the Ceres sheets, the cross-fade whites on the intro's) that only a runtime shade replaces.</item>
/// <item><b>Frames no actor draws.</b> A Ceres scene's sheet holds only the poses its actors' programs show. A pose the scene
/// never draws is pictured over characters its owner does not draw from: the Zebes planet and its title over the station's
/// page, before the reveal loads theirs.</item>
/// </list>
/// The colouring each actor draws in is the one its Core definition or draw site names: the Ceres actors' definitions,
/// the blasts' <see cref="CeresShades"/> draw, and for the opening each family's draw site, which names one colouring.
/// </summary>
internal static class CinematicSheetColouringsVerification
{
    /// <summary>The colours of one sprite set.</summary>
    private const int SetColours = 16;

    /// <summary>
    /// The colouring the opening draws each of its families' poses in: the discovery's egg, baby, egg effects and Rinkas in
    /// the discovery actors', the laboratory babies, the narration caret and the Japanese text's next-page arrow in the cross-fade objects', Mother Brain in hers
    /// and her explosions in theirs (<c>IntroCinematicState</c>'s draws, the baby definitions and the explosion system).
    /// </summary>
    private static readonly Dictionary<CinematicSpriteFamily, IntroSpriteColouring> IntroFamilies = new()
    {
        [CinematicSpriteFamily.IntroDiscoveryActor] = IntroSpriteColouring.DiscoveryActors,
        [CinematicSpriteFamily.IntroEggEffect] = IntroSpriteColouring.DiscoveryActors,
        [CinematicSpriteFamily.IntroRinka] = IntroSpriteColouring.DiscoveryActors,
        [CinematicSpriteFamily.IntroScientist] = IntroSpriteColouring.CrossFadeObjects,
        [CinematicSpriteFamily.IntroMotherBrain] = IntroSpriteColouring.MotherBrain,
        [CinematicSpriteFamily.IntroMotherBrainExplosion] = IntroSpriteColouring.Explosions,
        [CinematicSpriteFamily.IntroCaret] = IntroSpriteColouring.CrossFadeObjects,
        [CinematicSpriteFamily.IntroNextPageArrow] = IntroSpriteColouring.CrossFadeObjects,
    };

    /// <summary>Fails, naming each Ceres sheet frame no actor of its scene draws.</summary>
    internal static void VerifyCeresSheetsHoldDrawnPoses()
    {
        var findings = new List<string>();
        int frames = 0;
        foreach ((SpriteSheetId id, Func<IEnumerable<CeresDrawnProgram>> actors) in CeresSheets)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
            Dictionary<CinematicSpritePose, HashSet<int>> drawn = CeresPoseSets(actors());
            foreach (string name in sheet.frames.Keys.Order(StringComparer.Ordinal))
            {
                frames++;
                if (!drawn.ContainsKey(PoseOf(name)))
                    findings.Add($"{id} frame {name} is drawn by no actor of its scene");
            }
        }
        AssertTrue(findings.Count == 0,
            $"{findings.Count} Ceres sheet frames are drawn by no actor:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Ceres sheets: each of {frames} frames is a pose its scene's actors draw.");
    }

    /// <summary>Fails, naming each opening or Ceres sheet frame with a pixel outside the sprite sets of its colourings.</summary>
    internal static void VerifyCinematicSheetFrameColourings()
    {
        var findings = new List<string>();
        int frames = 0;
        foreach (SpriteSheetId id in CeresSheets.Keys.Append(SpriteSheetId.IntroSprites))
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
            Dictionary<CinematicSpritePose, HashSet<int>> ceres = CeresSheets.TryGetValue(id, out Func<IEnumerable<CeresDrawnProgram>>? actors)
                ? CeresPoseSets(actors()) : new();
            RgbaPixel[] pixels = sheet.Image.Pixels.ToArray();
            foreach ((string name, SpriteSheetFrame frame) in sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                frames++;
                HashSet<int> sets = id == SpriteSheetId.IntroSprites ? IntroSets(name) : CeresSets(ceres, name);
                SortedSet<int> foreign = ForeignSets(pixels, sheet.Image, frame, sets);
                if (foreign.Count > 0)
                {
                    findings.Add($"{id} frame {name} paints sprite sets {string.Join(", ", foreign)}, not its colourings' "
                        + $"{(sets.Count == 0 ? "(none)" : string.Join(", ", sets.Order()))}");
                }
            }
        }
        AssertTrue(findings.Count == 0,
            $"{findings.Count} cinematic sheet frames are painted outside their colourings:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Cinematic sheets: each of {frames} frames shows the colours its actors draw it in.");
    }

    /// <summary>The sprite set the opening draws its frame <paramref name="name"/> in: its family's colouring's.</summary>
    private static HashSet<int> IntroSets(string name) => [IntroSheetInks.SpriteSet(IntroFamilies[FamilyOf(name)])];

    /// <summary>The sprite sets a Ceres scene draws its frame <paramref name="name"/> in, of <paramref name="poses"/>; none for a pose it never draws.</summary>
    private static HashSet<int> CeresSets(Dictionary<CinematicSpritePose, HashSet<int>> poses, string name) =>
        poses.TryGetValue(PoseOf(name), out HashSet<int>? drawn) ? drawn : [];

    /// <summary>The Ceres scenes' sheets, each with the actors its scene draws from it.</summary>
    private static readonly Dictionary<SpriteSheetId, Func<IEnumerable<CeresDrawnProgram>>> CeresSheets = new()
    {
        [SpriteSheetId.CeresFlightSprites] = FlightActors,
        [SpriteSheetId.CeresDestructionSprites] = DestructionActors,
        [SpriteSheetId.ZebesRevealSprites] = ZebesRevealActors,
    };

    /// <summary>
    /// The sprite sets each pose a Ceres scene shows is drawn in, by pose: the sets of the colourings of every actor of
    /// <paramref name="actors"/> whose program shows it.
    /// </summary>
    private static Dictionary<CinematicSpritePose, HashSet<int>> CeresPoseSets(IEnumerable<CeresDrawnProgram> actors)
    {
        var sets = new Dictionary<CinematicSpritePose, HashSet<int>>();
        foreach (CeresDrawnProgram actor in actors)
        {
            foreach (CinematicSpritePose pose in CinematicSpriteAnimations.Segments.FramesOf(CinematicSpriteAnimations.StartingWith(actor.Program)))
            {
                if (pose == CinematicSpritePose.Blank)
                    continue;
                if (!sets.TryGetValue(pose, out HashSet<int>? poseSets))
                {
                    poseSets = [];
                    sets.Add(pose, poseSets);
                }
                poseSets.Add(CeresSheetInks.SpriteSet(actor.Colouring));
            }
        }
        return sets;
    }

    /// <summary>The Ceres flight's actors: the front stars and every rear-view actor, each in its definition's colouring.</summary>
    private static IEnumerable<CeresDrawnProgram> FlightActors()
    {
        yield return new(CeresFlightActorDefinitions.FrontStars.Animation, CeresFlightActorDefinitions.FrontStars.Colouring);
        for (int index = 0; index < CeresFlightActorDefinitions.RearViewActorCount; index++)
        {
            CeresFlightActorDefinition actor = CeresFlightActors.RearViewActor(index);
            yield return new(actor.Animation, actor.Colouring);
        }
    }

    /// <summary>
    /// The Ceres destruction's actors before the Zebes reveal: its three initial actors in their definitions' colourings and
    /// every blast in the explosions' colouring, which the destruction's blast spawns name.
    /// </summary>
    private static IEnumerable<CeresDrawnProgram> DestructionActors()
    {
        for (int index = 0; index < CeresDestructionActorDefinitions.InitialActorCount; index++)
        {
            CeresDestructionActorDefinition actor = CeresDestructionActors.InitialActor(index);
            yield return new(actor.Animation, actor.Colouring);
        }
        yield return new(CeresExplosionDefinitions.InitialActor.Animation, CeresSpriteColouring.Explosions);
        yield return new(CeresExplosionDefinitions.RepeatingActor.Animation, CeresSpriteColouring.Explosions);
        yield return new(CeresExplosionDefinitions.FinalWaveActor.Animation, CeresSpriteColouring.Explosions);
        yield return new(CeresExplosionDefinitions.StationBlastActor.Animation, CeresSpriteColouring.Explosions);
    }

    /// <summary>The Zebes reveal's six actors, each in its definition's colouring.</summary>
    private static IEnumerable<CeresDrawnProgram> ZebesRevealActors()
    {
        for (int index = 0; index < CeresDestructionActorDefinitions.ZebesActorCount; index++)
        {
            CeresDestructionActorDefinition actor = CeresDestructionActors.ZebesActor(index);
            yield return new(actor.Animation, actor.Colouring);
        }
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

    /// <summary>The family a cinematic sheet frame's name gives: the part before its pose index.</summary>
    /// <exception cref="InvalidDataException">The name is not a cinematic frame name.</exception>
    private static CinematicSpriteFamily FamilyOf(string name)
    {
        int dot = name.LastIndexOf('.');
        return dot > 0 && Enum.TryParse(name.AsSpan(0, dot), out CinematicSpriteFamily family) ? family
            : throw new InvalidDataException($"Sheet frame {name} is not a cinematic family's frame.");
    }

    /// <summary>The pose index a cinematic sheet frame's name gives: the part after its family.</summary>
    /// <exception cref="InvalidDataException">The name is not a cinematic frame name.</exception>
    private static CinematicSpritePose PoseOf(string name)
    {
        int dot = name.LastIndexOf('.');
        return dot > 0 && Enum.TryParse(name.AsSpan(dot + 1), ignoreCase: false, out CinematicSpritePose pose) && Enum.IsDefined(pose)
            && !char.IsAsciiDigit(name[dot + 1]) ? pose
            : throw new InvalidDataException($"Sheet frame {name} is not a cinematic family's frame.");
    }

    /// <summary>Runs the opening's and Ceres cinematics' sheet colouring rules.</summary>
    [VerificationCommand("--cinematic-sheet-colourings")]
    internal static int CinematicSheetColouringsCommand()
    {
        Suite(nameof(VerifyCeresSheetsHoldDrawnPoses), VerifyCeresSheetsHoldDrawnPoses);
        Suite(nameof(VerifyCinematicSheetFrameColourings), VerifyCinematicSheetFrameColourings);
        return 0;
    }
}
