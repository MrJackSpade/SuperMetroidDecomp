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
using SuperMetroid.Core.Programs;
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
    private static EnemySheetOwnFrameFamily SkreeAndMetaree() => new("Skree and Metaree", TableFrames(SkreeMetareeFrameDefinitions.ShownFrames),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.Skree] = Reach(SkreeMetareeAnimations.Segments, SkreeMetareeAnimations.StartingAt, SkreeMetareeFrameDefinitions.ShownFrames,
                SkreeMetareeAnimationDefinitions.SkreePrograms.Values),
            [EnemyDefinitionId.Metaree] = Reach(SkreeMetareeAnimations.Segments, SkreeMetareeAnimations.StartingAt, SkreeMetareeFrameDefinitions.ShownFrames,
                SkreeMetareeAnimationDefinitions.MetareePrograms.Values),
        });

    /// <summary>Each Sidehopper and Dessgeega runs the programs of the variant its header names, on either surface.</summary>
    private static EnemySheetOwnFrameFamily HoppersFamily()
    {
        EnemyDefinitionId[] hoppers =
        [
            EnemyDefinitionId.Sidehopper, EnemyDefinitionId.SidehopperLarge, EnemyDefinitionId.SidehopperTourian,
            EnemyDefinitionId.Dessgeega, EnemyDefinitionId.DessgeegaLarge,
        ];
        return new("Sidehopper and Dessgeega", TableFrames(HopperFrameDefinitions.ShownFrames),
            hoppers.ToDictionary<EnemyDefinitionId, EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>(definition => definition, definition => Reach(HopperAnimations.Segments, HopperAnimations.StartingAt, HopperFrameDefinitions.ShownFrames,
                HopperAnimationDefinitions.Programs
                    .Where(entry => entry.Key.Kind == HopperPrograms.KindOf(definition))
                    .Select(entry => entry.Value))));
    }

    /// <summary>The escaping Etecoon and Dachora each run only their own program set and frame table.</summary>
    private static EnemySheetOwnFrameFamily EscapeAnimalsFamily() => new("escaping animals",
        TableFrames(EscapeEtecoonFrameDefinitions.ShownFrames).Union(TableFrames(EscapeDachoraFrameDefinitions.ShownFrames)).ToHashSet(),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.EtecoonEscape] = Reach(EscapeEtecoonAnimations.Segments, EscapeEtecoonAnimations.StartingAt, EscapeEtecoonFrameDefinitions.ShownFrames,
                EscapeEtecoonAnimations.StartingAt.Keys),
            [EnemyDefinitionId.DachoraEscape] = Reach(EscapeDachoraAnimations.Segments, EscapeDachoraAnimations.StartingAt, EscapeDachoraFrameDefinitions.ShownFrames,
                EscapeDachoraAnimations.StartingAt.Keys),
        });

    /// <summary>A Fune or Namihe runs the programs its placements' variant selects, idle or active, facing either way.</summary>
    private static EnemySheetOwnFrameFamily FuneAndNamihe() => new("Fune and Namihe", TableFrames(FuneNamiheFrameDefinitions.ShownFrames),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.Fune] = Reach(FuneNamiheAnimations.Segments, FuneNamiheAnimations.StartingAt, FuneNamiheFrameDefinitions.ShownFrames, PlacedFuneNamihePrograms(EnemyDefinitionId.Fune)),
            [EnemyDefinitionId.Namihe] = Reach(FuneNamiheAnimations.Segments, FuneNamiheAnimations.StartingAt, FuneNamiheFrameDefinitions.ShownFrames, PlacedFuneNamihePrograms(EnemyDefinitionId.Namihe)),
        });

    /// <summary>The programs every room placement of <paramref name="definition"/> can select.</summary>
    private static IEnumerable<FuneNamiheProgram> PlacedFuneNamihePrograms(EnemyDefinitionId definition)
    {
        FuneNamiheSpawnSettings[] placements = [.. Placements(definition).Select(placement => placement.SpawnSettingsOf<FuneNamiheSpawnSettings>())];
        AssertTrue(placements.Length > 0, $"a room places {definition}");
        bool[] both = [false, true];
        return placements.SelectMany(settings => both.SelectMany(active => both.Select(right =>
            FuneNamiheBehavior.SelectFuneNamiheProgram(new FuneNamiheEnemyState { Variant = settings.Variant, ShowsActiveAnimation = active, FacingRight = right }))));
    }

    /// <summary>
    /// Kamer runs the suspensor platform's programs, the Tripper its own, as the platform initializer chooses; only the
    /// Tripper's shot and bomb reactions pin its frozen frames.
    /// </summary>
    private static EnemySheetOwnFrameFamily PlatformsFamily()
    {
        EnemyFrameId[] frozen = [EnemyFrameId.TripperFrozenMovingLeft, EnemyFrameId.TripperFrozenMovingRight];
        return new("Tripper and Kamer", TableFrames(PlatformFrameDefinitions.ShownFrames).Union(frozen).ToHashSet(),
            new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
            {
                [EnemyDefinitionId.Kamer2] = Reach(PlatformAnimations.Segments, PlatformAnimations.StartingAt, PlatformFrameDefinitions.ShownFrames,
                    PlatformAnimationDefinitions.Programs.Where(entry => entry.Key.Kamer).Select(entry => entry.Value)),
                [EnemyDefinitionId.Tripper] = With(Reach(PlatformAnimations.Segments, PlatformAnimations.StartingAt, PlatformFrameDefinitions.ShownFrames,
                    PlatformAnimationDefinitions.Programs.Where(entry => !entry.Key.Kamer).Select(entry => entry.Value)), frozen),
            });
    }

    /// <summary>An Evir placement is its body or arms; the Evir projectile runs only the spit's programs.</summary>
    private static EnemySheetOwnFrameFamily EvirFamily() => new("Evir", TableFrames(EvirFrameDefinitions.ShownFrames),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.Evir] = Reach(EvirAnimations.Segments, EvirAnimations.StartingAt, EvirFrameDefinitions.ShownFrames,
                [EvirProgram.BodyFacingLeft, EvirProgram.BodyFacingRight, EvirProgram.ArmsFacingLeft, EvirProgram.ArmsFacingRight]),
            [EnemyDefinitionId.EvirProjectile] = Reach(EvirAnimations.Segments, EvirAnimations.StartingAt, EvirFrameDefinitions.ShownFrames,
                [EvirProgram.ProjectileNormal, EvirProgram.ProjectileRegenerating]),
        });

    /// <summary>The gunship's top is its top hull; its bottom is the bottom hull or the entrance pad, which opens and closes.</summary>
    private static EnemySheetOwnFrameFamily GunshipFamily() => new("gunship", TableFrames(GunshipFrameDefinitions.ShownFrames),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.ShipTop] = Reach(GunshipAnimations.Segments, GunshipAnimations.StartingAt, GunshipFrameDefinitions.ShownFrames, [GunshipProgram.TopHull]),
            [EnemyDefinitionId.ShipBottomEntrance] = Reach(GunshipAnimations.Segments, GunshipAnimations.StartingAt, GunshipFrameDefinitions.ShownFrames,
                [GunshipProgram.BottomHull, GunshipProgram.BottomEntrancePad, GunshipProgram.EntrancePadOpening, GunshipProgram.EntrancePadClosing]),
        });

    /// <summary>The Mama Turtle sleeps, leaves and enters her shell and spins; the baby crawls, hides, leaves its shell and spins.</summary>
    private static EnemySheetOwnFrameFamily TurtlesFamily() => new("turtles", TableFrames(MamaTurtleFrameDefinitions.ShownFrames),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.MamaTurtle] = Reach(MamaTurtleAnimations.Segments, MamaTurtleAnimations.StartingAt, MamaTurtleFrameDefinitions.ShownFrames,
            [
                MamaTurtleProgram.MamaAsleep, MamaTurtleProgram.MamaLeaveShellLeft, MamaTurtleProgram.MamaLeaveShellRight,
                MamaTurtleProgram.MamaEnterShellLeft, MamaTurtleProgram.MamaEnterShellRight, MamaTurtleProgram.MamaSpinning,
            ]),
            [EnemyDefinitionId.BabyTurtle] = Reach(MamaTurtleAnimations.Segments, MamaTurtleAnimations.StartingAt, MamaTurtleFrameDefinitions.ShownFrames,
            [
                MamaTurtleProgram.BabyCrawlingLeft, MamaTurtleProgram.BabyCrawlingRight, MamaTurtleProgram.BabyHidingLeft,
                MamaTurtleProgram.BabyHidingRight, MamaTurtleProgram.BabySpinning, MamaTurtleProgram.BabyLeaveShellLeft,
                MamaTurtleProgram.BabyLeaveShellRight,
            ]),
        });

    /// <summary>
    /// Each Ripper runs its own pair of moving programs; only the GRipper's and Ripper II's shot and bomb reactions pin
    /// their shared frozen frames.
    /// </summary>
    private static EnemySheetOwnFrameFamily RippersFamily()
    {
        EnemyFrameId[] frozen = [EnemyFrameId.RipperSharedFrozenLeft, EnemyFrameId.RipperSharedFrozenRight];
        return new("Ripper", TableFrames(RipperFrameDefinitions.ShownFrames).Union(frozen).ToHashSet(),
            new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
            {
                [EnemyDefinitionId.GRipper] = With(Reach(RipperAnimations.Segments, RipperAnimations.StartingAt, RipperFrameDefinitions.ShownFrames,
                    [RipperProgram.GRipperMovingLeft, RipperProgram.GRipperMovingRight]), frozen),
                [EnemyDefinitionId.Ripper2] = With(Reach(RipperAnimations.Segments, RipperAnimations.StartingAt, RipperFrameDefinitions.ShownFrames,
                    [RipperProgram.Ripper2MovingLeft, RipperProgram.Ripper2MovingRight]), frozen),
                [EnemyDefinitionId.Ripper] = Reach(RipperAnimations.Segments, RipperAnimations.StartingAt, RipperFrameDefinitions.ShownFrames,
                    [RipperProgram.RipperMovingLeft, RipperProgram.RipperMovingRight]),
            });
    }

    /// <summary>A Ki-Hunter's body flies, swoops, hops, lands and spits; its wings follow, or fly off detached.</summary>
    private static EnemySheetOwnFrameFamily KiHuntersFamily()
    {
        KiHunterProgram[] body =
        [
            KiHunterProgram.FlyingLeft, KiHunterProgram.FlyingRight, KiHunterProgram.SwoopLeft, KiHunterProgram.SwoopRight,
            KiHunterProgram.JumpLeft, KiHunterProgram.JumpRight, KiHunterProgram.LandLeft, KiHunterProgram.LandRight,
            KiHunterProgram.SpitLeft, KiHunterProgram.SpitRight,
        ];
        KiHunterProgram[] wings = [KiHunterProgram.WingsLeft, KiHunterProgram.WingsRight, KiHunterProgram.DetachedWings];
        return new("Ki-Hunter", TableFrames(KiHunterFrameDefinitions.ShownFrames),
            KiHunterPartDefinitions.Parts.Keys.ToDictionary<EnemyDefinitionId, EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>(definition => definition, definition => Reach(KiHunterAnimations.Segments, KiHunterAnimations.StartingAt,
                KiHunterFrameDefinitions.ShownFrames,
                KiHunterBehavior.IsKiHunterBodyDefinition(definition) ? body
                    : KiHunterBehavior.IsKiHunterWingDefinition(definition) ? wings
                    : throw new InvalidDataException($"{definition} is neither a Ki-Hunter body nor its wings."))));
    }

    /// <summary>
    /// A work robot without power holds one of its unpowered poses; a powered robot starts unpowered until the area boss
    /// falls, then runs every program of its set.
    /// </summary>
    private static EnemySheetOwnFrameFamily WorkRobotsFamily() => new("work robot", TableFrames(WorkRobotFrameDefinitions.ShownFrames),
        new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
        {
            [EnemyDefinitionId.RobotNoPower] = Reach(WorkRobotAnimations.Segments, WorkRobotAnimations.StartingAt, WorkRobotFrameDefinitions.ShownFrames,
                [WorkRobotProgram.NoPowerNeutral, WorkRobotProgram.NoPowerLeaningLeft, WorkRobotProgram.NoPowerLeaningRight]),
            [EnemyDefinitionId.Robot] = Reach(WorkRobotAnimations.Segments, WorkRobotAnimations.StartingAt, WorkRobotFrameDefinitions.ShownFrames,
                Enum.GetValues<WorkRobotProgram>()),
        });

    /// <summary>
    /// Each Torizo starts in its initial program and the program it resumes after that program's sleep; its AI, shot
    /// reactions and falls start the rest, and every branch and choice its programs name is followed.
    /// </summary>
    private static EnemySheetOwnFrameFamily TorizosFamily()
    {
        TorizoBodyProgram bomb = TorizoInitializationDefinitions.Bomb.Program;
        TorizoBodyProgram golden = TorizoInitializationDefinitions.Golden.Program;
        TorizoBodyProgram[] bombStarts =
        [
            bomb, TorizoAnimations.AfterSleep(bomb), TorizoBodyProgram.SpecialCallableBlowUpBombTorizosGut, TorizoBodyProgram.CallableBlowUpBombTorizosFace,
            TorizoBodyProgram.FacingLeftFacelessTurningLeft, TorizoBodyProgram.FacingRightFacelessTurningRight, TorizoBodyProgram.FacingLeftTurningLeft,
            TorizoBodyProgram.FacingRightTurningRight, TorizoBodyProgram.FacingLeftFalling0, TorizoBodyProgram.FacingRightFalling0,
            TorizoBodyProgram.DeathSequence0,
        ];
        TorizoBodyProgram[] goldenStarts =
        [
            golden, TorizoAnimations.AfterSleep(golden), TorizoBodyProgram.GoldenTurningLeft, TorizoBodyProgram.GoldenTurningRight,
            TorizoBodyProgram.GoldenDodgeTurningLeft, TorizoBodyProgram.GoldenDodgeTurningRight, TorizoBodyProgram.GoldenCaughtSuperFacingLeftLeftLegFwd,
            TorizoBodyProgram.GoldenCaughtSuperFacingLeftRightLegFwd, TorizoBodyProgram.GoldenCaughtSuperFacingRightLeftLegFwd,
            TorizoBodyProgram.GoldenCaughtSuperFacingRightRightLegFwd, TorizoBodyProgram.FacingLeftFalling0, TorizoBodyProgram.FacingRightFalling0,
            TorizoBodyProgram.DeathSequence0,
        ];
        return new("Torizo", TableFrames(TorizoFrameDefinitions.ShownFrames),
            new Dictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>>
            {
                [EnemyDefinitionId.BombTorizo] = Reach(TorizoAnimations.Segments, TorizoAnimations.StartingAt, TorizoFrameDefinitions.ShownFrames, bombStarts, TorizoCueTargets),
                [EnemyDefinitionId.GoldenTorizo] = Reach(TorizoAnimations.Segments, TorizoAnimations.StartingAt, TorizoFrameDefinitions.ShownFrames, goldenStarts, TorizoCueTargets),
            });
    }

    /// <summary>The body programs a Torizo cue branches, calls or links to.</summary>
    private static IEnumerable<TorizoBodyProgram> TorizoCueTargets(TorizoCue cue) => cue switch
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
            [EnemyDefinitionId.Ridley] = Reach(RidleyAnimations.Segments, RidleyAnimations.StartingAt, RidleyBodyFrameDefinitions.ShownFrames,
            [
                RidleyBodyProgram.Initial, RidleyBodyProgram.OpeningRoar, RidleyBodyProgram.TransitionToFlying, RidleyBodyProgram.Fireballing,
                RidleyBodyProgram.TurnFromLeftToRight, RidleyBodyProgram.TurnFromRightToLeft, RidleyBodyProgram.DeathRoar,
            ], RidleyCueTargets),
            [EnemyDefinitionId.RidleyCeres] = Reach(RidleyAnimations.Segments, RidleyAnimations.StartingAt, RidleyBodyFrameDefinitions.ShownFrames,
            [
                RidleyBodyProgram.Initial, RidleyBodyProgram.OpeningRoar, RidleyBodyProgram.TransitionToFlying, RidleyBodyProgram.CeresLunge,
                RidleyBodyProgram.RetrieveBabyMetroid, RidleyBodyProgram.Fireballing,
            ], RidleyCueTargets),
            [EnemyDefinitionId.RidleyExplosion] = new HashSet<EnemyFrameId>(),
        });

    /// <summary>The body segment whose animation a Ridley cue redirects to.</summary>
    private static IEnumerable<RidleyBodyProgram> RidleyCueTargets(RidleyBodyCue cue) => cue switch
    {
        RidleyBodyCue.GoToUnlessFacingLeft redirect => [redirect.Target],
        RidleyBodyCue.GoToIfCeresAndSamusLowEnergy redirect => [redirect.Target],
        _ => [],
    };

    /// <summary>Every room placement of <paramref name="definition"/>.</summary>
    private static IEnumerable<RoomEnemyPopulationRecord> Placements(EnemyDefinitionId definition) =>
        RoomEnemyPopulationDefinitions.Definitions.SelectMany(list => list.Records.ToArray()).Where(placement => placement.DefinitionId == definition);

    /// <summary><paramref name="reached"/> with the frames <paramref name="pinned"/> a definition's own reactions set.</summary>
    private static HashSet<EnemyFrameId> With(HashSet<EnemyFrameId> reached, EnemyFrameId[] pinned) => [.. reached, .. pinned];

    /// <summary>Every frame <paramref name="table"/> maps a family frame to.</summary>
    private static HashSet<EnemyFrameId> TableFrames<TFrame>(EnemyFrameTable<TFrame> table) where TFrame : notnull =>
        [.. table.Select(entry => entry.Value)];

    /// <summary>
    /// The frames the animations starting at <paramref name="starts"/> show (#1600): every phase's frames, and those of the
    /// animations <paramref name="eventTargets"/> says an event continues in.
    /// </summary>
    private static HashSet<EnemyFrameId> Reach<TSegment, TFrame, TEvent>(AnimationSegments<TSegment, TFrame, TEvent> segments,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt, EnemyFrameTable<TFrame> table, IEnumerable<TSegment> starts,
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
                        shown.Add(table.Of(frame));
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
