using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed Samus overread file (#1474): what each of Samus's glitch reads yields, decoded from the player's
/// cartridge and keyed by the game situation that reaches it, never by a cartridge place.
/// </summary>
/// <param name="Version">The schema revision, <see cref="SamusOverreadFormat.Version"/>.</param>
/// <param name="AnimationContinuations">
/// For every pose with an animation program, the program laid out after its own, named by a pose that animates by it,
/// and the frame shift; null for the last program laid out.
/// </param>
/// <param name="AnimationOperandSteps">
/// For every pose with an animation program, the step the animation reads at each frame of its program that lands on an
/// instruction's operand rather than on a step of its own, by frame, in the program's own frame numbering (#1535). A place
/// whose read would name a record no gameplay path gives Samus has no entry.
/// </param>
/// <param name="SpaceTimeOverwrites">
/// What SpaceTime's palette copy overwrites in progression, by program name, then by step position, then by slot
/// ordinal; null where the copy stops before progression.
/// </param>
/// <param name="ChainsawFuseLandings">Where the Chainsaw fuse moves each animation position, by program name, then by step position.</param>
/// <param name="ChargeFlareDelaysBelowPrograms">The delays a charge-flare frame reads below the flare's programs, nearest first.</param>
/// <param name="PostureAdmissions">Whether the posture-transition admission admits weapons, by the name of each pose outside its table.</param>
/// <param name="SpeedBoosterOverrunColours">The colours Gravity copies on each Speed Booster step past its cycle, by step name.</param>
/// <param name="UnauthoredBeamLaunchSpeeds">The speeds each Spazer and Plasma beam combination launches at, by combination name.</param>
/// <param name="UnauthoredBeamCooldowns">The cooldowns each Spazer and Plasma beam combination waits, by combination name.</param>
/// <param name="DrainedCrashDepartures">The angles a Shinespark crash finishing in each drained pose releases its echoes at, by pose name.</param>
/// <param name="ChainsawBeamColours">The sixteen colours the Chainsaw beam's palette holds, in palette order.</param>
internal sealed record SamusOverreadDocument(
    int Version,
    IReadOnlyDictionary<string, SamusOverreadDocument.Continuation?> AnimationContinuations,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.OperandStep>> AnimationOperandSteps,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>> SpaceTimeOverwrites,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor>> ChainsawFuseLandings,
    IReadOnlyList<int> ChargeFlareDelaysBelowPrograms,
    IReadOnlyDictionary<string, bool> PostureAdmissions,
    IReadOnlyDictionary<string, IReadOnlyList<PaletteRgb5>> SpeedBoosterOverrunColours,
    IReadOnlyDictionary<string, SamusOverreadDocument.LaunchSpeeds> UnauthoredBeamLaunchSpeeds,
    IReadOnlyDictionary<string, SamusOverreadDocument.Cooldowns> UnauthoredBeamCooldowns,
    IReadOnlyDictionary<string, SamusOverreadDocument.CrashDepartures> DrainedCrashDepartures,
    IReadOnlyList<PaletteRgb5> ChainsawBeamColours)
{
    /// <summary>The program laid out after another, named by a pose that animates by it.</summary>
    /// <param name="Pose">The name of a pose that animates by the later program.</param>
    /// <param name="Shift">How many frames after the earlier program it starts.</param>
    internal sealed record Continuation(string Pose, int Shift);

    /// <summary>The step the animation reads at an operand's place: what the game does on reaching that frame (#1535).</summary>
    /// <param name="Kind">What the step does.</param>
    /// <param name="Ticks">A delay's ticks; null for any other kind.</param>
    /// <param name="Frames">
    /// The frames the step selects, in the program's own numbering: a repeat's one frame, or the wall-jump selection's
    /// ordinary, Space Jump and Screw Attack frames; null for any other kind.
    /// </param>
    /// <param name="Items">The names of the items an item transition tests; null for any other kind.</param>
    /// <param name="Poses">
    /// The names of the poses a transition publishes: a plain or turn transition's one pose; an item transition's unequipped
    /// and equipped poses; an item and vertical-speed transition's unequipped grounded, unequipped airborne, equipped
    /// grounded and equipped airborne poses; a vertical-speed transition's grounded and airborne poses; null for any other
    /// kind.
    /// </param>
    internal sealed record OperandStep(string Kind, int? Ticks, IReadOnlyList<int>? Frames, IReadOnlyList<string>? Items, IReadOnlyList<string>? Poses);

    /// <summary>What the step at an operand's place does, as the animation's handler for it does.</summary>
    internal enum OperandStepKind
    {
        /// <summary>Holds the picture for its ticks, then moves on to the next frame.</summary>
        Delay,

        /// <summary>Keeps the frame and timer for a tick, then moves on to the next frame.</summary>
        Hold,

        /// <summary>Restarts the program when Samus is healthy, otherwise moves on to the next frame.</summary>
        LoopUnlessLowEnergy,

        /// <summary>Hands Samus's movement to the drained fall, then moves on to the next frame.</summary>
        InstallDrainedFall,

        /// <summary>Publishes a pose as a turn or landing does, unless the pose input chose a jump.</summary>
        TurnTransition,

        /// <summary>Publishes one of four poses by the tested items and whether Samus moves vertically.</summary>
        ItemAirborneTransition,

        /// <summary>Publishes one of two poses by whether Samus moves vertically.</summary>
        VerticalSpeedTransition,

        /// <summary>Selects the wall jump's ordinary, Space Jump or Screw Attack frames.</summary>
        WallJumpSelect,

        /// <summary>Publishes one of two poses by the tested items.</summary>
        ItemTransition,

        /// <summary>Publishes a pose.</summary>
        Transition,

        /// <summary>Goes back to an earlier frame.</summary>
        Repeat,

        /// <summary>Goes back to the program's first frame.</summary>
        Restart,
    }

    /// <summary>What the copy turns on or off in progress, and the loading state it leaves when it reaches it (#1475).</summary>
    /// <param name="Events">The story events, by number, the copy decides.</param>
    /// <param name="AreaBosses">The defeated bosses the copy leaves, by area name, each as the names of its boss flags.</param>
    /// <param name="CollectedItems">The item locations, by room-data number, the copy decides.</param>
    /// <param name="OpenedDoors">The doors, by room-data number, the copy decides.</param>
    /// <param name="UsedStations">The used stations, by station number, the copy decides, by area name.</param>
    /// <param name="AreaMaps">Whether the copy leaves each area's map acquired, by area name.</param>
    /// <param name="SavedLoadingGameState">The saved loading state written, or null when the copy stops before it.</param>
    internal sealed record Overwrite(
        FlagOverwrite Events,
        IReadOnlyDictionary<string, IReadOnlyList<string>> AreaBosses,
        FlagOverwrite CollectedItems,
        FlagOverwrite OpenedDoors,
        IReadOnlyDictionary<string, FlagOverwrite> UsedStations,
        IReadOnlyDictionary<string, bool> AreaMaps,
        int? SavedLoadingGameState);

    /// <summary>An animation position: a program and its step.</summary>
    /// <param name="Program">The program's name.</param>
    /// <param name="Position">The step's position in the program.</param>
    internal sealed record Cursor(string Program, int Position);

    /// <summary>The speeds a beam launches at, each a signed count of 256ths of a pixel per update.</summary>
    /// <param name="Cardinal">The horizontal and vertical speed.</param>
    /// <param name="Diagonal">The diagonal speed.</param>
    internal sealed record LaunchSpeeds(int Cardinal, int Diagonal);

    /// <summary>The cooldowns a beam's shots wait, each 0 through 255 updates.</summary>
    /// <param name="Uncharged">After an uncharged shot fired with Charge equipped or on a fresh press.</param>
    /// <param name="Charged">After a charged shot.</param>
    /// <param name="HeldFire">After an uncharged shot repeated by holding the button.</param>
    internal sealed record Cooldowns(int Uncharged, int Charged, int HeldFire);

    /// <summary>The two angles a crash's echoes leave at, each 0 through 255 steps of the 256-step turn.</summary>
    /// <param name="First">The first echo's angle.</param>
    /// <param name="Second">The second echo's angle.</param>
    internal sealed record CrashDepartures(int First, int Second);
}
