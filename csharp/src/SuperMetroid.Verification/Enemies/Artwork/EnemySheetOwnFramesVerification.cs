using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Hoppers.Artwork;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.MamaTurtles.Artwork;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Platforms.Artwork;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds every installed enemy sheet of a definition that shares frame tables with other definitions to the rule that a
/// sheet holds only the frames its own enemy can show (#1513). Which frames each definition shows is derived here from
/// Core's per-definition data, independently of the frame lists the sheets were composed from: the programs each
/// definition's code starts (its initial program, its selector tables, its header's variant, its placements' spawn
/// settings), followed through every go-to, fall-through, counted repeat and program-naming cue, and mapped through its
/// family's frame table. A sheet holding a table frame its definition never reaches holds another definition's picture,
/// composed over its own characters.
/// </summary>
internal static class EnemySheetOwnFramesVerification
{
    /// <summary>Checks every installed sheet of every sharing family against the frames its own definition reaches.</summary>
    internal static void VerifyEnemySheetOwnFrames()
    {
        InstalledEnemySheets installed = InstalledFixtures.EnemySheets;
        // Each enemy's installed variants in a stable order, gathered once for every family, and the frames each distinct sheet
        // holds, which the variants sharing it would otherwise spell out again.
        Dictionary<EnemyDefinitionId, List<EnemyArtworkVariant<SpriteSheet>>> sheetsOf = installed.EnemyVariants
            .GroupBy(entry => entry.Key)
            .ToDictionary(group => group.Key, group => group.Select(entry => entry.Value).ToList());
        var heldBy = new Dictionary<SpriteSheet, HashSet<string>>();
        var failures = new List<string>();
        int sheets = 0;
        foreach (EnemySheetOwnFrameFamily family in Families())
        {
            int familySheets = 0;
            foreach ((EnemyDefinitionId enemy, IReadOnlySet<EnemyFrameId> own) in family.Own)
            {
                EnemyFrameId[] foreign = [.. family.Tables.Where(frame => !own.Contains(frame)).Order()];
                foreach (EnemyArtworkVariant<SpriteSheet> variant in sheetsOf.GetValueOrDefault(enemy, []))
                {
                    SpriteSheet sheet = variant.Sheet;
                    familySheets++;
                    if (!heldBy.TryGetValue(sheet, out HashSet<string>? held))
                        heldBy.Add(sheet, held = [.. sheet.frames.Keys.Concat(sheet.blanks).Select(picture => picture.Split('|')[0])]);
                    foreach (EnemyFrameId frame in foreign)
                    {
                        if (held.Contains(EnemySheetDefinitions.FrameName(frame)))
                            failures.Add($"{family.Name}: {variant.Name} ({sheet.Image.Name}) holds {frame}, which no program or state of {enemy} shows");
                    }
                }
            }
            AssertTrue(familySheets > 0, $"the installation holds a sheet of the {family.Name} family");
            sheets += familySheets;
        }
        AssertTrue(failures.Count == 0,
            $"{failures.Count} installed enemy sheet frames belong to another definition:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Enemy sheet own frames: {sheets} installed sheets of definitions sharing frame tables hold only frames their own programs show.");
    }

    /// <summary>Every family of definitions sharing frame tables, with the frames each definition's own programs reach.</summary>
    private static IEnumerable<EnemySheetOwnFrameFamily> Families() =>
    [
        SkreeAndMetaree(), HoppersFamily(), EscapeAnimalsFamily(), FuneAndNamihe(), PlatformsFamily(), EvirFamily(), GunshipFamily(), TurtlesFamily(), RippersFamily(),
        KiHuntersFamily(), WorkRobotsFamily(), TorizosFamily(), RidleysFamily(),
    ];

    /// <summary>The Skree and the Metaree each run the programs their own selector table names.</summary>
    private static EnemySheetOwnFrameFamily SkreeAndMetaree() => new("Skree and Metaree", SkreeMetareeAnimations.Segments.Frames.ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.Skree] = Reach(SkreeMetareeAnimations.Segments, SkreeMetareeAnimations.StartingAt,
                SkreeMetareeAnimationDefinitions.SkreeAnimations.Values),
            [EnemyDefinitionId.Metaree] = Reach(SkreeMetareeAnimations.Segments, SkreeMetareeAnimations.StartingAt,
                SkreeMetareeAnimationDefinitions.MetareeAnimations.Values),
        });

    /// <summary>Each Sidehopper and Dessgeega runs the programs of the variant its header names, on either surface.</summary>
    private static EnemySheetOwnFrameFamily HoppersFamily()
    {
        EnemyDefinitionId[] hoppers =
        [
            EnemyDefinitionId.Sidehopper, EnemyDefinitionId.SidehopperLarge, EnemyDefinitionId.SidehopperTourian,
            EnemyDefinitionId.Dessgeega, EnemyDefinitionId.DessgeegaLarge,
        ];
        return new("Sidehopper and Dessgeega", HopperAnimations.Segments.Frames.ToHashSet(),
            hoppers.ToDictionary<EnemyDefinitionId, EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>(definition => definition, definition => Reach(HopperAnimations.Segments, HopperAnimations.StartingAt,
                HopperAnimationDefinitions.Animations
                    .Where(entry => entry.Key.Kind == HopperAnimationChoices.KindOf(definition))
                    .Select(entry => entry.Value))));
    }

    /// <summary>The escaping Etecoon and Dachora each run only their own program set and frame table.</summary>
    private static EnemySheetOwnFrameFamily EscapeAnimalsFamily() => new("escaping animals",
        EscapeEtecoonAnimations.Segments.Frames.ToHashSet().Union(EscapeDachoraAnimations.Segments.Frames.ToHashSet()).ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.EtecoonEscape] = Reach(EscapeEtecoonAnimations.Segments, EscapeEtecoonAnimations.StartingAt,
                EscapeEtecoonAnimations.StartingAt.Keys),
            [EnemyDefinitionId.DachoraEscape] = Reach(EscapeDachoraAnimations.Segments, EscapeDachoraAnimations.StartingAt,
                EscapeDachoraAnimations.StartingAt.Keys),
        });

    /// <summary>A Fune or Namihe runs the programs its placements' variant selects, idle or active, facing either way.</summary>
    private static EnemySheetOwnFrameFamily FuneAndNamihe() => new("Fune and Namihe", FuneNamiheAnimations.Segments.Frames.ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.Fune] = Reach(FuneNamiheAnimations.Segments, FuneNamiheAnimations.StartingAt, PlacedFuneNamihePrograms(EnemyDefinitionId.Fune)),
            [EnemyDefinitionId.Namihe] = Reach(FuneNamiheAnimations.Segments, FuneNamiheAnimations.StartingAt, PlacedFuneNamihePrograms(EnemyDefinitionId.Namihe)),
        });

    /// <summary>The programs every room placement of <paramref name="definition"/> can select.</summary>
    private static IEnumerable<FuneNamiheSegment> PlacedFuneNamihePrograms(EnemyDefinitionId definition)
    {
        FuneNamiheSpawnSettings[] placements = [.. Placements(definition).Select(placement => placement.SpawnSettingsOf<FuneNamiheSpawnSettings>())];
        AssertTrue(placements.Length > 0, $"a room places {definition}");
        bool[] both = [false, true];
        return placements.SelectMany(settings => both.SelectMany(active => both.Select(right =>
            FuneNamiheBehavior.SelectFuneNamiheAnimation(new FuneNamiheEnemyState { Variant = settings.Variant, ShowsActiveAnimation = active, FacingRight = right }))));
    }

    /// <summary>
    /// Kamer runs the suspensor platform's programs, the Tripper its own, as the platform initializer chooses; only the
    /// Tripper's shot and bomb reactions pin its frozen frames.
    /// </summary>
    private static EnemySheetOwnFrameFamily PlatformsFamily()
    {
        EnemyFrameId[] frozen = [EnemyFrameId.TripperFrozenMovingLeft, EnemyFrameId.TripperFrozenMovingRight];
        return new("Tripper and Kamer", PlatformAnimations.Segments.Frames.ToHashSet().Union(frozen).ToHashSet(),
            new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
            {
                [EnemyDefinitionId.Kamer2] = Reach(PlatformAnimations.Segments, PlatformAnimations.StartingAt,
                    PlatformAnimationDefinitions.Animations.Where(entry => entry.Key.Kamer).Select(entry => entry.Value)),
                [EnemyDefinitionId.Tripper] = With(Reach(PlatformAnimations.Segments, PlatformAnimations.StartingAt,
                    PlatformAnimationDefinitions.Animations.Where(entry => !entry.Key.Kamer).Select(entry => entry.Value)), frozen),
            });
    }

    /// <summary>An Evir placement is its body or arms; the Evir projectile runs only the spit's programs.</summary>
    private static EnemySheetOwnFrameFamily EvirFamily() => new("Evir", EvirAnimations.Segments.Frames.ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.Evir] = Reach(EvirAnimations.Segments, EvirAnimations.StartingAt,
                [EvirSegment.BodyFacingLeft, EvirSegment.BodyFacingRight, EvirSegment.ArmsFacingLeft, EvirSegment.ArmsFacingRight]),
            [EnemyDefinitionId.EvirProjectile] = Reach(EvirAnimations.Segments, EvirAnimations.StartingAt,
                [EvirSegment.ProjectileNormal, EvirSegment.ProjectileRegenerating]),
        });

    /// <summary>The gunship's top is its top hull; its bottom is the bottom hull or the entrance pad, which opens and closes.</summary>
    private static EnemySheetOwnFrameFamily GunshipFamily() => new("gunship", GunshipAnimations.Segments.Frames.ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.ShipTop] = Reach(GunshipAnimations.Segments, GunshipAnimations.StartingAt, [GunshipSegment.TopHull]),
            [EnemyDefinitionId.ShipBottomEntrance] = Reach(GunshipAnimations.Segments, GunshipAnimations.StartingAt,
                [GunshipSegment.BottomHull, GunshipSegment.BottomEntrancePad, GunshipSegment.EntrancePadOpening, GunshipSegment.EntrancePadClosing]),
        });

    /// <summary>The Mama Turtle sleeps, leaves and enters her shell and spins; the baby crawls, hides, leaves its shell and spins.</summary>
    private static EnemySheetOwnFrameFamily TurtlesFamily() => new("turtles", MamaTurtleAnimations.Segments.Frames.ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.MamaTurtle] = Reach(MamaTurtleAnimations.Segments, MamaTurtleAnimations.StartingAt,
            [
                MamaTurtleSegment.MamaAsleep, MamaTurtleSegment.MamaLeaveShellLeft, MamaTurtleSegment.MamaLeaveShellRight,
                MamaTurtleSegment.MamaEnterShellLeft, MamaTurtleSegment.MamaEnterShellRight, MamaTurtleSegment.MamaSpinning,
            ]),
            [EnemyDefinitionId.BabyTurtle] = Reach(MamaTurtleAnimations.Segments, MamaTurtleAnimations.StartingAt,
            [
                MamaTurtleSegment.BabyCrawlingLeft, MamaTurtleSegment.BabyCrawlingRight, MamaTurtleSegment.BabyHidingLeft,
                MamaTurtleSegment.BabyHidingRight, MamaTurtleSegment.BabySpinning, MamaTurtleSegment.BabyLeaveShellLeft,
                MamaTurtleSegment.BabyLeaveShellRight,
            ]),
        });

    /// <summary>
    /// Each Ripper runs its own pair of moving programs; only the GRipper's and Ripper II's shot and bomb reactions pin
    /// their shared frozen frames.
    /// </summary>
    private static EnemySheetOwnFrameFamily RippersFamily()
    {
        EnemyFrameId[] frozen = [EnemyFrameId.RipperSharedFrozenLeft, EnemyFrameId.RipperSharedFrozenRight];
        return new("Ripper", RipperAnimations.Segments.Frames.ToHashSet().Union(frozen).ToHashSet(),
            new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
            {
                [EnemyDefinitionId.GRipper] = With(Reach(RipperAnimations.Segments, RipperAnimations.StartingAt,
                    [RipperSegment.GRipperMovingLeft, RipperSegment.GRipperMovingRight]), frozen),
                [EnemyDefinitionId.Ripper2] = With(Reach(RipperAnimations.Segments, RipperAnimations.StartingAt,
                    [RipperSegment.GRipperMovingLeft, RipperSegment.GRipperMovingRight]), frozen),
                [EnemyDefinitionId.Ripper] = Reach(RipperAnimations.Segments, RipperAnimations.StartingAt,
                    [RipperSegment.RipperMovingLeft, RipperSegment.RipperMovingRight]),
            });
    }

    /// <summary>A Ki-Hunter's body flies, swoops, hops, lands and spits; its wings follow, or fly off detached.</summary>
    private static EnemySheetOwnFrameFamily KiHuntersFamily()
    {
        KiHunterSegment[] body =
        [
            KiHunterSegment.FlyingLeft, KiHunterSegment.FlyingRight, KiHunterSegment.SwoopLeft, KiHunterSegment.SwoopRight,
            KiHunterSegment.JumpLeft, KiHunterSegment.JumpRight, KiHunterSegment.LandLeft, KiHunterSegment.LandRight,
            KiHunterSegment.SpitLeft, KiHunterSegment.SpitRight,
        ];
        KiHunterSegment[] wings = [KiHunterSegment.WingsLeft, KiHunterSegment.WingsRight, KiHunterSegment.DetachedWings];
        return new("Ki-Hunter", KiHunterAnimations.Segments.Frames.ToHashSet(),
            KiHunterPartDefinitions.Parts.Keys.ToDictionary<EnemyDefinitionId, EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>(definition => definition, definition => Reach(KiHunterAnimations.Segments, KiHunterAnimations.StartingAt,
                KiHunterBehavior.IsKiHunterBodyDefinition(definition) ? body
                    : KiHunterBehavior.IsKiHunterWingDefinition(definition) ? wings
                    : throw new InvalidDataException($"{definition} is neither a Ki-Hunter body nor its wings."))));
    }

    /// <summary>
    /// A work robot without power holds one of its unpowered poses; a powered robot starts unpowered until the area boss
    /// falls, then runs every program of its set.
    /// </summary>
    private static EnemySheetOwnFrameFamily WorkRobotsFamily() => new("work robot", WorkRobotAnimations.Segments.Frames.ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.RobotNoPower] = Reach(WorkRobotAnimations.Segments, WorkRobotAnimations.StartingAt,
                [WorkRobotSegment.NoPowerNeutral, WorkRobotSegment.NoPowerLeaningLeft, WorkRobotSegment.NoPowerLeaningRight]),
            [EnemyDefinitionId.Robot] = Reach(WorkRobotAnimations.Segments, WorkRobotAnimations.StartingAt,
                Enum.GetValues<WorkRobotSegment>()),
        });

    /// <summary>
    /// Each Torizo starts in its initial program and the program it resumes after that program's sleep; its AI, shot
    /// reactions and falls start the rest, and every branch and choice its programs name is followed.
    /// </summary>
    private static EnemySheetOwnFrameFamily TorizosFamily()
    {
        TorizoAnimation bomb = TorizoInitializationDefinitions.Bomb.Animation;
        TorizoAnimation golden = TorizoInitializationDefinitions.Golden.Animation;
        TorizoAnimation[] bombStarts =
        [
            bomb, TorizoAnimations.AfterSleep(bomb), TorizoAnimation.BombTorizoGutBlowingUp, TorizoAnimation.BombTorizoFaceBlowingUp,
            TorizoAnimation.FacelessTurningLeft, TorizoAnimation.FacelessTurningRight, TorizoAnimation.TurningLeft,
            TorizoAnimation.TurningRight, TorizoAnimation.FacingLeftFalling, TorizoAnimation.FacingRightFalling,
            TorizoAnimation.Dying,
        ];
        TorizoAnimation[] goldenStarts =
        [
            golden, TorizoAnimations.AfterSleep(golden), TorizoAnimation.GoldenTurningLeft, TorizoAnimation.GoldenTurningRight,
            TorizoAnimation.GoldenDodgeTurningLeft, TorizoAnimation.GoldenDodgeTurningRight, TorizoAnimation.GoldenFacingLeftCatchingSuperMissileLeftLegForward,
            TorizoAnimation.GoldenFacingLeftCatchingSuperMissileRightLegForward, TorizoAnimation.GoldenFacingRightCatchingSuperMissileLeftLegForward,
            TorizoAnimation.GoldenFacingRightCatchingSuperMissileRightLegForward, TorizoAnimation.FacingLeftFalling, TorizoAnimation.FacingRightFalling,
            TorizoAnimation.Dying,
        ];
        return new("Torizo", TorizoAnimations.Segments.Frames.ToHashSet(),
            new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
            {
                [EnemyDefinitionId.BombTorizo] = Reach(TorizoAnimations.Segments, TorizoAnimations.StartingAt, bombStarts, TorizoCueTargets),
                [EnemyDefinitionId.GoldenTorizo] = Reach(TorizoAnimations.Segments, TorizoAnimations.StartingAt, goldenStarts, TorizoCueTargets),
            });
    }

    /// <summary>The body animations a Torizo cue branches, calls or links to.</summary>
    private static IEnumerable<TorizoAnimation> TorizoCueTargets(TorizoCue cue) => cue switch
    {
        TorizoCue.Branch branch => [branch.Target],
        TorizoCue.Choice choice => [choice.Target, choice.Alternate],
        _ => [],
    };

    /// <summary>
    /// Ridley in Lower Norfair stands, roars, lifts off, breathes fire, turns and roars as he dies; Ceres Ridley stands,
    /// roars, lifts off, lunges, takes the Baby and breathes fire. His death breakup runs no body program.
    /// </summary>
    private static EnemySheetOwnFrameFamily RidleysFamily() => new("Ridley", TableFrames(RidleyBodyFrameDefinitions.ShownFrames),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.Ridley] = Reach(RidleyAnimations.Segments, RidleyAnimations.StartingAt, RidleyBodyFrameDefinitions.ShownFrames.Of,
            [
                RidleyBodySegment.Initial, RidleyBodySegment.OpeningRoar, RidleyBodySegment.TransitionToFlying, RidleyBodySegment.Fireballing,
                RidleyBodySegment.TurnFromLeftToRight, RidleyBodySegment.TurnFromRightToLeft, RidleyBodySegment.DeathRoar,
            ], RidleyCueTargets),
            [EnemyDefinitionId.RidleyCeres] = Reach(RidleyAnimations.Segments, RidleyAnimations.StartingAt, RidleyBodyFrameDefinitions.ShownFrames.Of,
            [
                RidleyBodySegment.Initial, RidleyBodySegment.OpeningRoar, RidleyBodySegment.TransitionToFlying, RidleyBodySegment.CeresLunge,
                RidleyBodySegment.RetrieveBabyMetroid, RidleyBodySegment.Fireballing,
            ], RidleyCueTargets),
            [EnemyDefinitionId.RidleyExplosion] = new HashSet<EnemyFrameId>(),
        });

    /// <summary>The body segment whose animation a Ridley cue redirects to.</summary>
    private static IEnumerable<RidleyBodySegment> RidleyCueTargets(RidleyBodyCue cue) => cue switch
    {
        RidleyBodyCue.GoToUnlessFacingLeft redirect => [redirect.Target],
        RidleyBodyCue.GoToIfCeresAndSamusLowEnergy redirect => [redirect.Target],
        _ => [],
    };

    /// <summary>Every room placement of <paramref name="definition"/>.</summary>
    private static IEnumerable<RoomEnemyPlacement> Placements(EnemyDefinitionId definition) =>
        RoomEnemyPopulationDefinitions.Definitions.SelectMany(list => list.Placements.ToArray()).Where(placement => placement.DefinitionId == definition);

    /// <summary><paramref name="reached"/> with the frames <paramref name="pinned"/> a definition's own reactions set.</summary>
    private static HashSet<EnemyFrameId> With(HashSet<EnemyFrameId> reached, EnemyFrameId[] pinned) => [.. reached, .. pinned];

    /// <summary>Every frame <paramref name="table"/> maps a family frame to.</summary>
    private static HashSet<EnemyFrameId> TableFrames<TFrame>(EnemyFrameTable<TFrame> table) where TFrame : notnull =>
        [.. table.Select(entry => entry.Value)];

    /// <summary>The frames <paramref name="starts"/> reach in animations that show installed frames outright.</summary>
    private static HashSet<EnemyFrameId> Reach<TSegment, TEvent>(AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt, IEnumerable<TSegment> starts,
        Func<TEvent, IEnumerable<TSegment>>? eventTargets = null)
        where TSegment : struct, Enum
        where TEvent : notnull => Reach(segments, startingAt, static frame => frame, starts, eventTargets);

    /// <summary>
    /// The frames the animations starting at <paramref name="starts"/> show (#1600): every phase's frames, and those of the
    /// animations <paramref name="eventTargets"/> says an event continues in.
    /// </summary>
    private static HashSet<EnemyFrameId> Reach<TSegment, TFrame, TEvent>(AnimationSegments<TSegment, TFrame, TEvent> segments,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt, Func<TFrame, EnemyFrameId> shownBy, IEnumerable<TSegment> starts,
        Func<TEvent, IEnumerable<TSegment>>? eventTargets = null)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        var reached = new HashSet<TSegment>();
        var pending = new Queue<TSegment>(starts);
        var shown = new HashSet<EnemyFrameId>();
        while (pending.TryDequeue(out TSegment start))
        {
            if (!reached.Add(start))
                continue;
            foreach (AnimationPhase<TSegment> phase in startingAt[start].Phases)
            {
                foreach (AnimationBeat<TFrame, TEvent> beat in segments.Beats(phase.Segment).Skip(phase.FirstBeat).Take(phase.BeatCount))
                {
                    if (beat.ShownFrame is { } frame)
                        shown.Add(shownBy(frame));
                    else if (beat is AnimationBeat<TFrame, TEvent>.Event raised && eventTargets is not null)
                    {
                        foreach (TSegment target in eventTargets(raised.Raised))
                            pending.Enqueue(target);
                    }
                }
            }
        }
        return shown;
    }

    /// <summary>Runs the installed enemy sheets' own-frame check.</summary>
    [VerificationCommand("--enemy-sheet-own-frames")]
    internal static int EnemySheetOwnFramesCommand()
    {
        Suite(nameof(VerifyEnemySheetOwnFrames), VerifyEnemySheetOwnFrames);
        return 0;
    }
}
