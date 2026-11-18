using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Animation;
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

    /// <summary>The Skree and the Metaree each run the animations their own AI starts.</summary>
    private static EnemySheetOwnFrameFamily SkreeAndMetaree() => new("Skree and Metaree", SkreeMetareeAnimations.Segments.Frames.ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.Skree] = Reach(SkreeMetareeAnimations.Segments, SkreeMetareeAnimations.StartingAt,
                [SkreeMetareeSegment.SkreeIdling, SkreeMetareeSegment.SkreePreparingAttack, SkreeMetareeSegment.SkreeDiving]),
            [EnemyDefinitionId.Metaree] = Reach(SkreeMetareeAnimations.Segments, SkreeMetareeAnimations.StartingAt,
                [SkreeMetareeSegment.MetareeIdling, SkreeMetareeSegment.MetareePreparingAttack, SkreeMetareeSegment.MetareeDiving]),
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
            [EnemyDefinitionId.DachoraEscape] = Reach(EscapeDachoraAnimations.Segments, EscapeDachoraAnimationsConstants.StartingAt,
                EscapeDachoraAnimationsConstants.StartingAt.Keys).Union(Shown(EscapeDachoraAnimations.Segments,
                    [EscapeDachoraAnimations.RunningAroundLowTide, EscapeDachoraAnimations.RunningAroundHighTide])).ToHashSet(),
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
        AssertTrue(Placements(definition).Any(), $"a room places {definition}");
        bool[] both = [false, true];
        return both.SelectMany(active => both.Select(right =>
            FuneNamiheBehavior.SelectFuneNamiheAnimation(definition == EnemyDefinitionId.Namihe
                ? new NamiheEnemyState(NamiheAction.WaitForSamus) { ShowsActiveAnimation = active, Facing = right ? HorizontalDirection.Right : HorizontalDirection.Left }
                : new FuneEnemyState(FuneAction.WaitForCooldown) { ShowsActiveAnimation = active, Facing = right ? HorizontalDirection.Right : HorizontalDirection.Left })));
    }

    /// <summary>
    /// Kamer runs the suspensor platform's programs, the Tripper its own, as the platform initializer chooses; only the
    /// Tripper's shot and bomb reactions pin its frozen frames.
    /// </summary>
    private static EnemySheetOwnFrameFamily PlatformsFamily()
    {
        EnemyFrameId[] frozen = [EnemyFrameId.TripperFrozenFacingLeft, EnemyFrameId.TripperFrozenFacingRight];
        return new("Tripper and Kamer", PlatformAnimations.Segments.Frames.ToHashSet().Union(frozen).ToHashSet(),
            new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
            {
                [EnemyDefinitionId.SuspensorPlatform] = Reach(PlatformAnimations.Segments, PlatformAnimations.StartingAt,
                    PlatformAnimationDefinitions.Animations.Where(entry => entry.Key.Kind == PlatformKind.SuspensorPlatform).Select(entry => entry.Value)),
                [EnemyDefinitionId.Tripper] = With(Reach(PlatformAnimations.Segments, PlatformAnimations.StartingAt,
                    PlatformAnimationDefinitions.Animations.Where(entry => entry.Key.Kind == PlatformKind.Tripper).Select(entry => entry.Value)), frozen),
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
    private static EnemySheetOwnFrameFamily TurtlesFamily() => new("turtles",
        MamaTurtleAnimations.Segments.Frames.Concat(BabyTurtleAnimations.Segments.Frames).ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.MamaTurtle] = Reach(MamaTurtleAnimations.Segments, MamaTurtleAnimations.StartingAt,
            [
                MamaTurtleSegment.Asleep, MamaTurtleSegment.LeaveShellLeft, MamaTurtleSegment.LeaveShellRight,
                MamaTurtleSegment.EnterShellLeft, MamaTurtleSegment.EnterShellRight, MamaTurtleSegment.Spinning,
            ]),
            [EnemyDefinitionId.BabyTurtle] = Reach(BabyTurtleAnimations.Segments, BabyTurtleAnimations.StartingAt,
            [
                BabyTurtleSegment.CrawlingLeft, BabyTurtleSegment.CrawlingRight, BabyTurtleSegment.HidingLeft,
                BabyTurtleSegment.HidingRight, BabyTurtleSegment.Spinning, BabyTurtleSegment.LeaveShell,
            ]),
        });

    /// <summary>
    /// Each Ripper runs its own pair of moving programs; only the grapple Ripper's and jet Ripper's shot and bomb reactions pin
    /// their shared frozen frames.
    /// </summary>
    private static EnemySheetOwnFrameFamily RippersFamily()
    {
        EnemyFrameId[] frozen = [EnemyFrameId.RipperFrozenFacingLeft, EnemyFrameId.RipperFrozenFacingRight];
        return new("Ripper", RipperAnimations.Segments.Frames.ToHashSet().Union(frozen).ToHashSet(),
            new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
            {
                [EnemyDefinitionId.GrappleRipper] = With(Reach(RipperAnimations.Segments, RipperAnimations.StartingAt,
                    [RipperSegment.GrappleRipperMovingLeft, RipperSegment.GrappleRipperMovingRight]), frozen),
                [EnemyDefinitionId.JetRipper] = With(Reach(RipperAnimations.Segments, RipperAnimations.StartingAt,
                    [RipperSegment.GrappleRipperMovingLeft, RipperSegment.GrappleRipperMovingRight]), frozen),
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
            new[]
            {
                EnemyDefinitionId.KihunterGreen, EnemyDefinitionId.KihunterGreenWings, EnemyDefinitionId.KihunterRed,
                EnemyDefinitionId.KihunterRedWings, EnemyDefinitionId.KihunterYellow, EnemyDefinitionId.KihunterYellowWings,
            }.ToDictionary<EnemyDefinitionId, EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>(definition => definition, definition => Reach(KiHunterAnimations.Segments, KiHunterAnimations.StartingAt,
                KiHunterBehaviorConstants.IsKiHunterBodyDefinition(definition) ? body
                    : KiHunterBehaviorConstants.IsKiHunterWingDefinition(definition) ? wings
                    : throw new InvalidDataException($"{definition} is neither a Ki-Hunter body nor its wings."))));
    }

    /// <summary>
    /// A work robot without power holds one of its unpowered poses; a powered robot starts unpowered until the area boss
    /// falls, then runs every program of its set.
    /// </summary>
    private static EnemySheetOwnFrameFamily WorkRobotsFamily() => new("work robot", WorkRobotAnimations.Segments.Frames.ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.UnpoweredWorkRobot] = Reach(WorkRobotAnimations.Segments, WorkRobotAnimations.StartingAt,
                [WorkRobotSegment.NoPowerNeutral, WorkRobotSegment.NoPowerLeaningLeft, WorkRobotSegment.NoPowerLeaningRight]),
            [EnemyDefinitionId.WorkRobot] = Reach(WorkRobotAnimations.Segments, WorkRobotAnimations.StartingAt,
                WorkRobotAnimations.StartingAt.Keys),
        });

    /// <summary>
    /// Each Torizo starts in its initial program and the awakening that program's wake starts; its AI, shot
    /// reactions and falls start the rest, and every branch and choice its programs name is followed.
    /// </summary>
    private static EnemySheetOwnFrameFamily TorizosFamily()
    {
        TorizoAnimation bomb = TorizoInitializationDefinitions.Bomb.Animation;
        TorizoAnimation golden = TorizoInitializationDefinitions.Golden.Animation;
        TorizoAnimation[] bombStarts =
        [
            bomb, TorizoAnimation.BombTorizoAwakening, TorizoAnimation.BombTorizoGutBlowingUp, TorizoAnimation.BombTorizoFaceBlowingUp,
            TorizoAnimation.FacelessTurningLeft, TorizoAnimation.FacelessTurningRight, TorizoAnimation.TurningLeft,
            TorizoAnimation.TurningRight, TorizoAnimation.FacingLeftFalling, TorizoAnimation.FacingRightFalling,
            TorizoAnimation.Dying,
        ];
        TorizoAnimation[] goldenStarts =
        [
            golden, TorizoAnimation.GoldenTorizoAwakening, TorizoAnimation.GoldenTurningLeft, TorizoAnimation.GoldenTurningRight,
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

    /// <summary>
    /// The body animations a Torizo cue continues in, lands in or interrupts the walk with (#2028); a decision's owner
    /// continues in the animation for the posture the walk's step left, either way and on either foot (#2175).
    /// </summary>
    private static IEnumerable<TorizoAnimation> TorizoCueTargets(TorizoCue cue) => cue switch
    {
        TorizoCue.Branch branch => [.. new[] { HorizontalDirection.Left, HorizontalDirection.Right }.SelectMany(direction =>
            new[] { HorizontalDirection.Left, HorizontalDirection.Right }.Select(foot =>
                TorizoBranches.Target(branch.Kind, TorizoPosture.AfterStep(direction, foot))))],
        TorizoCue.Interrupt interrupt => [interrupt.Animation],
        TorizoCue.Choice choice => [choice.Target, choice.Alternate],
        TorizoCue.LandIn landIn => [landIn.Landing],
        TorizoCue.WalkOn walkOn => [walkOn.Walking],
        TorizoCue.Turn turn => [turn.WhenFacingRight, turn.WhenFacingLeft],
        _ => [],
    };

    /// <summary>
    /// Ridley in Lower Norfair stands, roars, lifts off, breathes fire, turns and roars as he dies; Ceres Ridley stands,
    /// roars, lifts off, lunges, takes the Baby and breathes fire. His death breakup runs no body program.
    /// </summary>
    private static EnemySheetOwnFrameFamily RidleysFamily() => new("Ridley", RidleyAnimations.Segments.Frames.ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.Ridley] = Reach(RidleyAnimations.Segments, RidleyAnimations.StartingAt,
            [
                .. EitherFacing(RidleyAnimations.Initial, RidleyAnimations.OpeningRoar, RidleyAnimations.TransitionToFlying, RidleyAnimations.Fireballing,
                    RidleyAnimations.DeathRoar),
                RidleyBodySegment.TurnFromLeftToRight, RidleyBodySegment.TurnFromRightToLeft,
            ], RidleyCueTargets),
            [EnemyDefinitionId.RidleyCeres] = Reach(RidleyAnimations.Segments, RidleyAnimations.StartingAt,
            [
                .. EitherFacing(RidleyAnimations.Initial, RidleyAnimations.OpeningRoar, RidleyAnimations.TransitionToFlying, RidleyAnimations.CeresLunge,
                    RidleyAnimations.RetrieveBabyMetroid, RidleyAnimations.Fireballing),
            ], RidleyCueTargets),
            [EnemyDefinitionId.RidleyBreakupPiece] = new HashSet<EnemyFrameId>(),
        });

    /// <summary>The segments Ridley's owner starts <paramref name="animations"/> at, facing either way (#2175).</summary>
    private static IEnumerable<RidleyBodySegment> EitherFacing(params RidleyFacedAnimation[] animations) =>
        animations.SelectMany(animation => new[] { animation.FacingLeft, animation.FacingRight });

    /// <summary>The body segments whose animation a Ridley cue's owner continues in: a cut-short fireballing's end, either way (#2175).</summary>
    private static IEnumerable<RidleyBodySegment> RidleyCueTargets(RidleyBodyCue cue) => cue switch
    {
        RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.MayCutFireballingShort } => EitherFacing(RidleyAnimations.FireballingEnd),
        _ => [],
    };

    /// <summary>Every room placement of <paramref name="definition"/>.</summary>
    private static IEnumerable<RoomEnemyPlacement> Placements(EnemyDefinitionId definition) =>
        RoomEnemyPopulationDefinitions.Definitions.SelectMany(list => list.Placements.ToArray()).Where(placement => placement.DefinitionId == definition);

    /// <summary><paramref name="reached"/> with the frames <paramref name="pinned"/> a definition's own reactions set.</summary>
    private static HashSet<EnemyFrameId> With(HashSet<EnemyFrameId> reached, EnemyFrameId[] pinned) => [.. reached, .. pinned];

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

    /// <summary>The frames every phase of <paramref name="animations"/> shows: animations an owner starts by name.</summary>
    private static HashSet<EnemyFrameId> Shown<TSegment, TEvent>(AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        IEnumerable<Animation<TSegment>> animations)
        where TSegment : struct, Enum
        where TEvent : notnull =>
        [.. animations.SelectMany(animation => animation.Phases)
            .SelectMany(phase => segments.Beats(phase.Segment).Skip(phase.FirstBeat).Take(phase.BeatCount))
            .Select(beat => beat.ShownFrame)
            .OfType<EnemyFrameId>()];

    /// <summary>Runs the installed enemy sheets' own-frame check.</summary>
    [VerificationCommand("--enemy-sheet-own-frames")]
    internal static int EnemySheetOwnFramesCommand()
    {
        Suite(nameof(VerifyEnemySheetOwnFrames), VerifyEnemySheetOwnFrames);
        return 0;
    }
}
