using System.Collections.Immutable;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>An enemy composition schema revision the catalog accepts, with the named frames its documents carry.</summary>
/// <param name="Revision">The revision number a document stores.</param>
/// <param name="FrameCount">The named frames a document of this revision carries.</param>
internal sealed record EnemySpritemapSchema(int Revision, int FrameCount)
{
    /// <summary>Schema revision 4.</summary>
    internal static readonly EnemySpritemapSchema Legacy = new(4, EnemySpritemapDefinitions.LegacyFrameCount);
    /// <summary>Schema revision 5.</summary>
    internal static readonly EnemySpritemapSchema Intermediate = new(5, EnemySpritemapDefinitions.IntermediateFrameCount);
    /// <summary>Schema revision 6.</summary>
    internal static readonly EnemySpritemapSchema Earlier = new(6, EnemySpritemapDefinitions.EarlierFrameCount);
    /// <summary>Schema revision 7.</summary>
    internal static readonly EnemySpritemapSchema Prior = new(7, EnemySpritemapDefinitions.PriorFrameCount);
    /// <summary>Schema revision 8.</summary>
    internal static readonly EnemySpritemapSchema Previous = new(8, EnemySpritemapDefinitions.PreviousFrameCount);
    /// <summary>Schema revision 9.</summary>
    internal static readonly EnemySpritemapSchema PreOwtchStoke = new(9, EnemySpritemapDefinitions.PreOwtchStokeFrameCount);
    /// <summary>Schema revision 10.</summary>
    internal static readonly EnemySpritemapSchema PreRipper = new(10, EnemySpritemapDefinitions.PreRipperFrameCount);
    /// <summary>Schema revision 11.</summary>
    internal static readonly EnemySpritemapSchema PreFireflea = new(11, EnemySpritemapDefinitions.PreFirefleaFrameCount);
    /// <summary>Schema revision 12.</summary>
    internal static readonly EnemySpritemapSchema PreMagdollite = new(12, EnemySpritemapDefinitions.PreMagdolliteFrameCount);
    /// <summary>Last art-only composition schema; later accepted schemas own editable display bindings.</summary>
    internal static readonly EnemySpritemapSchema PreDisplayBindings = new(13, EnemySpritemapDefinitions.PreDisplayBindingsFrameCount);
    /// <summary>Schema revision 14.</summary>
    internal static readonly EnemySpritemapSchema PreCeresDoor = new(14, EnemySpritemapDefinitions.PreCeresDoorFrameCount);
    /// <summary>Schema revision 15.</summary>
    internal static readonly EnemySpritemapSchema PreCeresBaby = new(15, EnemySpritemapDefinitions.PreCeresBabyFrameCount);
    /// <summary>Schema revision 16.</summary>
    internal static readonly EnemySpritemapSchema PreRio = new(16, EnemySpritemapDefinitions.PreRioFrameCount);
    /// <summary>Schema revision 17.</summary>
    internal static readonly EnemySpritemapSchema PreLowerNorfairRio = new(17, EnemySpritemapDefinitions.PreLowerNorfairRioFrameCount);
    /// <summary>Schema revision 18.</summary>
    internal static readonly EnemySpritemapSchema PreNorfairRio = new(18, EnemySpritemapDefinitions.PreNorfairRioFrameCount);
    /// <summary>Schema revision 19.</summary>
    internal static readonly EnemySpritemapSchema PrePuyo = new(19, EnemySpritemapDefinitions.PrePuyoFrameCount);
    /// <summary>Schema revision 20.</summary>
    internal static readonly EnemySpritemapSchema PreBull = new(20, EnemySpritemapDefinitions.PreBullFrameCount);
    /// <summary>Schema revision 21.</summary>
    internal static readonly EnemySpritemapSchema PreAlcoon = new(21, EnemySpritemapDefinitions.PreAlcoonFrameCount);
    /// <summary>Schema revision 22.</summary>
    internal static readonly EnemySpritemapSchema PreBeetom = new(22, EnemySpritemapDefinitions.PreBeetomFrameCount);
    /// <summary>Schema revision 23.</summary>
    internal static readonly EnemySpritemapSchema PreHopper = new(23, EnemySpritemapDefinitions.PreHopperFrameCount);
    /// <summary>Schema revision 24.</summary>
    internal static readonly EnemySpritemapSchema PreChoot = new(24, EnemySpritemapDefinitions.PreChootFrameCount);
    /// <summary>Schema revision 25.</summary>
    internal static readonly EnemySpritemapSchema PreHZoomer = new(25, EnemySpritemapDefinitions.PreHZoomerFrameCount);
    /// <summary>Schema revision 26.</summary>
    internal static readonly EnemySpritemapSchema PreSbug = new(26, EnemySpritemapDefinitions.PreSbugFrameCount);
    /// <summary>Schema revision 27.</summary>
    internal static readonly EnemySpritemapSchema PreFuneNamihe = new(27, EnemySpritemapDefinitions.PreFuneNamiheFrameCount);
    /// <summary>Schema revision 28.</summary>
    internal static readonly EnemySpritemapSchema PreKamer = new(28, EnemySpritemapDefinitions.PreKamerFrameCount);
    /// <summary>Schema revision 29.</summary>
    internal static readonly EnemySpritemapSchema PreElevator = new(29, EnemySpritemapDefinitions.PreElevatorFrameCount);
    /// <summary>Schema revision 30.</summary>
    internal static readonly EnemySpritemapSchema PreDraygonIntro = new(30, EnemySpritemapDefinitions.PreDraygonIntroFrameCount);
    /// <summary>Schema revision 31.</summary>
    internal static readonly EnemySpritemapSchema PreDraygonBreath = new(31, EnemySpritemapDefinitions.PreDraygonBreathFrameCount);
    /// <summary>Schema revision 32.</summary>
    internal static readonly EnemySpritemapSchema PreRoomSpriteObject = new(32, EnemySpritemapDefinitions.PreRoomSpriteObjectFrameCount);
    /// <summary>Schema revision 33.</summary>
    internal static readonly EnemySpritemapSchema PreYappingMaw = new(33, EnemySpritemapDefinitions.PreYappingMawFrameCount);
    /// <summary>Schema revision 34.</summary>
    internal static readonly EnemySpritemapSchema PreKiHunter = new(34, EnemySpritemapDefinitions.PreKiHunterFrameCount);
    /// <summary>Schema revision 35.</summary>
    internal static readonly EnemySpritemapSchema PreMotherBrain = new(35, EnemySpritemapDefinitions.PreMotherBrainFrameCount);
    /// <summary>Schema revision 36.</summary>
    internal static readonly EnemySpritemapSchema PreDeadTorizo = new(36, EnemySpritemapDefinitions.PreDeadTorizoFrameCount);
    /// <summary>Schema revision 37.</summary>
    internal static readonly EnemySpritemapSchema PreRidleySupplement = new(37, EnemySpritemapDefinitions.PreRidleySupplementFrameCount);
    /// <summary>Schema revision 38.</summary>
    internal static readonly EnemySpritemapSchema PreSciser = new(38, EnemySpritemapDefinitions.PreSciserFrameCount);
    /// <summary>Schema revision 39.</summary>
    internal static readonly EnemySpritemapSchema PreFly = new(39, EnemySpritemapDefinitions.PreFlyFrameCount);
    /// <summary>Schema revision 40.</summary>
    internal static readonly EnemySpritemapSchema PreKago = new(40, EnemySpritemapDefinitions.PreKagoFrameCount);
    /// <summary>Schema revision 41.</summary>
    internal static readonly EnemySpritemapSchema PreFaceBlock = new(41, EnemySpritemapDefinitions.PreFaceBlockFrameCount);
    /// <summary>Schema revision 42.</summary>
    internal static readonly EnemySpritemapSchema PreMorphBallEye = new(42, EnemySpritemapDefinitions.PreMorphBallEyeFrameCount);
    /// <summary>Schema revision 43.</summary>
    internal static readonly EnemySpritemapSchema PreShutter = new(43, EnemySpritemapDefinitions.PreShutterFrameCount);
    /// <summary>Schema revision 44.</summary>
    internal static readonly EnemySpritemapSchema PreMetroid = new(44, EnemySpritemapDefinitions.PreMetroidFrameCount);
    /// <summary>Schema revision 45.</summary>
    internal static readonly EnemySpritemapSchema PreShaktool = new(45, EnemySpritemapDefinitions.PreShaktoolFrameCount);
    /// <summary>Schema revision 46.</summary>
    internal static readonly EnemySpritemapSchema PreTripperKamer = new(46, EnemySpritemapDefinitions.PreTripperKamerFrameCount);
    /// <summary>Schema revision 47.</summary>
    internal static readonly EnemySpritemapSchema PreDragon = new(47, EnemySpritemapDefinitions.PreDragonFrameCount);
    /// <summary>Schema revision 48.</summary>
    internal static readonly EnemySpritemapSchema PreMultiviola = new(48, EnemySpritemapDefinitions.PreMultiviolaFrameCount);
    /// <summary>Schema revision 49.</summary>
    internal static readonly EnemySpritemapSchema PreNorfairLavaJumper = new(49, EnemySpritemapDefinitions.PreNorfairLavaJumperFrameCount);
    /// <summary>Schema revision 50.</summary>
    internal static readonly EnemySpritemapSchema PreChozoStatue = new(50, EnemySpritemapDefinitions.PreChozoStatueFrameCount);
    /// <summary>Schema revision 51.</summary>
    internal static readonly EnemySpritemapSchema PreViola = new(51, EnemySpritemapDefinitions.PreViolaFrameCount);
    /// <summary>Schema revision 52.</summary>
    internal static readonly EnemySpritemapSchema PreRinka = new(52, EnemySpritemapDefinitions.PreRinkaFrameCount);
    /// <summary>Schema revision 53.</summary>
    internal static readonly EnemySpritemapSchema PreDeadTorizoStationary = new(53, EnemySpritemapDefinitions.PreDeadTorizoStationaryFrameCount);
    /// <summary>Schema revision 54.</summary>
    internal static readonly EnemySpritemapSchema PreDeadTourianCorpse = new(54, EnemySpritemapDefinitions.PreDeadTourianCorpseFrameCount);
    /// <summary>Schema revision 55.</summary>
    internal static readonly EnemySpritemapSchema PreMochtroid = new(55, EnemySpritemapDefinitions.PreMochtroidFrameCount);
    /// <summary>Schema revision 56.</summary>
    internal static readonly EnemySpritemapSchema PreEvir = new(56, EnemySpritemapDefinitions.PreEvirFrameCount);
    /// <summary>Schema revision 57.</summary>
    internal static readonly EnemySpritemapSchema PreWorkRobot = new(57, EnemySpritemapDefinitions.PreWorkRobotFrameCount);
    /// <summary>Schema revision 58.</summary>
    internal static readonly EnemySpritemapSchema PreYard = new(58, EnemySpritemapDefinitions.PreYardFrameCount);
    /// <summary>Schema revision 59.</summary>
    internal static readonly EnemySpritemapSchema PreBotwoon = new(59, EnemySpritemapDefinitions.PreBotwoonFrameCount);
    /// <summary>Schema revision 60.</summary>
    internal static readonly EnemySpritemapSchema PreGunship = new(60, EnemySpritemapDefinitions.PreGunshipFrameCount);
    /// <summary>Schema before the tatori family; previous compositions and bindings remain editable.</summary>
    internal static readonly EnemySpritemapSchema PreMamaTurtle = new(61, EnemySpritemapDefinitions.PreMamaTurtleFrameCount);
    /// <summary>Schema before the Zero crawler; existing edits and bindings remain valid.</summary>
    internal static readonly EnemySpritemapSchema PreZero = new(62, EnemySpritemapDefinitions.PreZeroFrameCount);
    /// <summary>Schema before the friendly-animal compositions; existing edits remain valid.</summary>
    internal static readonly EnemySpritemapSchema PreFriendlyAnimal = new(63, EnemySpritemapDefinitions.PreFriendlyAnimalFrameCount);
    /// <summary>Schema before audit-identified ordinary environmental/Tourian artwork additions.</summary>
    internal static readonly EnemySpritemapSchema PreAuditOrdinary = new(64, EnemySpritemapDefinitions.PreAuditOrdinaryFrameCount);
    /// <summary>Schema before Kraid's two ordinary belly-lint frames were installed.</summary>
    internal static readonly EnemySpritemapSchema PreKraidLint = new(65, EnemySpritemapDefinitions.PreKraidLintFrameCount);
    /// <summary>Schema before Puromi/Nuclear Waffle's eight head frames were installed.</summary>
    internal static readonly EnemySpritemapSchema PreNuclearWaffle = new(66, EnemySpritemapDefinitions.PreNuclearWaffleFrameCount);
    /// <summary>Schema before the single-frame Kzan and Polyp compositions were installed.</summary>
    internal static readonly EnemySpritemapSchema PreSingleFrame = new(67, EnemySpritemapDefinitions.PreSingleFrameFrameCount);
    /// <summary>Schema before Ridley's eight ordinary breakup body compositions.</summary>
    internal static readonly EnemySpritemapSchema PreRidleyBreakup = new(68, EnemySpritemapDefinitions.PreRidleyBreakupFrameCount);
    /// <summary>The current schema, holding every named frame.</summary>
    internal static readonly EnemySpritemapSchema Current = new(69, EnemySpritemapDefinitions.Frames.Length);

    /// <summary>Every accepted revision, oldest first.</summary>
    internal static readonly ImmutableArray<EnemySpritemapSchema> Supported =
    [
        Legacy,
        Intermediate,
        Earlier,
        Prior,
        Previous,
        PreOwtchStoke,
        PreRipper,
        PreFireflea,
        PreMagdollite,
        PreDisplayBindings,
        PreCeresDoor,
        PreCeresBaby,
        PreRio,
        PreLowerNorfairRio,
        PreNorfairRio,
        PrePuyo,
        PreBull,
        PreAlcoon,
        PreBeetom,
        PreHopper,
        PreChoot,
        PreHZoomer,
        PreSbug,
        PreFuneNamihe,
        PreKamer,
        PreElevator,
        PreDraygonIntro,
        PreDraygonBreath,
        PreRoomSpriteObject,
        PreYappingMaw,
        PreKiHunter,
        PreMotherBrain,
        PreDeadTorizo,
        PreRidleySupplement,
        PreSciser,
        PreFly,
        PreKago,
        PreFaceBlock,
        PreMorphBallEye,
        PreShutter,
        PreMetroid,
        PreShaktool,
        PreTripperKamer,
        PreDragon,
        PreMultiviola,
        PreNorfairLavaJumper,
        PreChozoStatue,
        PreViola,
        PreRinka,
        PreDeadTorizoStationary,
        PreDeadTourianCorpse,
        PreMochtroid,
        PreEvir,
        PreWorkRobot,
        PreYard,
        PreBotwoon,
        PreGunship,
        PreMamaTurtle,
        PreZero,
        PreFriendlyAnimal,
        PreAuditOrdinary,
        PreKraidLint,
        PreNuclearWaffle,
        PreSingleFrame,
        PreRidleyBreakup,
        Current,
    ];

    /// <summary>
    /// Whether documents of this revision author their own display bindings: every revision after
    /// <see cref="PreDisplayBindings"/>; earlier revisions inherit the stock bindings.
    /// </summary>
    internal bool OwnsDisplayBindings => Supported.IndexOf(this) > Supported.IndexOf(PreDisplayBindings);

    /// <summary>The accepted revision a document stores as <paramref name="revision"/>, or null when none is.</summary>
    internal static EnemySpritemapSchema? FromRevision(int revision) => Supported.FirstOrDefault(schema => schema.Revision == revision);
}
