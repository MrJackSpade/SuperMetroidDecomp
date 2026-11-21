using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// How a multi-part behaviour's frames are split between its parts (#1513). Each part slot runs shared part-animation
/// code that reads all its segments or its whole frame table, so the walk alone would give every part every frame; instead
/// each part's frames are those its own programs show (the closure of the animations each part's slot is started in) or
/// those whose table keys carry the part's selectors.
/// </summary>
/// <param name="Behaviour">The behaviour type's name.</param>
/// <param name="TableType">
/// The type declaring the frame table the segments' poses are looked up in, or null when the segments name the frames they
/// show (#1623).
/// </param>
/// <param name="TableMember">
/// The frame table property: <c>{ key, EnemyFrameId.X }</c> entries, or a list of frames, each keyed as the member it is
/// (<c>EnemyFrameId=X</c>, #2234); null with no table.
/// </param>
/// <param name="AnimationsType">
/// The type declaring the parts' <c>Segments</c> (<c>[P.X] = [beats]</c> entries, or segments joining tables it spreads in,
/// as Torizo's join three, whose entries are read from those tables) and <c>StartingAt</c> (the animation starting at each
/// segment, <c>[P.X] = new([phases], end)</c>), or null when parts select by key.
/// </param>
/// <param name="StartCue">The cue record starting every part's program at once, or null.</param>
/// <param name="StartCueDefinitions">The definition each of the start cue's positional programs is started for.</param>
/// <param name="EntryPrograms">The programs (the segments animations start at) code starts each definition's slot in, by definition.</param>
/// <param name="KeySelectors">
/// For a table split by key, the <c>Parameter=Value</c> selectors each definition's keys carry; an enum key is
/// <c>EnumType=Member</c>. A selector naming only the parameter (or the enum type) selects every key carrying it.
/// </param>
/// <param name="ExcludedType">The type declaring frames drawn on another layer than the sprites, or null.</param>
/// <param name="ExcludedMember">That type's set of those frames.</param>
/// <param name="PopulationChoices">
/// The population or header parameters an entry program choice follows from, each checked against every room placement
/// or compiled header of its definition.
/// </param>
internal sealed record PartFrameConfiguration(
    string Behaviour, string? TableType, string? TableMember, string? AnimationsType, string? StartCue,
    IReadOnlyList<string> StartCueDefinitions, IReadOnlyDictionary<string, string[]> EntryPrograms,
    IReadOnlyDictionary<string, string[]> KeySelectors, string? ExcludedType, string? ExcludedMember,
    IReadOnlyList<PopulationChoice> PopulationChoices)
{
    /// <summary>
    /// The types besides <see cref="AnimationsType"/> whose beat helpers the segments call, as Torizo's segments call
    /// <c>TorizoProgramSteps</c>' helpers.
    /// </summary>
    public IReadOnlyList<string> StepHelperTypes { get; init; } = [];

    /// <summary>
    /// Frames outside the table that only the named definitions' own code sets, by frame: a reaction that pins a frozen
    /// frame for its own definitions only. Every other definition of the behaviour is left without them.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> CodeFrames { get; init; } = new Dictionary<string, string[]>(StringComparer.Ordinal);

    /// <summary>
    /// The catalog list (<c>Type.Member</c>) naming the frames each named definition shows, for a behaviour whose parts'
    /// animations name the frames they show (#2210): every frame such a list names is that definition's alone, and every
    /// other definition of the behaviour keeps the walk's other frames. Empty for a behaviour split by table or programs.
    /// </summary>
    public IReadOnlyDictionary<string, string> FrameLists { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// For a family whose segments and animations are written once over the side a placement faces (#2557: the Ceres doors,
    /// which show each facing's pictures through one set of segments), the animations type's members holding each side's
    /// segments and animations: dictionaries keyed by the side, each value built by a helper of the animations type called
    /// with the side's poses. Each side's programs are named <c>Program@Side</c>; null for a family written once.
    /// </summary>
    public (string Segments, string StartingAt)? Sided { get; init; }

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
        new("DraygonBehavior", null, null, "DraygonAnimations", "StartPartAnimations",
            ["DraygonBody", "DraygonEye", "DraygonTail", "DraygonArms"],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["DraygonBody"] =
                [
                    "BodyFacingLeftIdle", "BodyFacingLeftReset", "BodyFacingRightReset", "BodyFacingLeftFireGoop", "BodyFacingRightFireGoop",
                    "BodyFacingLeftRoar", "BodyFacingRightRoar", "BodyFacingLeftDying", "BodyFacingRightDying", "Rest", "Delete",
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
                    "TailFacingRightFinalWhips", "TailFacingLeftFlail", "TailFacingRightFlail", "Rest",
                ],
                ["DraygonArms"] =
                [
                    "ArmsFacingLeftIdle", "ArmsFacingRightIdle", "ArmsFacingLeftNearSwoopApex", "ArmsFacingRightNearSwoopApex",
                    "ArmsFacingLeftGrab", "ArmsFacingRightGrab", "Rest",
                ],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), "DraygonBodyLayerFrameDefinitions", "Frames", []),
        // Kraid's parts show the frames their own animations name (#1885): the arm its swings, the foot its stances, walks
        // and lunge, the lints their two poses and the fingernails their spin; Kraid himself none.
        new("KraidBehavior", null, null, "KraidAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Kraid"] = [],
                ["KraidArm"] = ["ArmNormal", "ArmNormalPause", "ArmSlow", "ArmRisingOrSinking", "ArmDyingOrPreparingToLunge"],
                ["KraidFoot"] = ["FootInitial", "FootNeutral", "FootWalkingForward", "FootLungeForward", "FootWalkingBackward"],
                ["KraidLintTop"] = ["LintInitial", "LintPostGrowth"],
                ["KraidLintMiddle"] = ["LintInitial", "LintPostGrowth"],
                ["KraidLintBottom"] = ["LintInitial", "LintPostGrowth"],
                ["KraidFirstNail"] = ["NailLoop"],
                ["KraidSecondNail"] = ["NailLoop"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // Mother Brain's body frames are her body's and her brain frames her head's (#2210): her animations name them, the
        // catalogs list them; her tubes draw none of them.
        new("MotherBrainBehavior", null, null, null, null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal), new Dictionary<string, string[]>(StringComparer.Ordinal),
            null, null, [])
        {
            FrameLists = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["MotherBrainBody"] = "MotherBrainBodyVisualDefinitions.AnimationFrames",
                ["MotherBrainHead"] = "MotherBrainVisualDefinitions.HeadFrames",
            },
        },
        // A vertical shutter runs its fully grown program, Kamer its platform loop; a growing shutter grows through its
        // three growth programs to the fully grown one.
        new("ShuttersBehavior", null, null, "ShutterAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Kamer"] = ["KamerPlatform"],
                ["ShutterDestroyable"] = ["FullyGrown"],
                ["ShutterShootable"] = ["FullyGrown"],
                ["ShutterGrowing"] = ["QuarterGrown", "HalfGrown", "ThreeQuartersGrown", "FullyGrown"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // Each crawler starts in its family's surface program and takes its later surfaces from its species' own family:
        // Sciser, Zero and Viola their own; the spiny crawlers, the orange Zoomer among them, crawl each surface by the one
        // spiny crawl (#2283).
        new("CrawlersBehavior", null, null, "CrawlerAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Sciser"] = CrawlerFamily("Sciser"),
                ["Zero"] = CrawlerFamily("Zero"),
                // The Viola crawls the same on every surface (#2061).
                ["Viola"] = ["ViolaIdle"],
                ["Zeela"] = CrawlerFamily("SpinyCrawler"),
                ["Sova"] = CrawlerFamily("SpinyCrawler"),
                ["Zoomer"] = CrawlerFamily("SpinyCrawler"),
                ["ArmoredZoomer"] = CrawlerFamily("SpinyCrawler"),
                ["OrangeZoomer"] = CrawlerFamily("SpinyCrawler"),
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // A Brinstar pipe bug runs the weak or the strong programs its room's kind selects; Gamet and Geega their own.
        new("PipeBugsBehavior", null, null, "PipeBugAnimations", null, [],
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
        new("DeadSidehopperBehavior", null, null, "DeadTourianAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["CorpseSidehopper"] = ["SidehopperIdle", "SidehopperHopping", "SidehopperCorpse", "SidehopperInitiallyDead"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        new("DeadTourianBehavior", null, null, "DeadTourianAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["CorpseZoomer"] = ["ZoomerCorpseOnItsBack", "ZoomerCorpseUpright"],
                ["CorpseRipper"] = ["RipperCorpseLevel", "RipperCorpseTilted"],
                ["CorpseSkree"] = ["SkreeCorpse"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // The Skree and the Metaree are separate behaviours running one program set and frame table: each runs the three
        // animations its own AI starts (#1513, #1810).
        new("SkreeBehavior", null, null, "SkreeMetareeAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Skree"] = ["SkreeIdling", "SkreePreparingAttack", "SkreeDiving"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        new("MetareeBehavior", null, null, "SkreeMetareeAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Metaree"] = ["MetareeIdling", "MetareePreparingAttack", "MetareeDiving"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // A hopper runs the programs of the kind its definition is (HopperKindDefinitions: the Tourian Sidehopper is a large
        // Sidehopper), on the floor or the ceiling its placement mounts it on.
        new("HoppersBehavior", null, null, "HopperAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Dessgeega"] = HopperVariant("Dessgeega"),
                ["DessgeegaLarge"] = HopperVariant("LargeDessgeega"),
                ["Sidehopper"] = HopperVariant("Sidehopper"),
                ["SidehopperLarge"] = HopperVariant("LargeSidehopper"),
                ["SidehopperTourian"] = HopperVariant("LargeSidehopper"),
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // A Fune or Namihe runs its own species' idle and active programs, facing either way.
        new("FuneNamiheBehavior", null, null, "FuneNamiheAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Fune"] = ["FuneIdleLeft", "FuneActiveLeft", "FuneIdleRight", "FuneActiveRight"],
                ["Namihe"] = ["NamiheIdleLeft", "NamiheActiveLeft", "NamiheIdleRight", "NamiheActiveRight"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // Kamer runs the suspensor platform's programs and the Tripper its own, as the platform initializer chooses by
        // definition; only the Tripper's shot and bomb reactions pin its frozen frames.
        new("PlatformsBehavior", null, null, "PlatformAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["SuspensorPlatform"] = ["SuspensorPlatformHeadingLeft", "SuspensorPlatformHeadingRight"],
                ["Tripper"] = ["TripperMovingLeft", "TripperMovingRight", "TripperStillMovingLeft", "TripperStillMovingRight"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, [])
        {
            CodeFrames = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["TripperFrozenFacingLeft"] = ["Tripper"],
                ["TripperFrozenFacingRight"] = ["Tripper"],
            },
        },
        // An Evir placement is its body or its arms; the Evir projectile runs only the spit's programs.
        new("EvirBehavior", null, null, "EvirAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Evir"] = ["BodyFacingLeft", "BodyFacingRight", "ArmsFacingLeft", "ArmsFacingRight"],
                ["EvirProjectile"] = ["ProjectileIdle", "ProjectileRegenerating"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // The gunship's top is its top hull; its bottom is the bottom hull or the entrance pad, which opens and closes.
        new("GunshipBehavior", null, null, "GunshipAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["ShipBottomEntrance"] = ["BottomHull", "BottomEntrancePad", "EntrancePadOpening", "EntrancePadClosing"],
                ["ShipTop"] = ["TopHull"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // The Ceres doors and walls run the doors' one animation set, each from its own start (#1989): an ordinary door from
        // its facing's, which its placement chooses; the door of Ridley's room from its own, running on into the right-facing
        // door's; the elevator room's blocked door, which a room with a turning floor places (#2426), its wall (which runs on
        // into the left-facing door's) and the getaway chamber's walls each from theirs.
        // Each runs its animations on the side it faces (#2557): the elevator room's blocked door, the door of Ridley's room and
        // the getaway chamber's walls the right, the elevator room's wall the left, an ordinary door the side its placement
        // chooses.
        new("CeresElevatorDoorFactory", null, null, "CeresDoorAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["CeresElevatorDoor"] = ["ElevatorDoor@Right"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, [])
        {
            Sided = ("SegmentsBySide", "StartingAtBySide"),
        },
        new("CeresDoorFactory", null, null, "CeresDoorAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                // Each door's cues start, by name, the closing, closed and waiting animations of its facing (#2189); the
                // elevator room's wall starts the left-facing door's and then receives that door's cues.
                ["CeresDoor"] = ["Door@Right", "Door@Left", "Close@Right", "Close@Left", "Closed@Right", "Closed@Left",
                    "ClosedWait@Right", "ClosedWait@Left"],
                ["CeresRidleyRoomDoor"] = ["RidleyRoomDoor@Right", "RidleyRoomDoorWait@Right"],
                ["CeresElevatorRoomWall"] = ["ElevatorRoomWall@Left", "Door@Left", "Close@Left", "Closed@Left", "ClosedWait@Left"],
                ["CeresGetawayChamberWall"] = ["GetawayChamberLeftWall@Right", "GetawayChamberRightWall@Right"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null,
            [
                new("CeresDoor", "CeresDoorSpawnSettings", "Facing", ["Right", "Left"]),
            ])
        {
            Sided = ("SegmentsBySide", "StartingAtBySide"),
        },
        // The grapple Ripper and the Ripper run their own pair of moving programs and the jet Ripper plays the grapple Ripper's (#1842);
        // only the grapple Ripper's and jet Ripper's reactions pin their shared frozen frames.
        new("RipperBehavior", null, null, "RipperAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["GrappleRipper"] = ["GrappleRipperMovingLeft", "GrappleRipperMovingRight"],
                ["Ripper"] = ["RipperMovingLeft", "RipperMovingRight"],
                ["JetRipper"] = ["GrappleRipperMovingLeft", "GrappleRipperMovingRight"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, [])
        {
            CodeFrames = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["RipperFrozenFacingLeft"] = ["GrappleRipper", "JetRipper"],
                ["RipperFrozenFacingRight"] = ["GrappleRipper", "JetRipper"],
            },
        },
        // A Ki-Hunter's body flies, swoops, hops, lands and spits; its wings follow it, or fly off detached.
        new("KiHunterBehavior", null, null, "KiHunterAnimations", null, [],
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
        new("WorkRobotBehavior", null, null, "WorkRobotAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["WorkRobot"] =
                [
                    "NoPowerNeutral", "NoPowerLeaningLeft", "NoPowerLeaningRight", "StandingFacingLeft", "FacingLeftWalkingForwards",
                    "FacingLeftHitWallMovingForwards", "FacingLeftShotSamusAhead", "FacingLeftShotSamusBehind", "FacingLeftShotLaserDownLeft",
                    "FacingLeftShotLaserLeft", "FacingLeftShotLaserUpLeft", "ApproachingFallRight", "FacingRightWalkingForwards",
                    "FacingRightHitWallMovingForwards", "FacingRightShotSamusAhead", "FacingRightShotSamusBehind", "FacingRightShotLaserDownRight",
                    "FacingRightShotLaserRight", "FacingRightShotLaserUpRight", "ApproachingFallLeft",
                ],
                ["UnpoweredWorkRobot"] = ["NoPowerNeutral", "NoPowerLeaningLeft", "NoPowerLeaningRight"],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, []),
        // Each Torizo starts dormant and wakes after its sleep; its AI, shot reactions and falls start the rest. Bomb
        // Torizo blows up its gut and face and turns and lands with and without its face; Golden Torizo turns, dodges and
        // catches Super Missiles; both fall and die. A walking Torizo's decisions continue in the animation the owner picks
        // for the posture its step left (#2175, TorizoBranches): Bomb Torizo's jump back, Golden Torizo's jump back, leap
        // and sit, each way and onto either foot. Golden Torizo's owner settles it once its drop reaches the floor and closes
        // its chest once its eggs are gone (#2411). Every other branch and choice an animation names is followed, so Golden
        // Torizo shows every pose but Bomb Torizo's dormant statue, which only Bomb Torizo's animations show.
        new("TorizoBehavior", null, null, "TorizoAnimations", null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["BombTorizo"] =
                [
                    "BombTorizoDormant", "BombTorizoAwakening",
                    "BombTorizoGutBlowingUp", "BombTorizoFaceBlowingUp", "FacelessTurningLeft", "FacelessTurningRight",
                    "TurningLeft", "TurningRight", "FacingLeftFalling", "FacingRightFalling", "Dying",
                    .. TorizoJumpsBack,
                ],
                ["GoldenTorizo"] =
                [
                    "GoldenTorizoDormant", "GoldenTorizoAwakening", "GoldenTorizoSettling", "GoldenChestClosing", "GoldenTurningLeft",
                    "GoldenTurningRight", "GoldenDodgeTurningLeft",
                    "GoldenDodgeTurningRight", "GoldenFacingLeftCatchingSuperMissileLeftLegForward",
                    "GoldenFacingLeftCatchingSuperMissileRightLegForward", "GoldenFacingRightCatchingSuperMissileLeftLegForward",
                    "GoldenFacingRightCatchingSuperMissileRightLegForward", "FacingLeftFalling", "FacingRightFalling", "Dying",
                    .. TorizoJumpsBack, "FacingLeftJumpingForward", "FacingRightJumpingForward",
                    "GoldenFacingLeftSittingDownAttack", "GoldenFacingRightSittingDownAttack",
                ],
            },
            new Dictionary<string, string[]>(StringComparer.Ordinal), null, null, [])
        {
            StepHelperTypes = ["TorizoAnimationSteps"],
        },
        // Ridley's body poses (his body frames, #2234): Lower Norfair Ridley's programs stand, roar, lift off, breathe fire,
        // turn through the forward pose and roar as he dies; Ceres Ridley's stand, roar, lift off, lunge, take the Baby and
        // breathe fire, never turning; his death breakup runs no body program.
        new("RidleyEntityFactory", "RidleyCollisionDefinitions", "BodyFrames", null, null, [],
            new Dictionary<string, string[]>(StringComparer.Ordinal),
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Ridley"] = [.. RidleyBodyFrames, "EnemyFrameId=RidleyBodyForward"],
                ["RidleyCeres"] = RidleyBodyFrames,
                ["RidleyBreakupPiece"] = [],
            },
            null, null, []),
    ];

    /// <summary>A hopper variant's four programs, as <c>HopperAnimationDefinitions.Programs</c> names them.</summary>
    private static string[] HopperVariant(string variant) =>
        [$"{variant}FloorJumping", $"{variant}FloorLanded", $"{variant}CeilingJumping", $"{variant}CeilingLanded"];

    /// <summary>The jumps back a walking Torizo's decisions continue in, each way onto either foot (#2175).</summary>
    private static string[] TorizoJumpsBack =>
    [
        "FacingLeftJumpingBackwardToLeftFoot", "FacingLeftJumpingBackwardToRightFoot",
        "FacingRightJumpingBackwardToLeftFoot", "FacingRightJumpingBackwardToRightFoot",
    ];

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
        "EnemyFrameId=RidleyBodyLeft", "EnemyFrameId=RidleyBodyLeftMouthHalfOpen", "EnemyFrameId=RidleyBodyLeftMouthOpen",
        "EnemyFrameId=RidleyBodyLeftLegsHalfExtended", "EnemyFrameId=RidleyBodyLeftLegsExtended", "EnemyFrameId=RidleyBodyRight",
        "EnemyFrameId=RidleyBodyRightMouthHalfOpen", "EnemyFrameId=RidleyBodyRightMouthOpen", "EnemyFrameId=RidleyBodyRightLegsHalfExtended",
        "EnemyFrameId=RidleyBodyRightLegsExtended",
    ];

    /// <summary>The four surface programs named <paramref name="crawls"/>, as <c>CrawlerAnimationDefinitions.Families</c> selects them.</summary>
    private static string[] CrawlerFamily(string crawls) =>
        [$"{crawls}OnLeftWall", $"{crawls}OnRightWall", $"{crawls}OnCeiling", $"{crawls}OnFloor"];
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
/// Evaluates a <see cref="PartFrameConfiguration"/> over Core's source: the frame table's entries by key (or the frames the
/// segments name), the segments' shown keys and the segments they name (inlining the segments' own beat helpers, as
/// <c>Show(int, int)</c>), the segments each animation plays, each definition's closure, and the self-checks that no program
/// Core names and no constant key Core looks up is left out of every part.
/// </summary>
/// <param name="model">The Core facts.</param>
/// <param name="configuration">The behaviour's configuration.</param>
internal sealed class PartFrames(CoreModel model, PartFrameConfiguration configuration)
{
    /// <summary>
    /// One segment's or animation's shown keys, the segments or animations it runs on into in its own slot (an animation
    /// plays its phases' segments; a segment naming a segment continues in the animation starting there), and the programs
    /// it starts in other definitions' slots.
    /// </summary>
    private sealed record PartProgram(HashSet<string> Keys, HashSet<AnimationNode> Continuations, List<(string Definition, string Program)> Starts);

    /// <summary>
    /// <paramref name="split"/>'s frames with each configured definition's table frames replaced by its own: its frames
    /// outside the table, with the table frames its programs or selectors show, less the frames drawn on another layer;
    /// a definition left with no frames is left out.
    /// </summary>
    /// <exception cref="InvalidDataException">The source does not have the configured shape, or a self-check fails.</exception>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> Apply(Behaviour behaviour, IReadOnlyDictionary<string, IReadOnlySet<string>> split)
    {
        if (configuration.FrameLists.Count > 0)
            return ApplyFrameLists(behaviour, split);
        var problems = new List<string>();
        Dictionary<string, string> table = Table(problems);
        HashSet<string> excluded = Excluded();
        Dictionary<string, HashSet<string>> keys = configuration.AnimationsType is null ? SelectedKeys(table, problems) : ProgramKeys(table, problems);
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
    /// <paramref name="split"/>'s frames with each listed frame given to the definition whose catalog list names it alone
    /// (<see cref="PartFrameConfiguration.FrameLists"/>); a definition left with no frames is left out.
    /// </summary>
    /// <exception cref="InvalidDataException">A list names no frame, names another list's frame, or a definition is not the behaviour's.</exception>
    private IReadOnlyDictionary<string, IReadOnlySet<string>> ApplyFrameLists(Behaviour behaviour, IReadOnlyDictionary<string, IReadOnlySet<string>> split)
    {
        var problems = new List<string>();
        var owned = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach ((string definition, string list) in configuration.FrameLists)
        {
            if (!behaviour.Definitions.Contains(definition))
                problems.Add($"{definition} is not a definition {behaviour.Type.Name} serves");
            string[] parts = list.Split('.');
            HashSet<string> frames = parts.Length == 2 ? model.EnemyFramesNamed(Initializer(parts[0], parts[1])) : [];
            if (frames.Count == 0)
                problems.Add($"{list} names no frame");
            owned.Add(definition, frames);
        }
        var listed = new HashSet<string>(StringComparer.Ordinal);
        foreach ((string definition, HashSet<string> frames) in owned)
        {
            foreach (string frame in frames.Where(frame => !listed.Add(frame)))
                problems.Add($"{definition}'s list names {frame}, which another list names");
        }
        if (problems.Count > 0)
            throw new InvalidDataException($"{behaviour.Type.Name}'s part frames cannot be split:\n" + string.Join('\n', problems));
        var result = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);
        foreach (string definition in behaviour.Definitions)
        {
            var frames = new HashSet<string>(split[definition].Where(frame => !listed.Contains(frame)), StringComparer.Ordinal);
            if (owned.TryGetValue(definition, out HashSet<string>? own))
                frames.UnionWith(own);
            if (frames.Count > 0)
                result.Add(definition, frames);
        }
        return result;
    }

    /// <summary>The configured table, as <c>Type.Member</c>, or the segments that name their frames.</summary>
    private string TableName => configuration.TableType is null ? $"{configuration.AnimationsType}.Segments" : $"{configuration.TableType}.{configuration.TableMember}";

    /// <summary>
    /// The self-check that each configured code frame is a frame outside the table that the walk gives each of its owning
    /// definitions, so the configuration names only frames the behaviour's code sets.
    /// </summary>
    private void CheckCodeFrames(Behaviour behaviour, IReadOnlyDictionary<string, IReadOnlySet<string>> split, HashSet<string> tableFrames, List<string> problems)
    {
        foreach ((string frame, string[] owners) in configuration.CodeFrames)
        {
            if (tableFrames.Contains(frame))
                problems.Add($"code frame {frame} is a frame of {TableName}");
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

    /// <summary>
    /// The frame table's entries, by key; with no table, every frame the segments name, and with a list of frames, every
    /// frame it lists, each keyed as the enum member it is (<c>EnemyFrameId=X</c>).
    /// </summary>
    private Dictionary<string, string> Table(List<string> problems)
    {
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        if (configuration.TableType is null)
        {
            foreach (ExpressionSyntax initializer in SegmentInitializers())
            {
                foreach (string frame in model.EnemyFramesNamed(initializer))
                    entries.TryAdd($"{model.EnemyFrameId.Name}={frame}", frame);
            }
            // So does every frame a segment shows through a helper written once over a side's poses (#2411); the segments'
            // own problems are reported as their closures are read.
            (Dictionary<AnimationNode, PartProgram> programs, _) = Programs([]);
            string prefix = $"{model.EnemyFrameId.Name}=";
            foreach (string key in programs.Values.SelectMany(program => program.Keys).Where(key => key.StartsWith(prefix, StringComparison.Ordinal)))
                entries.TryAdd(key, key[prefix.Length..]);
            if (entries.Count == 0)
                problems.Add($"{TableName} names no frame");
            return entries;
        }
        ExpressionSyntax table = TableInitializer();
        if (IsFrameList(table))
        {
            foreach (string frame in model.EnemyFramesNamed(table))
                entries.TryAdd($"{model.EnemyFrameId.Name}={frame}", frame);
            return entries;
        }
        foreach (InitializerExpressionSyntax entry in table.DescendantNodes().OfType<InitializerExpressionSyntax>()
                     .Where(node => node.Expressions.Count == 2 && node.Parent is InitializerExpressionSyntax))
        {
            SemanticModel semantic = model.ModelOf(entry.SyntaxTree);
            string? key = KeyOf(semantic, entry.Expressions[0], arguments: null);
            if (key is null || semantic.GetSymbolInfo(entry.Expressions[1]).Symbol is not IFieldSymbol frame
                || !SymbolEqualityComparer.Default.Equals(frame.ContainingType, model.EnemyFrameId))
            {
                problems.Add($"{TableName} entry {entry} is not a constant key and an EnemyFrameId member");
                continue;
            }
            if (!entries.TryAdd(key, frame.Name))
                problems.Add($"{TableName} lists key {key} twice");
        }
        if (entries.Count == 0)
            problems.Add($"{TableName} has no entries");
        return entries;
    }

    /// <summary>Whether the configured table is a list of frames: it holds no <c>{ key, frame }</c> entry and names a frame.</summary>
    private bool IsFrameList(ExpressionSyntax table) =>
        !table.DescendantNodes().OfType<InitializerExpressionSyntax>().Any(node => node.Expressions.Count == 2 && node.Parent is InitializerExpressionSyntax)
        && model.EnemyFramesNamed(table).Count > 0;

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
            pair => new HashSet<string>(table.Keys.Where(key => pair.Value.Any(selector => KeyParts(key).Any(part => Selects(selector, part)))),
                StringComparer.Ordinal),
            StringComparer.Ordinal);
        foreach (string selector in configuration.KeySelectors.Values.SelectMany(selectors => selectors)
                     .Where(selector => !table.Keys.Any(key => KeyParts(key).Any(part => Selects(selector, part)))))
            problems.Add($"selector {selector} selects no key of {TableName}");
        foreach (string key in table.Keys.Where(key => !keys.Values.Any(selected => selected.Contains(key))))
            problems.Add($"table key {key} carries no part's selector");
        return keys;
    }

    /// <summary>
    /// Whether <paramref name="selector"/> selects the key part <paramref name="part"/>: a <c>Parameter=Value</c> selector
    /// the part itself, a bare <c>Parameter</c> selector any value of that parameter (every member of an enum key type).
    /// </summary>
    private static bool Selects(string selector, string part) =>
        selector.Contains('=', StringComparison.Ordinal)
            ? string.Equals(selector, part, StringComparison.Ordinal)
            : part.StartsWith(selector + "=", StringComparison.Ordinal);

    /// <summary>Each definition's keys by the closure of the programs its slot is started in.</summary>
    private Dictionary<string, HashSet<string>> ProgramKeys(Dictionary<string, string> table, List<string> problems)
    {
        (Dictionary<AnimationNode, PartProgram> programs, INamedTypeSymbol programType) = Programs(problems);
        var keys = configuration.EntryPrograms.Keys.Concat(configuration.StartCueDefinitions).Distinct(StringComparer.Ordinal)
            .ToDictionary(definition => definition, _ => new HashSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);
        HashSet<(string Definition, AnimationNode Node)> reached = Closure(programs, problems);
        foreach ((string definition, AnimationNode reachedNode) in reached)
        {
            foreach (string key in programs[reachedNode].Keys)
            {
                if (!table.ContainsKey(key))
                    problems.Add($"segment {reachedNode.Name} shows key {key}, which {TableName} lacks");
                keys[definition].Add(key);
            }
        }
        // The self-check: every program Core names outside the segments and animations lies in some part's closure, this
        // behaviour's or another configured behaviour's running the same animations, so no start the configuration missed
        // leaves a part without the frames it shows.
        var inClosure = new HashSet<string>(reached.Where(pair => pair.Node.IsStart).Select(pair => Unsided(pair.Node.Name)), StringComparer.Ordinal);
        foreach (PartFrameConfiguration sibling in PartFrameConfiguration.All.Where(other => other.Behaviour != configuration.Behaviour
                     && other.AnimationsType == configuration.AnimationsType))
        {
            // The sibling reports its own problems when its behaviour is split; here only its closure counts.
            var siblingProblems = new List<string>();
            PartFrames siblingFrames = new(model, sibling);
            (Dictionary<AnimationNode, PartProgram> siblingPrograms, _) = siblingFrames.Programs(siblingProblems);
            inClosure.UnionWith(siblingFrames.Closure(siblingPrograms, siblingProblems).Where(pair => pair.Node.IsStart).Select(pair => Unsided(pair.Node.Name)));
        }
        foreach (string named in NamedOutside(programType, AnimationInitializers()).Where(program => !inClosure.Contains(program)))
            problems.Add($"{programType.Name}.{named} is named outside the segments and animations but no part's closure starts it");
        return keys;
    }

    /// <summary>
    /// Every segment and animation each definition's slot runs, as definition and node: the closure of the animations its
    /// entry programs start over what they run on into and start in other definitions' slots.
    /// </summary>
    private HashSet<(string Definition, AnimationNode Node)> Closure(Dictionary<AnimationNode, PartProgram> programs, List<string> problems)
    {
        var reached = new HashSet<(string Definition, AnimationNode Node)>();
        var pending = new Queue<(string Definition, AnimationNode Node)>(
            configuration.EntryPrograms.SelectMany(pair => pair.Value.Select(program => (pair.Key, AnimationNode.Start(program)))));
        while (pending.TryDequeue(out (string Definition, AnimationNode Node) next))
        {
            if (reached.Contains(next))
                continue;
            if (!programs.TryGetValue(next.Node, out PartProgram? program))
            {
                problems.Add(next.Node.IsStart
                    ? $"{next.Definition}'s program {next.Node.Name} starts no animation of {configuration.AnimationsType}.StartingAt"
                    : $"{next.Definition}'s animations play segment {next.Node.Name}, which {configuration.AnimationsType}.Segments lacks");
                continue;
            }
            reached.Add(next);
            foreach (AnimationNode continuation in program.Continuations)
                pending.Enqueue((next.Definition, continuation));
            foreach ((string definition, string started) in program.Starts)
                pending.Enqueue((definition, AnimationNode.Start(started)));
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
            foreach (SyntaxTree tree in model.AuthoredTrees)
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
    /// The segments and animations by node, with the segment identity type: each segment read from the segments' initializer
    /// and the tables it spreads in, each animation from <c>StartingAt</c>.
    /// </summary>
    private (Dictionary<AnimationNode, PartProgram>, INamedTypeSymbol) Programs(List<string> problems)
    {
        if (configuration.Sided is { } sided)
            return SidedPrograms(sided.Segments, sided.StartingAt, problems);
        var programs = new Dictionary<AnimationNode, PartProgram>();
        INamedTypeSymbol? programType = null;
        INamedTypeSymbol? startType = StartType();
        foreach (ExpressionSyntax initializer in SegmentInitializers())
            programType = ReadSegments(initializer, programs, programType, startType, problems);
        if (programType is null)
            throw new InvalidDataException($"{configuration.AnimationsType}.Segments has no [Segment] = [beats] entries.");
        startType ??= programType;
        ReadAnimations(StartingAtInitializer(), programs, programType, startType, problems);
        foreach (ISymbol named in NamedAnimations())
            ReadNamedAnimation(named, programs, programType, problems);
        return (programs, startType);
    }

    /// <summary>
    /// Every side's segments and animations of a sided family (<see cref="PartFrameConfiguration.Sided"/>), each named for its
    /// side: the helper building a side's segments is read with its parameters bound to the poses that side passes it, and the
    /// helper building a side's animations likewise.
    /// </summary>
    private (Dictionary<AnimationNode, PartProgram>, INamedTypeSymbol) SidedPrograms(string segmentsMember, string startingAtMember, List<string> problems)
    {
        var programs = new Dictionary<AnimationNode, PartProgram>();
        INamedTypeSymbol? programType = null;
        foreach ((string side, ExpressionSyntax body, IReadOnlyDictionary<IParameterSymbol, BoundArgument> bindings) in SideBodies(segmentsMember, problems))
        {
            var read = new Dictionary<AnimationNode, PartProgram>();
            programType = ReadSegments(body, read, programType, programType, problems, bindings);
            AddSide(programs, read, side, problems);
        }
        if (programType is null)
            throw new InvalidDataException($"{configuration.AnimationsType}.{segmentsMember} has no side's [Segment] = [beats] entries.");
        foreach ((string side, ExpressionSyntax body, _) in SideBodies(startingAtMember, problems))
        {
            var read = new Dictionary<AnimationNode, PartProgram>();
            ReadAnimations(body, read, programType, programType, problems);
            AddSide(programs, read, side, problems);
        }
        return (programs, programType);
    }

    /// <summary>
    /// The body of the helper each side's entry of <paramref name="member"/> calls, with that call's bindings: the side is the
    /// entry's key, an enum member, and the helper the first of the animations type's own helpers its value calls.
    /// </summary>
    private IEnumerable<(string Side, ExpressionSyntax Body, IReadOnlyDictionary<IParameterSymbol, BoundArgument> Bindings)> SideBodies(string member, List<string> problems)
    {
        ExpressionSyntax initializer = Initializer(configuration.AnimationsType!, member);
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        int sides = 0;
        foreach (AssignmentExpressionSyntax entry in initializer.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (entry is not { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var key] }, Right: var value }
                || semantic.GetSymbolInfo(key.Expression).Symbol is not IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } side)
                continue;
            (InvocationExpressionSyntax Call, IMethodSymbol Helper, ExpressionSyntax Body)? built = value.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                .Select(call => semantic.GetSymbolInfo(call).Symbol is IMethodSymbol helper && helper.ContainingType.Name == configuration.AnimationsType
                    && HelperBody(helper) is { } body ? (call, helper, body) : ((InvocationExpressionSyntax, IMethodSymbol, ExpressionSyntax)?)null)
                .FirstOrDefault(found => found is not null);
            if (built is not { } found)
            {
                problems.Add($"{configuration.AnimationsType}.{member}'s {side.Name} entry calls no helper of {configuration.AnimationsType} this generator can read");
                continue;
            }
            sides++;
            yield return (side.Name, found.Body, Bind(semantic, found.Call, found.Helper, Unbound));
        }
        if (sides == 0)
            problems.Add($"{configuration.AnimationsType}.{member} has no side's entry");
    }

    /// <summary>Adds one side's programs to <paramref name="programs"/>, each named, with what it continues in, for <paramref name="side"/>.</summary>
    private static void AddSide(Dictionary<AnimationNode, PartProgram> programs, Dictionary<AnimationNode, PartProgram> read, string side, List<string> problems)
    {
        foreach ((AnimationNode node, PartProgram program) in read)
        {
            var sided = new PartProgram(program.Keys, [.. program.Continuations.Select(continuation => continuation with { Name = Sided(continuation.Name, side) })],
                [.. program.Starts.Select(start => (start.Definition, Sided(start.Program, side)))]);
            if (!programs.TryAdd(node with { Name = Sided(node.Name, side) }, sided))
                problems.Add($"{node.Name} is listed twice for side {side}");
        }
    }

    /// <summary>The name of <paramref name="program"/> on <paramref name="side"/>.</summary>
    private static string Sided(string program, string side) => $"{program}@{side}";

    /// <summary>The program a sided program's name names, whatever its side.</summary>
    private static string Unsided(string program) => program.Split('@')[0];

    /// <summary>
    /// The identity <c>StartingAt</c> keys its animations by: the segment type, or, for a family whose animations are named
    /// apart from their segments (#2399, #2411: Torizo's <c>TorizoAnimation</c> over its <c>TorizoBodySegment</c>s), the
    /// animations' own enum. Cues and code start the animations by that identity.
    /// </summary>
    private INamedTypeSymbol? StartType() =>
        model.Member(configuration.AnimationsType!, "StartingAt") switch
        {
            IPropertySymbol { Type: INamedTypeSymbol { TypeArguments: [INamedTypeSymbol { TypeKind: TypeKind.Enum } key, ..] } } => key,
            IFieldSymbol { Type: INamedTypeSymbol { TypeArguments: [INamedTypeSymbol { TypeKind: TypeKind.Enum } key, ..] } } => key,
            _ => null,
        };

    /// <summary>
    /// The animations the configured animations type holds by a name of their own (#2399): its static properties and fields
    /// holding one animation over the segment type its <c>Segments</c> plays, each an animation an owner starts by that name,
    /// as Draygon's tail starts his final whips.
    /// </summary>
    private IEnumerable<ISymbol> NamedAnimations()
    {
        ISymbol segments = model.Member(configuration.AnimationsType!, "Segments");
        if (TypeOf(segments) is not INamedTypeSymbol { TypeArguments: [var segmentType, ..] })
            throw new InvalidDataException($"{configuration.AnimationsType}.Segments names no segment type.");
        return segments.ContainingType.GetMembers().Where(member => member.IsStatic && member.CanBeReferencedByName
            && TypeOf(member) is INamedTypeSymbol { Name: "Animation", TypeArguments: [var played] }
            && SymbolEqualityComparer.Default.Equals(played, segmentType));

        static ITypeSymbol? TypeOf(ISymbol member) => member switch
        {
            IPropertySymbol property => property.Type,
            IFieldSymbol field => field.Type,
            _ => null,
        };
    }

    /// <summary>Reads the animation <paramref name="named"/> holds into <paramref name="programs"/>, started by its own name: it plays the segments it names.</summary>
    private void ReadNamedAnimation(ISymbol named, Dictionary<AnimationNode, PartProgram> programs, INamedTypeSymbol programType, List<string> problems)
    {
        ExpressionSyntax initializer = CoreModel.InitializerOf(named);
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        var animation = new PartProgram(new HashSet<string>(StringComparer.Ordinal), [], []);
        foreach (IdentifierNameSyntax name in initializer.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            if (semantic.GetSymbolInfo(name).Symbol is IFieldSymbol segment && SymbolEqualityComparer.Default.Equals(segment.ContainingType, programType))
                animation.Continuations.Add(AnimationNode.Segment(segment.Name));
        }
        if (!programs.TryAdd(AnimationNode.Start(named.Name), animation))
            problems.Add($"the animation {named.Name} shares its name with another start of {configuration.AnimationsType}");
    }

    /// <summary>
    /// The initializer of the configured <c>Segments</c>, followed by those of the tables it spreads in and of the runs of
    /// steps their segments spread in (#1788), which several segments share.
    /// </summary>
    private List<ExpressionSyntax> SegmentInitializers()
    {
        if (configuration.Sided is { } sided)
            return [Initializer(configuration.AnimationsType!, sided.Segments), .. SideBodies(sided.Segments, []).Select(read => read.Body)];
        List<ExpressionSyntax> initializers = model.InitializersWithSpreads(model.Member(configuration.AnimationsType!, "Segments"));
        for (int index = 0; index < initializers.Count; index++)
        {
            foreach (ISymbol run in SpreadRuns(initializers[index]))
            {
                ExpressionSyntax initializer = CoreModel.InitializerOf(run);
                if (!initializers.Contains(initializer))
                    initializers.Add(initializer);
            }
        }
        return initializers;
    }

    /// <summary>The array members <paramref name="initializer"/> spreads into a segment's beats: the runs of steps segments share.</summary>
    private IEnumerable<ISymbol> SpreadRuns(ExpressionSyntax initializer)
    {
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        foreach (SpreadElementSyntax spread in initializer.DescendantNodes().OfType<SpreadElementSyntax>())
        {
            if (semantic.GetTypeInfo(spread.Expression).Type is IArrayTypeSymbol
                && semantic.GetSymbolInfo(spread.Expression).Symbol is (IFieldSymbol or IPropertySymbol) and var run)
                yield return run.OriginalDefinition;
        }
    }

    /// <summary>
    /// A helper's parameter bound to the argument a call of it passes (#2411): the argument, read in the caller's semantic
    /// model with the caller's own bindings, as a faced animation written once over a side's poses is called for each side.
    /// </summary>
    /// <param name="Semantic">The caller's semantic model.</param>
    /// <param name="Argument">The argument passed.</param>
    /// <param name="Outer">The caller's bindings, which the argument's own parameter names read.</param>
    private sealed record BoundArgument(SemanticModel Semantic, ExpressionSyntax Argument, IReadOnlyDictionary<IParameterSymbol, BoundArgument> Outer)
    {
        /// <summary>
        /// The elements a params parameter is handed one by one (#2557), none where the call passes none; null for a parameter
        /// handed one argument of its own type.
        /// </summary>
        public IReadOnlyList<ExpressionSyntax>? ParamsElements { get; init; }
    }

    /// <summary>No bindings: an initializer read outside any helper.</summary>
    private static readonly IReadOnlyDictionary<IParameterSymbol, BoundArgument> Unbound = new Dictionary<IParameterSymbol, BoundArgument>(SymbolEqualityComparer.Default);

    /// <summary>
    /// The beats <paramref name="beats"/> holds, each with the semantic model and bindings it is read in: a collection's own
    /// beats, the beats of each run of shared steps it spreads in, and the beats a helper written once over a side's or a
    /// stance's poses returns (#2411), its parameters bound to the call's arguments, in order. Null for an expression this
    /// generator cannot read as beats.
    /// </summary>
    private List<(SemanticModel Semantic, ExpressionSyntax Beat, IReadOnlyDictionary<IParameterSymbol, BoundArgument> Bindings)>? BeatsOf(
        SemanticModel semantic, ExpressionSyntax beats, IReadOnlyDictionary<IParameterSymbol, BoundArgument> bindings, int depth = 0)
    {
        if (depth > 16)
            return null;
        switch (beats)
        {
            case CollectionExpressionSyntax collection:
            {
                var read = new List<(SemanticModel, ExpressionSyntax, IReadOnlyDictionary<IParameterSymbol, BoundArgument>)>();
                foreach (CollectionElementSyntax element in collection.Elements)
                {
                    switch (element)
                    {
                        case SpreadElementSyntax { Expression: var spread }:
                            if (BeatsOf(semantic, spread, bindings, depth + 1) is { } inner)
                                read.AddRange(inner);
                            // Beats a library call reorders (Enumerable.Reverse over a run of beats handed in) show what the
                            // call names, read as one beat (#2557).
                            else if (spread is InvocationExpressionSyntax && semantic.GetSymbolInfo(spread).Symbol is IMethodSymbol library && !model.IsSource(library))
                                read.Add((semantic, spread, bindings));
                            else
                                return null;
                            break;
                        case ExpressionElementSyntax { Expression: var beat }:
                            read.Add((semantic, beat, bindings));
                            break;
                        default:
                            return null;
                    }
                }
                return read;
            }
            case InvocationExpressionSyntax invocation when semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol helper
                && model.IsSource(helper) && HelperBody(helper) is { } body:
                return BeatsOf(model.ModelOf(body.SyntaxTree), body, Bind(semantic, invocation, helper, bindings), depth + 1);
            // A shared beat builder whose body is no one expression (AnimationBeats.Steady over the frames it is handed) shows
            // what its call names, read as one beat (#2557).
            case InvocationExpressionSyntax invocation when semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol { ReturnType: IArrayTypeSymbol } builder
                && model.IsSource(builder):
                return [(semantic, invocation, bindings)];
        }
        switch (semantic.GetSymbolInfo(beats).Symbol)
        {
            case IParameterSymbol parameter when bindings.TryGetValue(parameter, out BoundArgument? bound) && bound.ParamsElements is { } elements:
                return [.. elements.Select(element => (bound.Semantic, element, bound.Outer))];
            case IParameterSymbol parameter when bindings.TryGetValue(parameter, out BoundArgument? bound):
                return BeatsOf(bound.Semantic, bound.Argument, bound.Outer, depth + 1);
            case IFieldSymbol or IPropertySymbol when semantic.GetTypeInfo(beats).Type is IArrayTypeSymbol:
                ExpressionSyntax shared = CoreModel.InitializerOf(semantic.GetSymbolInfo(beats).Symbol!.OriginalDefinition);
                return BeatsOf(model.ModelOf(shared.SyntaxTree), shared, Unbound, depth + 1);
            default:
                return null;
        }
    }

    /// <summary>The expression <paramref name="helper"/>'s body returns, when it is one expression; null otherwise.</summary>
    private static ExpressionSyntax? HelperBody(IMethodSymbol helper) =>
        helper.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())
            .Select(declaration => declaration switch
            {
                MethodDeclarationSyntax method => method.ExpressionBody?.Expression ?? Returned(method.Body),
                LocalFunctionStatementSyntax local => local.ExpressionBody?.Expression ?? Returned(local.Body),
                _ => null,
            })
            .FirstOrDefault(body => body is not null);

    /// <summary>The expression <paramref name="body"/> returns when its only other statements are local functions.</summary>
    private static ExpressionSyntax? Returned(BlockSyntax? body) =>
        body is { Statements: { Count: > 0 } statements }
            && statements[statements.Count - 1] is ReturnStatementSyntax { Expression: { } returned }
            && statements.Take(statements.Count - 1).All(statement => statement is LocalFunctionStatementSyntax)
            ? returned
            : null;

    /// <summary>The bindings a call of <paramref name="helper"/> makes: each parameter bound to the argument the call passes it.</summary>
    private static Dictionary<IParameterSymbol, BoundArgument> Bind(SemanticModel caller, InvocationExpressionSyntax invocation, IMethodSymbol helper,
        IReadOnlyDictionary<IParameterSymbol, BoundArgument> callerBindings)
    {
        var bound = new Dictionary<IParameterSymbol, BoundArgument>(SymbolEqualityComparer.Default);
        SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;
        for (int index = 0; index < arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments[index];
            IParameterSymbol? parameter = argument.NameColon is { } named
                ? helper.Parameters.FirstOrDefault(candidate => candidate.Name == named.Name.Identifier.Text)
                : index < helper.Parameters.Length ? helper.Parameters[index] : null;
            if (parameter is null)
                continue;
            // A params parameter handed its elements one by one binds them all, read as the beats they are.
            if (parameter.IsParams && argument.NameColon is null
                && (arguments.Count != helper.Parameters.Length || !SymbolEqualityComparer.Default.Equals(caller.GetTypeInfo(argument.Expression).Type, parameter.Type)))
            {
                bound[parameter.OriginalDefinition] = new BoundArgument(caller, argument.Expression, callerBindings)
                {
                    ParamsElements = [.. arguments.Skip(index).Select(element => element.Expression)],
                };
                break;
            }
            bound[parameter.OriginalDefinition] = new BoundArgument(caller, argument.Expression, callerBindings);
        }
        // A params parameter the call hands nothing binds no elements.
        foreach (IParameterSymbol parameter in helper.Parameters.Where(parameter => parameter.IsParams && !bound.ContainsKey(parameter.OriginalDefinition)))
            bound[parameter.OriginalDefinition] = new BoundArgument(caller, invocation, callerBindings) { ParamsElements = [] };
        return bound;
    }

    /// <summary>
    /// The enum members <paramref name="expression"/> stands for through its bindings and the records it reads (#2411): a
    /// bound parameter's argument's, or a record member's, read through the construction that gave the record its value (a
    /// side's poses read through <c>TorizoSide.Left</c>'s initializer). An expression standing for an enum member directly is
    /// read by the beat itself.
    /// </summary>
    private IEnumerable<IFieldSymbol> BoundMembers(SemanticModel semantic, ExpressionSyntax expression, IReadOnlyDictionary<IParameterSymbol, BoundArgument> bindings)
    {
        foreach (SyntaxNode node in expression.DescendantNodesAndSelf())
        {
            switch (node)
            {
                case IdentifierNameSyntax identifier when !IsAccessedRecord(identifier):
                    if (semantic.GetSymbolInfo(identifier).Symbol is IParameterSymbol parameter && bindings.TryGetValue(parameter.OriginalDefinition, out BoundArgument? bound))
                    {
                        foreach (IFieldSymbol member in ValueMembers(bound.Semantic, bound.Argument, bound.Outer, depth: 0))
                            yield return member;
                    }
                    break;
                case MemberAccessExpressionSyntax access when semantic.GetSymbolInfo(access).Symbol is IPropertySymbol { IsStatic: false }
                    && !IsAccessedRecord(access):
                    foreach (IFieldSymbol member in ValueMembers(semantic, access, bindings, depth: 0))
                        yield return member;
                    break;
            }
        }
    }

    /// <summary>Whether <paramref name="expression"/> is the record a member access reads a member of, which the access stands for whole.</summary>
    private static bool IsAccessedRecord(ExpressionSyntax expression) =>
        expression.Parent is MemberAccessExpressionSyntax access && access.Expression == expression;

    /// <summary>
    /// The enum members the value of <paramref name="expression"/> is: an enum member itself, or what a bound parameter's
    /// argument or a record member read through its construction is.
    /// </summary>
    private IEnumerable<IFieldSymbol> ValueMembers(SemanticModel semantic, ExpressionSyntax expression, IReadOnlyDictionary<IParameterSymbol, BoundArgument> bindings, int depth)
    {
        if (depth > 16)
            yield break;
        ISymbol? symbol = semantic.GetSymbolInfo(expression).Symbol;
        switch (symbol)
        {
            case IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member:
                yield return member;
                yield break;
            case IParameterSymbol parameter when bindings.TryGetValue(parameter.OriginalDefinition, out BoundArgument? bound):
                foreach (IFieldSymbol member in ValueMembers(bound.Semantic, bound.Argument, bound.Outer, depth + 1))
                    yield return member;
                yield break;
            case IPropertySymbol { IsStatic: false } property when expression is MemberAccessExpressionSyntax { Expression: var record }
                && RecordValue(semantic, record, bindings, depth + 1) is { } construction
                && ConstructionArgument(construction.Semantic, construction.Creation, property.Name) is { } argument:
                foreach (IFieldSymbol member in ValueMembers(construction.Semantic, argument, construction.Bindings, depth + 1))
                    yield return member;
                yield break;
        }
    }

    /// <summary>
    /// The construction that gave <paramref name="expression"/>'s record its value: the construction itself, a bound
    /// parameter's argument's, a static member's initializer's, or a record member's argument's in its own construction.
    /// </summary>
    private (SemanticModel Semantic, BaseObjectCreationExpressionSyntax Creation, IReadOnlyDictionary<IParameterSymbol, BoundArgument> Bindings)? RecordValue(
        SemanticModel semantic, ExpressionSyntax expression, IReadOnlyDictionary<IParameterSymbol, BoundArgument> bindings, int depth)
    {
        if (depth > 16)
            return null;
        if (expression is BaseObjectCreationExpressionSyntax creation)
            return (semantic, creation, bindings);
        switch (semantic.GetSymbolInfo(expression).Symbol)
        {
            case IParameterSymbol parameter when bindings.TryGetValue(parameter.OriginalDefinition, out BoundArgument? bound):
                return RecordValue(bound.Semantic, bound.Argument, bound.Outer, depth + 1);
            case IFieldSymbol { IsStatic: true } or IPropertySymbol { IsStatic: true } when model.IsSource(semantic.GetSymbolInfo(expression).Symbol!):
                ExpressionSyntax initializer = CoreModel.InitializerOf(semantic.GetSymbolInfo(expression).Symbol!.OriginalDefinition);
                return RecordValue(model.ModelOf(initializer.SyntaxTree), initializer, Unbound, depth + 1);
            case IPropertySymbol { IsStatic: false } property when expression is MemberAccessExpressionSyntax { Expression: var record }
                && RecordValue(semantic, record, bindings, depth + 1) is { } construction
                && ConstructionArgument(construction.Semantic, construction.Creation, property.Name) is { } argument:
                return RecordValue(construction.Semantic, argument, construction.Bindings, depth + 1);
            default:
                return null;
        }
    }

    /// <summary>The argument <paramref name="creation"/> passes its constructor's parameter <paramref name="name"/>: a positional record's member's value.</summary>
    private static ExpressionSyntax? ConstructionArgument(SemanticModel semantic, BaseObjectCreationExpressionSyntax creation, string name)
    {
        // A record set in its initializer names the member there (#2557).
        if (creation.Initializer is { } initializer
            && initializer.Expressions.OfType<AssignmentExpressionSyntax>()
                .FirstOrDefault(assignment => assignment.Left is IdentifierNameSyntax { Identifier.Text: var member } && member == name) is { } set)
            return set.Right;
        if (semantic.GetSymbolInfo(creation).Symbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } constructor || creation.ArgumentList is not { } arguments)
            return null;
        for (int index = 0; index < arguments.Arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments.Arguments[index];
            string? parameter = argument.NameColon is { } named ? named.Name.Identifier.Text
                : index < constructor.Parameters.Length ? constructor.Parameters[index].Name : null;
            if (parameter == name)
                return argument.Expression;
        }
        return null;
    }

    /// <summary>The initializer of the configured <c>StartingAt</c>.</summary>
    private ExpressionSyntax StartingAtInitializer() => Initializer(configuration.AnimationsType!, "StartingAt");

    /// <summary>Every initializer the segments and animations are read from, the animations held by name (#2399) included.</summary>
    private List<ExpressionSyntax> AnimationInitializers() => configuration.Sided is { } sided
        ? [.. SegmentInitializers(), Initializer(configuration.AnimationsType!, sided.StartingAt),
            .. SideBodies(sided.StartingAt, []).SelectMany(read => (IEnumerable<ExpressionSyntax>)[read.Body, .. LocalFunctionBodies(read.Body)])]
        : [.. SegmentInitializers(), StartingAtInitializer(), .. NamedAnimations().Select(CoreModel.InitializerOf)];

    /// <summary>
    /// The expression bodies of the local functions declared beside <paramref name="body"/> when it is the expression a
    /// helper's block returns (#2557): a side's animations written over a local function, as <c>RunningOn</c>, name their
    /// segments there.
    /// </summary>
    private static IEnumerable<ExpressionSyntax> LocalFunctionBodies(ExpressionSyntax body) =>
        body.Parent is ReturnStatementSyntax { Parent: BlockSyntax block }
            ? block.Statements.OfType<LocalFunctionStatementSyntax>().Select(local => local.ExpressionBody?.Expression).OfType<ExpressionSyntax>()
            : [];

    /// <summary>
    /// Reads the <c>[P.X] = [beats]</c> entries of <paramref name="initializer"/> into <paramref name="programs"/>, and
    /// returns the segment identity type.
    /// </summary>
    private INamedTypeSymbol? ReadSegments(ExpressionSyntax initializer, Dictionary<AnimationNode, PartProgram> programs, INamedTypeSymbol? programType,
        INamedTypeSymbol? startType, List<string> problems, IReadOnlyDictionary<IParameterSymbol, BoundArgument>? bindings = null)
    {
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        foreach (AssignmentExpressionSyntax entry in initializer.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (entry is not { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var name] }, Right: var beats }
                || semantic.GetSymbolInfo(name.Expression).Symbol is not IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } identity)
                continue;
            // A segment is its collection of beats, or a helper's beats written once over a side's poses (#2411); any other
            // entry is no segment.
            if (BeatsOf(semantic, beats, bindings ?? Unbound) is not { } read)
            {
                if (beats is CollectionExpressionSyntax)
                    problems.Add($"segment {identity.Name} has beats this generator cannot read: {beats}");
                continue;
            }
            programType ??= identity.ContainingType;
            var program = new PartProgram(new HashSet<string>(StringComparer.Ordinal), [], []);
            foreach ((SemanticModel beatSemantic, ExpressionSyntax beat, IReadOnlyDictionary<IParameterSymbol, BoundArgument> beatBindings) in read)
                ReadBeat(beatSemantic, beat, beatBindings, startType ?? identity.ContainingType, program, problems);
            if (!programs.TryAdd(AnimationNode.Segment(identity.Name), program))
                problems.Add($"segment {identity.Name} is listed twice");
        }
        return programType;
    }

    /// <summary>
    /// Reads the <c>[P.X] = new([phases], end)</c> entries of <paramref name="initializer"/> into <paramref name="programs"/>:
    /// the animation starting at each segment, or named by <paramref name="startType"/>'s member (#2411), plays the segments
    /// its phases name, there or in the animations type's helpers it calls.
    /// </summary>
    private void ReadAnimations(ExpressionSyntax initializer, Dictionary<AnimationNode, PartProgram> programs, INamedTypeSymbol programType,
        INamedTypeSymbol startType, List<string> problems)
    {
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        int read = 0;
        foreach (AssignmentExpressionSyntax entry in initializer.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (entry is not { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var name] } }
                || semantic.GetSymbolInfo(name.Expression).Symbol is not IFieldSymbol identity
                || !SymbolEqualityComparer.Default.Equals(identity.ContainingType, startType))
                continue;
            read++;
            var animation = new PartProgram(new HashSet<string>(StringComparer.Ordinal), [], []);
            foreach (IFieldSymbol segment in NamedIn(semantic, entry.Right, depth: 0))
            {
                if (SymbolEqualityComparer.Default.Equals(segment.ContainingType, programType))
                    animation.Continuations.Add(AnimationNode.Segment(segment.Name));
            }
            if (!programs.TryAdd(AnimationNode.Start(identity.Name), animation))
                problems.Add($"the animation starting at {identity.Name} is listed twice");
        }
        // A helper written once over a facing's or a species' segments (#2557) returns that one's starts, each a key and the
        // animation starting there, read with its call's bindings.
        foreach (InvocationExpressionSyntax call in initializer.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            if (semantic.GetSymbolInfo(call).Symbol is not IMethodSymbol helper || helper.ContainingType.Name != configuration.AnimationsType
                || helper.ReturnType is not IArrayTypeSymbol { ElementType: INamedTypeSymbol { Name: "KeyValuePair" } }
                || HelperBody(helper) is not CollectionExpressionSyntax body)
                continue;
            Dictionary<IParameterSymbol, BoundArgument> helperBindings = Bind(semantic, call, helper, Unbound);
            SemanticModel helperSemantic = model.ModelOf(body.SyntaxTree);
            foreach (ExpressionElementSyntax element in body.Elements.OfType<ExpressionElementSyntax>())
            {
                if (element.Expression is not BaseObjectCreationExpressionSyntax { ArgumentList.Arguments: [var key, var played] })
                {
                    problems.Add($"{helper.Name}'s start {element} is no new(key, animation)");
                    continue;
                }
                IFieldSymbol[] keys = [.. ValueMembers(helperSemantic, key.Expression, helperBindings, depth: 0)];
                if (keys is not [var identity] || !SymbolEqualityComparer.Default.Equals(identity.ContainingType, startType))
                {
                    problems.Add($"{helper.Name}'s start {key} names no one {startType.Name} member");
                    continue;
                }
                read++;
                var animation = new PartProgram(new HashSet<string>(StringComparer.Ordinal), [], []);
                foreach (IFieldSymbol segment in NamedIn(helperSemantic, played.Expression, depth: 0)
                             .Concat(BoundMembers(helperSemantic, played.Expression, helperBindings)))
                {
                    if (SymbolEqualityComparer.Default.Equals(segment.ContainingType, programType))
                        animation.Continuations.Add(AnimationNode.Segment(segment.Name));
                }
                if (!programs.TryAdd(AnimationNode.Start(identity.Name), animation))
                    problems.Add($"the animation starting at {identity.Name} is listed twice");
            }
        }
        // The segments' own builders (#2557) start, at each segment handed to them, an animation playing that segment whole.
        foreach (InvocationExpressionSyntax call in initializer.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            if (semantic.GetSymbolInfo(call).Symbol is not IMethodSymbol
                {
                    ContainingType.Name: "AnimationSegments", Name: "Starts" or "EachLooping" or "StartsEnding" or "EachEnding",
                })
            {
                continue;
            }
            foreach (ArgumentSyntax argument in call.ArgumentList.Arguments)
            {
                if (semantic.GetSymbolInfo(argument.Expression).Symbol is not IFieldSymbol identity
                    || !SymbolEqualityComparer.Default.Equals(identity.ContainingType, startType))
                    continue;
                read++;
                var animation = new PartProgram(new HashSet<string>(StringComparer.Ordinal), [], []);
                if (SymbolEqualityComparer.Default.Equals(identity.ContainingType, programType))
                    animation.Continuations.Add(AnimationNode.Segment(identity.Name));
                if (!programs.TryAdd(AnimationNode.Start(identity.Name), animation))
                    problems.Add($"the animation starting at {identity.Name} is listed twice");
            }
        }
        if (read == 0)
            problems.Add($"{configuration.AnimationsType}.StartingAt has no [Segment] = new(...) entries and builds no start from a segment");
    }

    /// <summary>
    /// The fields <paramref name="expression"/> names, and those named in the bodies of the animations type's own helpers it
    /// calls (#2411: a volley of sonic booms stands its ground in the segment its helper names).
    /// </summary>
    private IEnumerable<IFieldSymbol> NamedIn(SemanticModel semantic, ExpressionSyntax expression, int depth)
    {
        foreach (SyntaxNode node in expression.DescendantNodesAndSelf())
        {
            ISymbol? symbol = node is IdentifierNameSyntax or InvocationExpressionSyntax ? semantic.GetSymbolInfo(node).Symbol : null;
            if (symbol is IFieldSymbol field)
                yield return field;
            if (depth < 8 && node is InvocationExpressionSyntax && symbol is IMethodSymbol helper && helper.ContainingType.Name == configuration.AnimationsType
                && HelperBody(helper) is { } body)
            {
                foreach (IFieldSymbol named in NamedIn(model.ModelOf(body.SyntaxTree), body, depth + 1))
                    yield return named;
            }
        }
    }

    /// <summary>
    /// Reads one beat, read with <paramref name="bindings"/> when a helper written once over a side's poses holds it (#2411):
    /// the keys it shows, the animations it names (which it continues in), the programs its start cue starts.
    /// </summary>
    private void ReadBeat(SemanticModel semantic, ExpressionSyntax beat, IReadOnlyDictionary<IParameterSymbol, BoundArgument> bindings,
        INamedTypeSymbol programType, PartProgram program, List<string> problems)
    {
        var startArguments = new HashSet<SyntaxNode>();
        foreach (BaseObjectCreationExpressionSyntax creation in beat.DescendantNodesAndSelf().OfType<BaseObjectCreationExpressionSyntax>())
        {
            if (configuration.StartCue is null || semantic.GetSymbolInfo(creation).Symbol is not IMethodSymbol { ContainingType.Name: var created }
                || created != configuration.StartCue || creation.ArgumentList is not { } arguments)
                continue;
            if (arguments.Arguments.Count != configuration.StartCueDefinitions.Count)
            {
                problems.Add($"{configuration.StartCue} at {beat} does not start one program for each of {string.Join(", ", configuration.StartCueDefinitions)}");
                continue;
            }
            for (int index = 0; index < arguments.Arguments.Count; index++)
            {
                startArguments.Add(arguments.Arguments[index]);
                if (semantic.GetSymbolInfo(arguments.Arguments[index].Expression).Symbol is IFieldSymbol started
                    && SymbolEqualityComparer.Default.Equals(started.ContainingType, programType))
                    program.Starts.Add((configuration.StartCueDefinitions[index], started.Name));
                else
                    problems.Add($"{configuration.StartCue} at {beat} starts a program that is not a {programType.Name} member");
            }
        }
        // Any other segment the beat names continues its own slot's play in the animation starting there (a go-to, a
        // counted repeat).
        INamedTypeSymbol keyType = KeyType();
        foreach (IdentifierNameSyntax name in beat.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            if (name.Ancestors().Any(startArguments.Contains) || semantic.GetSymbolInfo(name).Symbol is not IFieldSymbol field)
                continue;
            if (SymbolEqualityComparer.Default.Equals(field.ContainingType, programType))
                program.Continuations.Add(AnimationNode.Start(field.Name));
            // A beat shows an enum key (a frame the segments name) by naming it.
            else if (keyType.TypeKind == TypeKind.Enum && SymbolEqualityComparer.Default.Equals(field.ContainingType, keyType))
                program.Keys.Add($"{keyType.Name}={field.Name}");
        }
        // A beat a helper holds names what its bound parameters and the records it reads stand for (#2411): a side's pose, the
        // walk an attack ends in.
        foreach (IFieldSymbol field in BoundMembers(semantic, beat, bindings))
        {
            if (SymbolEqualityComparer.Default.Equals(field.ContainingType, programType))
                program.Continuations.Add(AnimationNode.Start(field.Name));
            else if (keyType.TypeKind == TypeKind.Enum && SymbolEqualityComparer.Default.Equals(field.ContainingType, keyType))
                program.Keys.Add($"{keyType.Name}={field.Name}");
        }
        // A beat shows a constructed key either by constructing it or through one of the segments' own helpers.
        foreach (InvocationExpressionSyntax invocation in beat.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            if (semantic.GetSymbolInfo(invocation).Symbol is not IMethodSymbol helper
                || (helper.ContainingType.Name != configuration.AnimationsType && !configuration.StepHelperTypes.Contains(helper.ContainingType.Name)))
                continue;
            foreach (string key in HelperKeys(semantic, invocation, helper))
                program.Keys.Add(key);
        }
        foreach (BaseObjectCreationExpressionSyntax creation in beat.DescendantNodesAndSelf().OfType<BaseObjectCreationExpressionSyntax>())
        {
            if (IsKeyCreation(semantic, creation) && KeyOf(semantic, creation, arguments: null) is { } key)
                program.Keys.Add(key);
        }
    }

    /// <summary>The keys a call of a segments' helper shows: the key its body constructs, with the call's arguments in place of its parameters.</summary>
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

    /// <summary>The frame table's key type: the type its entries' keys construct, or EnemyFrameId for segments naming their frames.</summary>
    private INamedTypeSymbol KeyType()
    {
        if (_keyType is not null)
            return _keyType;
        if (configuration.TableType is null || IsFrameList(TableInitializer()))
            return _keyType = model.EnemyFrameId;
        ExpressionSyntax initializer = TableInitializer();
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
            ?? throw new InvalidDataException($"{TableName}'s keys construct no type and name no enum member.");
        return _keyType;
    }

    private INamedTypeSymbol? _keyType;

    /// <summary>The configured frame table's initializer.</summary>
    private ExpressionSyntax TableInitializer() => Initializer(configuration.TableType!, configuration.TableMember!);

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
    /// The self-check: every key Core constructs from constants, or every table frame Core names, outside the frame table,
    /// segments and animations (and the catalogs naming every frame) is shown by some part, so a pose or frame code sets
    /// directly is on that part's sheet.
    /// </summary>
    private void CheckConstantKeys(Dictionary<string, string> table, Dictionary<string, HashSet<string>> keys, List<string> problems)
    {
        INamedTypeSymbol keyType = KeyType();
        List<ExpressionSyntax> sources = [.. configuration.TableType is null ? [] : new[] { TableInitializer() },
            .. configuration.AnimationsType is null ? [] : AnimationInitializers()];
        var shown = new HashSet<string>(keys.Values.SelectMany(selected => selected), StringComparer.Ordinal);
        // Behaviours running one animation set over one table share its frames: a key a part of any of them shows is shown
        // (#2426: the Ceres doors' owner and the elevator door's factory). Each sharing behaviour's own problems are
        // reported by its own split, which runs this check for it.
        foreach (PartFrameConfiguration sharing in PartFrameConfiguration.All.Where(other => !ReferenceEquals(other, configuration)
                     && other.AnimationsType is not null && other.AnimationsType == configuration.AnimationsType
                     && other.TableType == configuration.TableType && other.TableMember == configuration.TableMember))
        {
            var other = new PartFrames(model, sharing);
            var reportedByItsOwnSplit = new List<string>();
            shown.UnionWith(other.ProgramKeys(other.Table(reportedByItsOwnSplit), reportedByItsOwnSplit).Values.SelectMany(selected => selected));
        }
        // Only a name spelled as one of an enum key type's members can name one; the rest need no binding.
        var members = new HashSet<string>(keyType.GetMembers().OfType<IFieldSymbol>().Select(member => member.Name), StringComparer.Ordinal);
        foreach (SyntaxTree tree in model.AuthoredTrees)
        {
            SemanticModel semantic = model.ModelOf(tree);
            foreach (ExpressionSyntax expression in tree.GetRoot().DescendantNodes().OfType<ExpressionSyntax>())
            {
                if (keyType.TypeKind == TypeKind.Enum
                        ? expression is not IdentifierNameSyntax { Identifier.Text: var text } || !members.Contains(text)
                        : expression is not BaseObjectCreationExpressionSyntax)
                    continue;
                if (expression.Ancestors().Any(node => node is ExpressionSyntax ancestor && sources.Contains(ancestor) || ProjectileProgramFrames.IsTest(node, expression))
                    || InTable(semantic, expression)
                    || InAnimationSegments(semantic, expression)
                    || KeyNamed(semantic, expression, keyType) is not { } key)
                    continue;
                if (!table.ContainsKey(key))
                {
                    // A frame the segments do not name is a frame of the behaviour's own code, not of its parts' animations.
                    if (keyType.TypeKind != TypeKind.Enum)
                        problems.Add($"{model.RelativePath(tree)}: key {key} has no entry in {TableName}");
                }
                else if (!shown.Contains(key))
                    problems.Add($"{model.RelativePath(tree)}: key {key} is looked up but no part shows it");
            }
        }
    }

    /// <summary>
    /// Whether <paramref name="expression"/> lies in the segments a family's typed animations declare: a frame another
    /// family's segments show is that family's show, which its own behaviour's walk finds, however many families show the
    /// frame (#2327); it is no lookup of this behaviour's table.
    /// </summary>
    private bool InAnimationSegments(SemanticModel semantic, ExpressionSyntax expression) =>
        expression.Ancestors().OfType<PropertyDeclarationSyntax>().FirstOrDefault() is { } property
        && semantic.GetDeclaredSymbol(property) is IPropertySymbol { Type: INamedTypeSymbol type }
        && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, model.AnimationSegments);

    /// <summary>Whether <paramref name="expression"/> lies in a catalog naming every frame or kind, which shows nothing.</summary>
    private bool InTable(SemanticModel semantic, ExpressionSyntax expression) =>
        semantic.GetEnclosingSymbol(expression.SpanStart)?.ContainingType is { } type && model.IsTable(type);

    /// <summary>
    /// The key <paramref name="expression"/> names: a construction of the key type from constants, or a member of an enum
    /// key type; null for anything else.
    /// </summary>
    private static string? KeyNamed(SemanticModel semantic, ExpressionSyntax expression, INamedTypeSymbol keyType) => expression switch
    {
        IdentifierNameSyntax when semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol member
                                  && SymbolEqualityComparer.Default.Equals(member.ContainingType, keyType) => $"{keyType.Name}={member.Name}",
        BaseObjectCreationExpressionSyntax creation when semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
                                                         && SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, keyType)
            => KeyOf(semantic, creation, arguments: null),
        _ => null,
    };

    /// <summary>The members of <paramref name="programType"/> Core names outside the segments' and animations' initializers, other than in tests.</summary>
    private IEnumerable<string> NamedOutside(INamedTypeSymbol programType, IReadOnlyList<ExpressionSyntax> initializers)
    {
        var named = new SortedSet<string>(StringComparer.Ordinal);
        foreach (SyntaxTree tree in model.AuthoredTrees)
        {
            SemanticModel semantic = model.ModelOf(tree);
            foreach (IdentifierNameSyntax name in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (name.Ancestors().Any(node => node is ExpressionSyntax expression && initializers.Contains(expression) || ProjectileProgramFrames.IsTest(node, name))
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
