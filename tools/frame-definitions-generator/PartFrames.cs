using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// How a multi-part behaviour's frame table is split between its parts (#1513). Each part slot runs shared part-animation
/// code that reads the whole table, so the walk alone would give every part every frame; instead each part's table frames
/// are those its own programs show (a program set's closure from the programs each part's slot is started in) or those
/// whose table keys carry the part's selectors.
/// </summary>
/// <param name="Behaviour">The behaviour type's name.</param>
/// <param name="TableType">The type declaring the frame table.</param>
/// <param name="TableMember">The frame table property: <c>{ key, EnemyFrameId.X }</c> entries.</param>
/// <param name="ProgramsType">The type declaring the part program set, or null when parts select by key.</param>
/// <param name="ProgramsMember">
/// The program set property: <c>[P.X] = [steps]</c> entries, or a set joining program tables it spreads in (Torizo's joins
/// four), whose entries are read from those tables.
/// </param>
/// <param name="StartCue">The cue record starting every part's program at once, or null.</param>
/// <param name="StartCueDefinitions">The definition each of the start cue's positional programs is started for.</param>
/// <param name="EntryPrograms">The programs code starts each definition's slot in, by definition.</param>
/// <param name="KeySelectors">
/// For a table split by key, the <c>Parameter=Value</c> selectors each definition's keys carry; an enum key is
/// <c>EnumType=Member</c>.
/// </param>
/// <param name="ExcludedType">The type declaring frames drawn on another layer than the sprites, or null.</param>
/// <param name="ExcludedMember">That type's set of those frames.</param>
/// <param name="PopulationChoices">
/// The population or header parameters an entry program choice follows from, each checked against every room placement
/// or compiled header of its definition.
/// </param>
internal sealed record PartFrameConfiguration(
    string Behaviour, string TableType, string TableMember, string? ProgramsType, string? ProgramsMember, string? StartCue,
    IReadOnlyList<string> StartCueDefinitions, IReadOnlyDictionary<string, string[]> EntryPrograms,
    IReadOnlyDictionary<string, string[]> KeySelectors, string? ExcludedType, string? ExcludedMember,
    IReadOnlyList<PopulationChoice> PopulationChoices)
{
    /// <summary>
    /// The types besides <see cref="ProgramsType"/> whose step helpers the programs call, as Torizo's programs call
    /// <c>TorizoProgramSteps.Show(int, int)</c>.
    /// </summary>
    public IReadOnlyList<string> StepHelperTypes { get; init; } = [];

    /// <summary>
    /// Frames outside the table that only the named definitions' own code sets, by frame: a reaction that pins a frozen
    /// frame for its own definitions only. Every other definition of the behaviour is left without them.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> CodeFrames { get; init; } = new Dictionary<string, string[]>(StringComparer.Ordinal);

    /// <summary>
    /// The configured behaviours: Draygon's parts each show the programs its slot runs (the body, eye, tail and arms
    /// started together by <c>StartPartPrograms</c> and separately by the code each program start names), and none of his
    /// sprites shows a pose he draws on his body layer; Kraid's parts each show their own kind of pose, Kraid himself none.
    /// The shutters, crawlers, pipe bugs and dead monsters each run their behaviour's one program set and frame table, of
    /// which each definition shows only the programs its own code, or its rooms' population parameters, start it in.
    /// Every other behaviour serving several definitions over one shared table, and the Skree and Metaree behaviours sharing
    /// one, does the same (#1513): each definition shows only the frames its own programs, header variant or placements reach.
    /// </summary>
    public static IReadOnlyList<PartFrameConfiguration> All { get; } =
    [
        new("DraygonBehavior", "DraygonFrameDefinitions", "ShownFrames", "DraygonAnimations", "Programs", "StartPartPrograms",
            ["DraygonBody", "DraygonEye", "DraygonTail", "DraygonArms"],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["DraygonBody"] =
                [
                    "BodyFacingLeftIdle", "BodyFacingLeftReset", "BodyFacingRightReset", "BodyFacingLeftFireGoop", "BodyFacingRightFireGoop",
                    "BodyFacingLeftRoar", "BodyFacingRightRoar", "ArmsFacingLeftDying", "ArmsFacingRightDying", "Sleep", "Delete",
                ],
                ["DraygonEye"] =
                [
                    "EyeFacingLeftIdle", "EyeFacingLeftDying", "EyeFacingRightDying", "EyeFacingLeftDead", "EyeFacingRightDead",
                    "EyeFacingLeftLookingUp", "EyeFacingRightLookingUp", "EyeFacingLeftLookingRight", "EyeFacingRightLookingRight",
                    "EyeFacingLeftLookingDown", "EyeFacingRightLookingDown", "EyeFacingLeftLookingLeft", "EyeFacingRightLookingLeft",
                ],
                ["DraygonTail"] =
                [
                    "TailFacingLeftInitialFakeWhip", "TailFacingLeftWhip", "TailFacingRightWhip", "TailFacingLeftFinalWhips",
                    "TailFacingRightFinalWhips", "TailFacingLeftFlail", "TailFacingRightFlail", "Sleep",
                ],
                ["DraygonArms"] =
                [
                    "ArmsFacingLeftIdle", "ArmsFacingRightIdle", "ArmsFacingLeftNearSwoopApex", "ArmsFacingRightNearSwoopApex",
                    "ArmsFacingLeftGrab", "ArmsFacingRightGrab", "Sleep",
                ],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), "DraygonBodyLayerFrameDefinitions", "Frames", []),
        new("KraidBehavior", "KraidPartFrameDefinitions", "ShownFrames", null, null, null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal),
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Kraid"] = [],
                ["KraidArm"] = ["Kind=Arm"],
                ["KraidFoot"] = ["Kind=Foot", "Kind=FootStanding"],
                ["KraidLintTop"] = ["Kind=Lint"],
                ["KraidLintMiddle"] = ["Kind=Lint"],
                ["KraidLintBottom"] = ["Kind=Lint"],
                ["KraidNail"] = ["Kind=Nail"],
                ["KraidNailBad"] = ["Kind=Nail"],
            },
            null, null, []),
        // Mother Brain's body poses are her body's: its slot draws their sprite limbs; her head and tubes draw none.
        new("MotherBrainBehavior", "MotherBrainBodyVisualDefinitions", "ShownFrames", null, null, null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal),
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["MotherBrainBody"] = [.. Enumerable.Range(0, 17).Select(index => $"Index={index}")],
                ["MotherBrainHead"] = [],
                ["MotherBrainTubes"] = [],
            },
            null, null, []),
        // A vertical shutter runs its forty-pixel program, Kamer its platform loop; a growing shutter grows through its
        // three growth programs to the forty-pixel one; the horizontal shutter runs its own.
        new("ShuttersBehavior", "ShutterFrameDefinitions", "ShownFrames", "ShutterAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Kamer"] = ["KamerPlatform"],
                ["ShutterDestroyable"] = ["FortyPixels"],
                ["ShutterShootable"] = ["FortyPixels"],
                ["ShutterGrowing"] = ["GrowingTenPixels", "GrowingTwentyPixels", "GrowingThirtyPixels", "FortyPixels"],
                ["ShutterHorizShootable"] = ["Horizontal"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // Each crawler starts in its family's surface program and takes its later surfaces from its surface family:
        // Sciser, Zero and Viola their own, the shared crawlers the shared family their rooms' entries name; the orange
        // Zoomer chooses only among its own.
        new("CrawlersBehavior", "CrawlerFrameDefinitions", "ShownFrames", "CrawlerAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Sciser"] = CrawlerFamily("Sciser"),
                ["Zero"] = CrawlerFamily("Zero"),
                ["Viola"] = CrawlerFamily("Viola"),
                ["Zeela"] = CrawlerFamily("Shared"),
                ["Sova"] = CrawlerFamily("Shared"),
                ["Zoomer"] = CrawlerFamily("Shared"),
                ["MZoomer"] = CrawlerFamily("Shared"),
                ["HZoomer"] = CrawlerFamily("HZoomer"),
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null,
            [
                new("Zeela", "CrawlerSpawnSettings", "SurfaceFamily", ["Shared"]),
                new("Sova", "CrawlerSpawnSettings", "SurfaceFamily", ["Shared"]),
                new("Zoomer", "CrawlerSpawnSettings", "SurfaceFamily", ["Shared"]),
                new("MZoomer", "CrawlerSpawnSettings", "SurfaceFamily", ["Shared"]),
            ]),
        // A Brinstar pipe bug runs the weak or the strong programs its room's kind selects; Gamet and Geega their own.
        new("PipeBugsBehavior", "PipeBugFrameDefinitions", "ShownFrames", "PipeBugAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Zeb"] = ["ZebRisingLeft", "ZebShootingLeft", "ZebRisingRight", "ZebShootingRight"],
                ["Zebbo"] = ["ZebboRisingLeft", "ZebboShootingLeft", "ZebboRisingRight", "ZebboShootingRight"],
                ["Gamet"] = ["GametRisingLeft", "GametFlyingLeft", "GametRisingRight", "GametFlyingRight"],
                ["Geega"] = ["GeegaFlyingLeft", "GeegaArcingLeft", "GeegaFlyingRight", "GeegaArcingRight"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null,
            [
                new("Zeb", "BrinstarPipeBugSpawnSettings", "Kind", ["Weak"]),
                new("Zebbo", "BrinstarPipeBugSpawnSettings", "Kind", ["Strong"]),
            ]),
        // The dead Sidehopper idles, hops and lies as a corpse, or lies dead from the start; the dead Tourian monsters
        // each lie as one of their own species' corpses. Both run the dead monsters' one program set.
        new("DeadSidehopperBehavior", "DeadTourianFrameDefinitions", "ShownFrames", "DeadTourianAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["CorpseSidehopper"] = ["SidehopperIdle", "SidehopperHopping", "SidehopperCorpse", "SidehopperInitiallyDead"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        new("DeadTourianBehavior", "DeadTourianFrameDefinitions", "ShownFrames", "DeadTourianAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["CorpseZoomer"] = ["ZoomerCorpse0", "ZoomerCorpse1", "ZoomerCorpse2"],
                ["CorpseRipper"] = ["RipperCorpse0", "RipperCorpse1"],
                ["CorpseSkree"] = ["SkreeCorpse0", "SkreeCorpse1", "SkreeCorpse2"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // The Skree and the Metaree are separate behaviours running one program set and frame table: each runs the four
        // programs its own selector table names (#1513).
        new("SkreeBehavior", "SkreeMetareeFrameDefinitions", "ShownFrames", "SkreeMetareeAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Skree"] = ["SkreeIdling", "SkreePreparingAttack", "SkreeDiving", "SkreeStopAnimating"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        new("MetareeBehavior", "SkreeMetareeFrameDefinitions", "ShownFrames", "SkreeMetareeAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Metaree"] = ["MetareeIdling", "MetareePreparingAttack", "MetareeDiving", "MetareeStopAnimating"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // A hopper runs the programs of the variant its header names (0 Sidehopper, 1 large Sidehopper, 2 large Dessgeega,
        // 3 Dessgeega), on the floor or the ceiling its placement mounts it on.
        new("HoppersBehavior", "HopperFrameDefinitions", "ShownFrames", "HopperAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Dessgeega"] = HopperVariant("Dessgeega"),
                ["DessgeegaLarge"] = HopperVariant("LargeDessgeega"),
                ["Sidehopper"] = HopperVariant("Sidehopper"),
                ["SidehopperLarge"] = HopperVariant("LargeSidehopper"),
                ["SidehopperTourian"] = HopperVariant("LargeSidehopper"),
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null,
            [
                new("Sidehopper", "RoomEnemyDefinition", "VariantIndex", ["0"]),
                new("SidehopperLarge", "RoomEnemyDefinition", "VariantIndex", ["1"]),
                new("SidehopperTourian", "RoomEnemyDefinition", "VariantIndex", ["1"]),
                new("DessgeegaLarge", "RoomEnemyDefinition", "VariantIndex", ["2"]),
                new("Dessgeega", "RoomEnemyDefinition", "VariantIndex", ["3"]),
            ]),
        // A Fune or Namihe runs the idle and active programs its placements' variant selects, facing either way.
        new("FuneNamiheBehavior", "FuneNamiheFrameDefinitions", "ShownFrames", "FuneNamiheAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Fune"] = ["FuneIdleLeft", "FuneActiveLeft", "FuneIdleRight", "FuneActiveRight"],
                ["Namihe"] = ["NamiheIdleLeft", "NamiheActiveLeft", "NamiheIdleRight", "NamiheActiveRight"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null,
            [
                new("Fune", "FuneNamiheSpawnSettings", "Variant", ["Fune"]),
                new("Namihe", "FuneNamiheSpawnSettings", "Variant", ["Namihe"]),
            ]),
        // Kamer runs the suspensor platform's programs and the Tripper its own, as the platform initializer chooses by
        // definition; only the Tripper's shot and bomb reactions pin its frozen frames.
        new("PlatformsBehavior", "PlatformFrameDefinitions", "ShownFrames", "PlatformAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Kamer2"] = ["KamerMovingLeft", "KamerMovingRight", "KamerStillLeft", "KamerStillRight"],
                ["Tripper"] = ["TripperMovingLeft", "TripperMovingRight", "TripperStillMovingLeft", "TripperStillMovingRight"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, [])
        {
            CodeFrames = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["TripperFrozenMovingLeft"] = ["Tripper"],
                ["TripperFrozenMovingRight"] = ["Tripper"],
            },
        },
        // An Evir placement is its body or its arms; the Evir projectile runs only the spit's programs.
        new("EvirBehavior", "EvirFrameDefinitions", "ShownFrames", "EvirAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Evir"] = ["BodyFacingLeft", "BodyFacingRight", "ArmsFacingLeft", "ArmsFacingRight"],
                ["EvirProjectile"] = ["ProjectileNormal", "ProjectileRegenerating"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // The gunship's top is its top hull; its bottom is the bottom hull or the entrance pad, which opens and closes.
        new("GunshipBehavior", "GunshipFrameDefinitions", "ShownFrames", "GunshipAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["ShipBottomEntrance"] = ["BottomHull", "BottomEntrancePad", "EntrancePadOpening", "EntrancePadClosing"],
                ["ShipTop"] = ["TopHull"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // The Mama Turtle sleeps, leaves and enters her shell and spins; the baby crawls, hides, leaves its shell and spins.
        new("MamaTurtleBehavior", "MamaTurtleFrameDefinitions", "ShownFrames", "MamaTurtleAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["BabyTurtle"] =
                [
                    "BabyCrawlingLeft", "BabyCrawlingRight", "BabyHidingLeft", "BabyHidingRight", "BabySpinning", "BabyLeaveShellLeft",
                    "BabyLeaveShellRight",
                ],
                ["MamaTurtle"] =
                [
                    "MamaAsleep", "MamaLeaveShellLeft", "MamaLeaveShellRight", "MamaEnterShellLeft", "MamaEnterShellRight", "MamaSpinning",
                ],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // Each Ripper runs its own pair of moving programs; only the GRipper's and Ripper II's reactions pin their shared
        // frozen frames.
        new("RipperBehavior", "RipperFrameDefinitions", "ShownFrames", "RipperAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["GRipper"] = ["GRipperMovingLeft", "GRipperMovingRight"],
                ["Ripper"] = ["RipperMovingLeft", "RipperMovingRight"],
                ["Ripper2"] = ["Ripper2MovingLeft", "Ripper2MovingRight"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, [])
        {
            CodeFrames = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["RipperSharedFrozenLeft"] = ["GRipper", "Ripper2"],
                ["RipperSharedFrozenRight"] = ["GRipper", "Ripper2"],
            },
        },
        // A Ki-Hunter's body flies, swoops, hops, lands and spits; its wings follow it, or fly off detached.
        new("KiHunterBehavior", "KiHunterFrameDefinitions", "ShownFrames", "KiHunterAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["KihunterGreen"] = KiHunterBody,
                ["KihunterGreenWings"] = KiHunterWings,
                ["KihunterRed"] = KiHunterBody,
                ["KihunterRedWings"] = KiHunterWings,
                ["KihunterYellow"] = KiHunterBody,
                ["KihunterYellowWings"] = KiHunterWings,
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // A robot without power holds one of its unpowered poses; a powered robot stands unpowered until the area boss
        // falls, then runs every program of its set.
        new("WorkRobotBehavior", "WorkRobotFrameDefinitions", "ShownFrames", "WorkRobotAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Robot"] =
                [
                    "NoPowerNeutral", "NoPowerLeaningLeft", "NoPowerLeaningRight", "Initial", "FacingLeftWalkingForwards",
                    "FacingLeftHitWallMovingForwards", "FacingLeftShotSamusAhead", "FacingLeftShotSamusBehind", "FacingLeftShotLaserDownLeft",
                    "FacingLeftShotLaserLeft", "FacingLeftShotLaserUpLeft", "ApproachingFallRight", "FacingRightWalkingForwards",
                    "FacingRightHitWallMovingForwards", "FacingRightShotSamusAhead", "FacingRightShotSamusBehind", "FacingRightShotLaserDownRight",
                    "FacingRightShotLaserRight", "FacingRightShotLaserUpRight", "ApproachingFallLeft",
                ],
                ["RobotNoPower"] = ["NoPowerNeutral", "NoPowerLeaningLeft", "NoPowerLeaningRight"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // Each Torizo starts in its initial program and resumes after its sleep; its AI, shot reactions and falls start
        // the rest. Bomb Torizo blows up its gut and face and turns and lands with and without its face; Golden Torizo
        // turns, dodges and catches Super Missiles; both fall and die. Every branch and choice a program names is followed,
        // so Golden Torizo shows every pose but Bomb Torizo's dormant statue, which only Bomb Torizo's programs show.
        // Bomb Torizo's faceless trailing step, which only the sleep's resumption names, is its own.
        new("BombTorizoBehavior", "TorizoFrameDefinitions", "ShownFrames", "TorizoAnimations", "Programs", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["BombTorizo"] =
                [
                    "BombTorizoInitial0", "BombTorizoInitial0Resumed", "FacingLeftFacelessWalkingLeftLegMovingTrailing",
                    "FacingLeftFacelessWalkingLeftLegMovingTrailingResumed", "SpecialCallableBlowUpBombTorizosGut",
                    "CallableBlowUpBombTorizosFace", "FacingLeftFacelessTurningLeft", "FacingRightFacelessTurningRight",
                    "FacingLeftTurningLeft", "FacingRightTurningRight", "FacingLeftFalling0", "FacingRightFalling0", "DeathSequence0",
                ],
                ["GoldenTorizo"] =
                [
                    "GoldenInitial0", "GoldenInitial0Resumed", "GoldenTurningLeft", "GoldenTurningRight", "GoldenDodgeTurningLeft",
                    "GoldenDodgeTurningRight", "GoldenCaughtSuperFacingLeftLeftLegFwd", "GoldenCaughtSuperFacingLeftRightLegFwd",
                    "GoldenCaughtSuperFacingRightLeftLegFwd", "GoldenCaughtSuperFacingRightRightLegFwd", "FacingLeftFalling0",
                    "FacingRightFalling0", "DeathSequence0",
                ],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, [])
        {
            StepHelperTypes = ["TorizoProgramSteps"],
        },
        // Ridley's body poses: Lower Norfair Ridley's programs stand, roar, lift off, breathe fire, turn through the
        // forward pose and roar as he dies; Ceres Ridley's stand, roar, lift off, lunge, take the Baby and breathe fire,
        // never turning; his death breakup runs no body program.
        new("RidleyEntityFactory", "RidleyBodyFrameDefinitions", "ShownFrames", null, null, null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal),
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Ridley"] = [.. RidleyBodyFrames, "BodyFrame=Forward"],
                ["RidleyCeres"] = RidleyBodyFrames,
                ["RidleyExplosion"] = [],
            },
            null, null, []),
    ];

    /// <summary>A hopper variant's four programs, as <c>HopperAnimationDefinitions.Programs</c> names them.</summary>
    private static string[] HopperVariant(string variant) =>
        [$"{variant}FloorJumping", $"{variant}FloorLanded", $"{variant}CeilingJumping", $"{variant}CeilingLanded"];

    /// <summary>The programs a Ki-Hunter's body is started in.</summary>
    private static string[] KiHunterBody =>
    [
        "FlyingLeft", "FlyingRight", "SwoopLeft", "SwoopRight", "JumpLeft", "JumpRight", "LandLeft", "LandRight", "SpitLeft", "SpitRight",
    ];

    /// <summary>The programs a Ki-Hunter's wings are started in.</summary>
    private static string[] KiHunterWings => ["WingsLeft", "WingsRight", "DetachedWings"];

    /// <summary>Ridley's body poses of both facings that both Ridleys show: standing, mouth and legs.</summary>
    private static string[] RidleyBodyFrames =>
    [
        "BodyFrame=Left", "BodyFrame=LeftMouthHalfOpen", "BodyFrame=LeftMouthOpen", "BodyFrame=LeftLegsHalfExtended", "BodyFrame=LeftLegsExtended",
        "BodyFrame=Right", "BodyFrame=RightMouthHalfOpen", "BodyFrame=RightMouthOpen", "BodyFrame=RightLegsHalfExtended",
        "BodyFrame=RightLegsExtended",
    ];

    /// <summary>A crawler family's four surface programs, as <c>CrawlerAnimationDefinitions.Families</c> names them.</summary>
    private static string[] CrawlerFamily(string family) =>
        [$"{family}UpsideRight", $"{family}UpsideLeft", $"{family}UpsideDown", $"{family}UpsideUp"];
}

/// <summary>
/// A population or header parameter an entry program choice follows from (#1513): every room placement of
/// <paramref name="Definition"/> carries <paramref name="Settings"/> whose <paramref name="Parameter"/> is one of
/// <paramref name="Values"/>. A compiled header (<c>RoomEnemyDefinitionEntry(definition, new RoomEnemyDefinition(...))</c>)
/// has the shape of a placement, so a header parameter such as a hopper's variant is checked the same way.
/// </summary>
/// <param name="Definition">The definition placed.</param>
/// <param name="Settings">The spawn-settings record its placements carry, or <c>RoomEnemyDefinition</c> for its header.</param>
/// <param name="Parameter">The record's parameter the choice follows from.</param>
/// <param name="Values">The parameter values the configured programs follow from: constants, or enum member names.</param>
internal sealed record PopulationChoice(string Definition, string Settings, string Parameter, string[] Values);

/// <summary>
/// Evaluates a <see cref="PartFrameConfiguration"/> over Core's source: the frame table's entries by key, the program set's
/// shown keys and continuations (inlining the program set's own step helpers, as <c>Show(int, int)</c>), each definition's
/// closure, and the self-checks that no program Core names and no constant key Core looks up is left out of every part.
/// </summary>
/// <param name="model">The Core facts.</param>
/// <param name="configuration">The behaviour's configuration.</param>
internal sealed class PartFrames(CoreModel model, PartFrameConfiguration configuration)
{
    /// <summary>One program's shown keys, continuations in its own slot, and programs it starts in other definitions' slots.</summary>
    private sealed record PartProgram(HashSet<string> Keys, HashSet<string> Continuations, List<(string Definition, string Program)> Starts);

    /// <summary>
    /// <paramref name="split"/>'s frames with each configured definition's table frames replaced by its own: its frames
    /// outside the table, with the table frames its programs or selectors show, less the frames drawn on another layer;
    /// a definition left with no frames is left out.
    /// </summary>
    /// <exception cref="InvalidDataException">The source does not have the configured shape, or a self-check fails.</exception>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> Apply(Behaviour behaviour, IReadOnlyDictionary<string, IReadOnlySet<string>> split)
    {
        var problems = new List<string>();
        Dictionary<string, string> table = Table(problems);
        HashSet<string> excluded = Excluded();
        Dictionary<string, HashSet<string>> keys = configuration.ProgramsType is null ? SelectedKeys(table, problems) : ProgramKeys(table, problems);
        foreach (string definition in keys.Keys.Where(definition => !behaviour.Definitions.Contains(definition)))
            problems.Add($"{definition} is not a definition {behaviour.Type.Name} serves");
        foreach (string definition in behaviour.Definitions.Where(definition => !keys.ContainsKey(definition)))
            problems.Add($"{definition} has no configured programs or selectors");
        CheckConstantKeys(table, keys, problems);
        CheckPopulations(behaviour, problems);
        var tableFrames = new HashSet<string>(table.Values, StringComparer.Ordinal);
        CheckCodeFrames(behaviour, split, tableFrames, problems);
        if (problems.Count > 0)
            throw new InvalidDataException($"{behaviour.Type.Name}'s part frames cannot be split:\n" + string.Join('\n', problems));

        var result = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);
        foreach (string definition in behaviour.Definitions)
        {
            var frames = new HashSet<string>(split[definition].Where(frame => !tableFrames.Contains(frame)
                && (!configuration.CodeFrames.TryGetValue(frame, out string[]? owners) || owners.Contains(definition, StringComparer.Ordinal))),
                StringComparer.Ordinal);
            frames.UnionWith(keys[definition].Select(key => table[key]));
            frames.ExceptWith(excluded);
            if (frames.Count > 0)
                result.Add(definition, frames);
        }
        return result;
    }

    /// <summary>
    /// The self-check that each configured code frame is a frame outside the table that the walk gives each of its owning
    /// definitions, so the configuration names only frames the behaviour's code sets.
    /// </summary>
    private void CheckCodeFrames(Behaviour behaviour, IReadOnlyDictionary<string, IReadOnlySet<string>> split, HashSet<string> tableFrames, List<string> problems)
    {
        foreach ((string frame, string[] owners) in configuration.CodeFrames)
        {
            if (tableFrames.Contains(frame))
                problems.Add($"code frame {frame} is a frame of {configuration.TableType}.{configuration.TableMember}");
            if (owners.Length == 0)
                problems.Add($"code frame {frame} names no definition");
            foreach (string owner in owners)
            {
                if (!split.TryGetValue(owner, out IReadOnlySet<string>? frames))
                    problems.Add($"code frame {frame} names {owner}, which {behaviour.Type.Name} does not serve");
                else if (!frames.Contains(frame))
                    problems.Add($"code frame {frame} is not a frame {owner}'s code reaches");
            }
        }
    }

    /// <summary>The frame table's entries, by key.</summary>
    private Dictionary<string, string> Table(List<string> problems)
    {
        ExpressionSyntax initializer = Initializer(configuration.TableType, configuration.TableMember);
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (InitializerExpressionSyntax entry in initializer.DescendantNodes().OfType<InitializerExpressionSyntax>()
                     .Where(node => node.Expressions.Count == 2 && node.Parent is InitializerExpressionSyntax))
        {
            SemanticModel semantic = model.ModelOf(entry.SyntaxTree);
            string? key = KeyOf(semantic, entry.Expressions[0], arguments: null);
            if (key is null || semantic.GetSymbolInfo(entry.Expressions[1]).Symbol is not IFieldSymbol frame
                || !SymbolEqualityComparer.Default.Equals(frame.ContainingType, model.EnemyFrameId))
            {
                problems.Add($"{configuration.TableType}.{configuration.TableMember} entry {entry} is not a constant key and an EnemyFrameId member");
                continue;
            }
            if (!entries.TryAdd(key, frame.Name))
                problems.Add($"{configuration.TableType}.{configuration.TableMember} lists key {key} twice");
        }
        if (entries.Count == 0)
            problems.Add($"{configuration.TableType}.{configuration.TableMember} has no entries");
        return entries;
    }

    /// <summary>The frames drawn on another layer than the sprites.</summary>
    private HashSet<string> Excluded()
    {
        if (configuration.ExcludedType is null || configuration.ExcludedMember is null)
            return new HashSet<string>(StringComparer.Ordinal);
        return model.EnemyFramesNamed(Initializer(configuration.ExcludedType, configuration.ExcludedMember));
    }

    /// <summary>Each definition's keys by its selectors, every key's selector carried by some definition.</summary>
    private Dictionary<string, HashSet<string>> SelectedKeys(Dictionary<string, string> table, List<string> problems)
    {
        var keys = configuration.KeySelectors.ToDictionary(pair => pair.Key,
            pair => new HashSet<string>(table.Keys.Where(key => pair.Value.Any(selector => KeyParts(key).Contains(selector))), StringComparer.Ordinal),
            StringComparer.Ordinal);
        foreach (string key in table.Keys.Where(key => !keys.Values.Any(selected => selected.Contains(key))))
            problems.Add($"table key {key} carries no part's selector");
        return keys;
    }

    /// <summary>Each definition's keys by the closure of the programs its slot is started in.</summary>
    private Dictionary<string, HashSet<string>> ProgramKeys(Dictionary<string, string> table, List<string> problems)
    {
        (Dictionary<string, PartProgram> programs, INamedTypeSymbol programType, IReadOnlyList<ExpressionSyntax> initializers) = Programs(problems);
        var keys = configuration.EntryPrograms.Keys.Concat(configuration.StartCueDefinitions).Distinct(StringComparer.Ordinal)
            .ToDictionary(definition => definition, _ => new HashSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);
        HashSet<(string Definition, string Program)> reached = Closure(programs, problems);
        foreach ((string definition, string reachedProgram) in reached)
        {
            foreach (string key in programs[reachedProgram].Keys)
            {
                if (!table.ContainsKey(key))
                    problems.Add($"program {reachedProgram} shows key {key}, which {configuration.TableType}.{configuration.TableMember} lacks");
                keys[definition].Add(key);
            }
        }
        // The self-check: every program Core names outside the program set lies in some part's closure, this behaviour's
        // or another configured behaviour's running the same program set, so no start the configuration missed leaves a
        // part without the frames it shows.
        var inClosure = new HashSet<string>(reached.Select(pair => pair.Program), StringComparer.Ordinal);
        foreach (PartFrameConfiguration sibling in PartFrameConfiguration.All.Where(other => other.Behaviour != configuration.Behaviour
                     && other.ProgramsType == configuration.ProgramsType && other.ProgramsMember == configuration.ProgramsMember))
        {
            // The sibling reports its own problems when its behaviour is split; here only its closure counts.
            var siblingProblems = new List<string>();
            PartFrames siblingFrames = new(model, sibling);
            (Dictionary<string, PartProgram> siblingPrograms, _, _) = siblingFrames.Programs(siblingProblems);
            inClosure.UnionWith(siblingFrames.Closure(siblingPrograms, siblingProblems).Select(pair => pair.Program));
        }
        foreach (string named in NamedOutside(programType, initializers).Where(program => !inClosure.Contains(program)))
            problems.Add($"{programType.Name}.{named} is named outside the program set but no part's closure reaches it");
        return keys;
    }

    /// <summary>
    /// Every program each definition's slot runs, as definition and program: the closure of its entry programs over the
    /// programs they continue into and start in other definitions' slots.
    /// </summary>
    private HashSet<(string Definition, string Program)> Closure(Dictionary<string, PartProgram> programs, List<string> problems)
    {
        var reached = new HashSet<(string Definition, string Program)>();
        var pending = new Queue<(string Definition, string Program)>(
            configuration.EntryPrograms.SelectMany(pair => pair.Value.Select(program => (pair.Key, program))));
        while (pending.TryDequeue(out (string Definition, string Program) next))
        {
            if (reached.Contains(next))
                continue;
            if (!programs.TryGetValue(next.Program, out PartProgram? program))
            {
                problems.Add($"{next.Definition}'s program {next.Program} is not in {configuration.ProgramsType}.{configuration.ProgramsMember}");
                continue;
            }
            reached.Add(next);
            foreach (string continuation in program.Continuations)
                pending.Enqueue((next.Definition, continuation));
            foreach ((string definition, string started) in program.Starts)
                pending.Enqueue((definition, started));
        }
        return reached;
    }

    /// <summary>
    /// The self-check that each population-chosen program set is the one the rooms choose: every placement of a configured
    /// definition carries spawn settings whose configured parameter is one of the configured values, and some placement
    /// carries them.
    /// </summary>
    private void CheckPopulations(Behaviour behaviour, List<string> problems)
    {
        foreach (PopulationChoice choice in configuration.PopulationChoices)
        {
            if (!behaviour.Definitions.Contains(choice.Definition))
                problems.Add($"population choice for {choice.Definition}, which {behaviour.Type.Name} does not serve");
            int placements = 0;
            foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
            {
                SemanticModel semantic = model.ModelOf(tree);
                foreach (BaseObjectCreationExpressionSyntax settings in tree.GetRoot().DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>())
                {
                    if (semantic.GetSymbolInfo(settings).Symbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
                        || constructor.ContainingType.Name != choice.Settings
                        || settings.Parent is not ArgumentSyntax { Parent: ArgumentListSyntax { Parent: BaseObjectCreationExpressionSyntax placement } }
                        || placement.ArgumentList is not { Arguments: [var first, ..] }
                        || semantic.GetSymbolInfo(first.Expression).Symbol is not IFieldSymbol placed
                        || !SymbolEqualityComparer.Default.Equals(placed.ContainingType, model.EnemyDefinitionId)
                        || placed.Name != choice.Definition)
                        continue;
                    placements++;
                    string? value = ParameterConstant(semantic, settings, constructor, choice.Parameter);
                    if (value is null || !choice.Values.Contains(value, StringComparer.Ordinal))
                    {
                        problems.Add($"{model.RelativePath(tree)}: a {choice.Definition} placement's {choice.Settings}.{choice.Parameter} is "
                            + $"{value ?? "not a constant"}, not one of {string.Join(", ", choice.Values)} its configured programs follow from");
                    }
                }
            }
            if (placements == 0)
                problems.Add($"no room places {choice.Definition} with {choice.Settings}, so its configured choice is unchecked");
        }
    }

    /// <summary>The constant the creation <paramref name="settings"/> passes for <paramref name="constructor"/>'s parameter <paramref name="parameter"/>, or null.</summary>
    private static string? ParameterConstant(SemanticModel semantic, BaseObjectCreationExpressionSyntax settings, IMethodSymbol constructor, string parameter)
    {
        if (settings.ArgumentList is not { } arguments)
            return null;
        for (int index = 0; index < arguments.Arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments.Arguments[index];
            string? name = argument.NameColon is { } named ? named.Name.Identifier.Text
                : index < constructor.Parameters.Length ? constructor.Parameters[index].Name : null;
            if (name == parameter)
                return ConstantOf(semantic, argument.Expression);
        }
        return null;
    }

    /// <summary>
    /// The program set's programs by name, with the program identity type and the initializers they are read from: the
    /// set's own, and those of the program tables it spreads in.
    /// </summary>
    private (Dictionary<string, PartProgram>, INamedTypeSymbol, IReadOnlyList<ExpressionSyntax>) Programs(List<string> problems)
    {
        List<ExpressionSyntax> initializers = ProgramInitializers();
        var programs = new Dictionary<string, PartProgram>(StringComparer.Ordinal);
        INamedTypeSymbol? programType = null;
        foreach (ExpressionSyntax initializer in initializers)
            programType = ReadPrograms(initializer, programs, programType, problems);
        if (programType is null)
            throw new InvalidDataException($"{configuration.ProgramsType}.{configuration.ProgramsMember} has no [Program] = [steps] entries.");
        return (programs, programType, initializers);
    }

    /// <summary>
    /// The program set's initializer, followed by the initializer of every program table it spreads in, in the order
    /// spread, as <c>TorizoAnimations.Programs</c> joins four tables of programs.
    /// </summary>
    private List<ExpressionSyntax> ProgramInitializers()
    {
        var initializers = new List<ExpressionSyntax>();
        var pending = new Queue<ExpressionSyntax>([Initializer(configuration.ProgramsType!, configuration.ProgramsMember!)]);
        while (pending.TryDequeue(out ExpressionSyntax? initializer))
        {
            initializers.Add(initializer);
            SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
            foreach (SpreadElementSyntax spread in initializer.DescendantNodes().OfType<SpreadElementSyntax>())
            {
                if (semantic.GetTypeInfo(spread.Expression).Type is INamedTypeSymbol { Name: "Dictionary" or "IReadOnlyDictionary" }
                    && semantic.GetSymbolInfo(spread.Expression).Symbol is IPropertySymbol or IFieldSymbol)
                {
                    ISymbol table = semantic.GetSymbolInfo(spread.Expression).Symbol ?? throw new InvalidDataException($"{spread} names no member.");
                    pending.Enqueue(Initializer(table.ContainingType.Name, table.Name));
                }
            }
        }
        return initializers;
    }

    /// <summary>
    /// Reads the <c>[P.X] = [steps]</c> entries of <paramref name="initializer"/> into <paramref name="programs"/>, and
    /// returns the program identity type.
    /// </summary>
    private INamedTypeSymbol? ReadPrograms(ExpressionSyntax initializer, Dictionary<string, PartProgram> programs, INamedTypeSymbol? programType,
        List<string> problems)
    {
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        foreach (AssignmentExpressionSyntax entry in initializer.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (entry is not { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var name] }, Right: CollectionExpressionSyntax steps }
                || semantic.GetSymbolInfo(name.Expression).Symbol is not IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } identity)
                continue;
            programType ??= identity.ContainingType;
            var program = new PartProgram(new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal), []);
            foreach (CollectionElementSyntax element in steps.Elements)
            {
                if (element is not ExpressionElementSyntax { Expression: var step })
                {
                    problems.Add($"program {identity.Name} has a step this generator cannot read: {element}");
                    continue;
                }
                ReadStep(semantic, step, identity.ContainingType, program, problems);
            }
            if (!programs.TryAdd(identity.Name, program))
                problems.Add($"program {identity.Name} is listed twice");
        }
        return programType;
    }

    /// <summary>Reads one program step: the keys it shows, the programs it continues into, the programs its start cue starts.</summary>
    private void ReadStep(SemanticModel semantic, ExpressionSyntax step, INamedTypeSymbol programType, PartProgram program, List<string> problems)
    {
        var startArguments = new HashSet<SyntaxNode>();
        foreach (BaseObjectCreationExpressionSyntax creation in step.DescendantNodesAndSelf().OfType<BaseObjectCreationExpressionSyntax>())
        {
            if (configuration.StartCue is null || semantic.GetSymbolInfo(creation).Symbol is not IMethodSymbol { ContainingType.Name: var created }
                || created != configuration.StartCue || creation.ArgumentList is not { } arguments)
                continue;
            if (arguments.Arguments.Count != configuration.StartCueDefinitions.Count)
            {
                problems.Add($"{configuration.StartCue} at {step} does not start one program for each of {string.Join(", ", configuration.StartCueDefinitions)}");
                continue;
            }
            for (int index = 0; index < arguments.Arguments.Count; index++)
            {
                startArguments.Add(arguments.Arguments[index]);
                if (semantic.GetSymbolInfo(arguments.Arguments[index].Expression).Symbol is IFieldSymbol started
                    && SymbolEqualityComparer.Default.Equals(started.ContainingType, programType))
                    program.Starts.Add((configuration.StartCueDefinitions[index], started.Name));
                else
                    problems.Add($"{configuration.StartCue} at {step} starts a program that is not a {programType.Name} member");
            }
        }
        // Any other program the step names is one its own slot runs on into (a go-to, a fall-through, a counted repeat).
        foreach (IdentifierNameSyntax name in step.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            if (!name.Ancestors().Any(startArguments.Contains)
                && semantic.GetSymbolInfo(name).Symbol is IFieldSymbol continuation
                && SymbolEqualityComparer.Default.Equals(continuation.ContainingType, programType))
                program.Continuations.Add(continuation.Name);
        }
        // A step shows a key either by constructing it or through one of the program set's own helpers.
        foreach (InvocationExpressionSyntax invocation in step.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            if (semantic.GetSymbolInfo(invocation).Symbol is not IMethodSymbol helper
                || (helper.ContainingType.Name != configuration.ProgramsType && !configuration.StepHelperTypes.Contains(helper.ContainingType.Name)))
                continue;
            foreach (string key in HelperKeys(semantic, invocation, helper))
                program.Keys.Add(key);
        }
        foreach (BaseObjectCreationExpressionSyntax creation in step.DescendantNodesAndSelf().OfType<BaseObjectCreationExpressionSyntax>())
        {
            if (IsKeyCreation(semantic, creation) && KeyOf(semantic, creation, arguments: null) is { } key)
                program.Keys.Add(key);
        }
    }

    /// <summary>The keys a call of a program set helper shows: the key its body constructs, with the call's arguments in place of its parameters.</summary>
    private IEnumerable<string> HelperKeys(SemanticModel callSite, InvocationExpressionSyntax invocation, IMethodSymbol helper)
    {
        var arguments = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int index = 0; index < invocation.ArgumentList.Arguments.Count && index < helper.Parameters.Length; index++)
        {
            if (ConstantOf(callSite, invocation.ArgumentList.Arguments[index].Expression) is { } value)
                arguments[helper.Parameters[index].Name] = value;
        }
        foreach (SyntaxReference reference in helper.DeclaringSyntaxReferences)
        {
            SyntaxNode declaration = reference.GetSyntax();
            SemanticModel semantic = model.ModelOf(declaration.SyntaxTree);
            foreach (BaseObjectCreationExpressionSyntax creation in declaration.DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>())
            {
                if (IsKeyCreation(semantic, creation) && KeyOf(semantic, creation, arguments) is { } key)
                    yield return key;
            }
        }
    }

    /// <summary>Whether <paramref name="creation"/> constructs the frame table's key type.</summary>
    private bool IsKeyCreation(SemanticModel semantic, BaseObjectCreationExpressionSyntax creation) =>
        semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
        && SymbolEqualityComparer.Default.Equals(constructor.ContainingType, KeyType());

    /// <summary>The frame table's key type: the type its entries' keys construct.</summary>
    private INamedTypeSymbol KeyType()
    {
        if (_keyType is not null)
            return _keyType;
        ExpressionSyntax initializer = Initializer(configuration.TableType, configuration.TableMember);
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        // A key constructs its type, or is a member of its enum type.
        _keyType = initializer.DescendantNodes().OfType<InitializerExpressionSyntax>()
            .Where(node => node.Expressions.Count == 2 && node.Parent is InitializerExpressionSyntax)
            .Select(node => semantic.GetSymbolInfo(node.Expressions[0]).Symbol)
            .Select(symbol => symbol switch
            {
                IMethodSymbol constructor => constructor.ContainingType.OriginalDefinition,
                IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member => member.ContainingType,
                _ => null,
            })
            .FirstOrDefault(type => type is not null)
            ?? throw new InvalidDataException($"{configuration.TableType}.{configuration.TableMember}'s keys construct no type and name no enum member.");
        return _keyType;
    }

    private INamedTypeSymbol? _keyType;

    /// <summary>
    /// The key <paramref name="expression"/> constructs, as <c>Parameter=Value</c> pairs in parameter order, its arguments
    /// constants or, inside a helper, the helper's parameters bound in <paramref name="arguments"/>; null when any is neither.
    /// A key that is an enum member is <c>EnumType=Member</c>.
    /// </summary>
    private static string? KeyOf(SemanticModel semantic, ExpressionSyntax expression, IReadOnlyDictionary<string, string>? arguments)
    {
        if (expression is not BaseObjectCreationExpressionSyntax
            && semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member)
            return $"{member.ContainingType.Name}={member.Name}";
        if (expression is not BaseObjectCreationExpressionSyntax { ArgumentList: { } list } creation
            || semantic.GetSymbolInfo(creation).Symbol is not IMethodSymbol constructor)
            return null;
        var values = new string?[constructor.Parameters.Length];
        for (int index = 0; index < list.Arguments.Count; index++)
        {
            ArgumentSyntax argument = list.Arguments[index];
            int position = argument.NameColon is { } name
                ? constructor.Parameters.IndexOf(constructor.Parameters.FirstOrDefault(p => p.Name == name.Name.Identifier.Text)!)
                : index;
            if (position < 0 || position >= values.Length)
                return null;
            values[position] = ConstantOf(semantic, argument.Expression)
                ?? (semantic.GetSymbolInfo(argument.Expression).Symbol is IParameterSymbol parameter && arguments?.GetValueOrDefault(parameter.Name) is { } bound
                    ? bound : null);
            if (values[position] is null)
                return null;
        }
        if (values.Any(value => value is null))
            return null;
        return string.Join(',', constructor.Parameters.Select((parameter, index) => $"{parameter.Name}={values[index]}"));
    }

    /// <summary>The constant <paramref name="expression"/> is: a literal or constant's value, or an enum member's name.</summary>
    private static string? ConstantOf(SemanticModel semantic, ExpressionSyntax expression) =>
        semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member
            ? member.Name
            : semantic.GetConstantValue(expression) is { HasValue: true, Value: { } value } ? Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) : null;

    /// <summary>The <c>Parameter=Value</c> pairs of <paramref name="key"/>.</summary>
    private static string[] KeyParts(string key) => key.Split(',');

    /// <summary>
    /// The self-check: every key Core constructs from constants outside the frame table and program set is shown by some
    /// part, so a pose code sets directly is on that part's sheet.
    /// </summary>
    private void CheckConstantKeys(Dictionary<string, string> table, Dictionary<string, HashSet<string>> keys, List<string> problems)
    {
        INamedTypeSymbol keyType = KeyType();
        SyntaxNode tableNode = Initializer(configuration.TableType, configuration.TableMember);
        List<ExpressionSyntax> programsNodes = configuration.ProgramsType is null ? [] : ProgramInitializers();
        var shown = new HashSet<string>(keys.Values.SelectMany(selected => selected), StringComparer.Ordinal);
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel semantic = model.ModelOf(tree);
            foreach (BaseObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>())
            {
                if (creation.Ancestors().Any(node => node == tableNode || node is ExpressionSyntax expression && programsNodes.Contains(expression))
                    || semantic.GetSymbolInfo(creation).Symbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
                    || !SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, keyType)
                    || KeyOf(semantic, creation, arguments: null) is not { } key)
                    continue;
                if (!table.ContainsKey(key))
                    problems.Add($"{model.RelativePath(tree)}: key {key} has no entry in {configuration.TableType}.{configuration.TableMember}");
                else if (!shown.Contains(key))
                    problems.Add($"{model.RelativePath(tree)}: key {key} is looked up but no part shows it");
            }
        }
    }

    /// <summary>The members of <paramref name="programType"/> Core names outside the program set's initializers.</summary>
    private IEnumerable<string> NamedOutside(INamedTypeSymbol programType, IReadOnlyList<ExpressionSyntax> initializers)
    {
        var named = new SortedSet<string>(StringComparer.Ordinal);
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel semantic = model.ModelOf(tree);
            foreach (IdentifierNameSyntax name in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (name.Ancestors().Any(node => node is ExpressionSyntax expression && initializers.Contains(expression)
                        || node is AttributeListSyntax or CaseSwitchLabelSyntax)
                    || name.Ancestors().OfType<InvocationExpressionSyntax>().Any(invocation => invocation.Expression is IdentifierNameSyntax { Identifier.Text: "nameof" })
                    || semantic.GetSymbolInfo(name).Symbol is not IFieldSymbol field
                    || !SymbolEqualityComparer.Default.Equals(field.ContainingType, programType))
                    continue;
                named.Add(field.Name);
            }
        }
        return named;
    }

    /// <summary>The initializer value of <paramref name="type"/>'s property or field <paramref name="member"/>.</summary>
    private ExpressionSyntax Initializer(string type, string member) => model.MemberInitializer(type, member);
}
