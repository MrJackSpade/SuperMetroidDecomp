using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed Samus overread file (#1474): what each of Samus's glitch reads yields, decoded from the player's
/// cartridge and keyed by the game situation that reaches it, never by a cartridge place.
/// </summary>
/// <param name="Version">The schema revision, <see cref="SamusOverreadFormat.Version"/>.</param>
/// <param name="AnimationCueValueSteps">
/// For every animation, by its name, the step it reads at each frame of its program that lands on a value one of its cues
/// carries rather than on a step of its own, by the frame's name in the animation: its segment's name and the frame
/// within the segment (#1535, #2048). A place whose read would name a record no gameplay path
/// gives Samus has no entry.
/// </param>
/// <param name="AnimationFramesAfterCues">
/// For every animation, by its name, the frame each frame on or past the cue ending one of its segments other than its last
/// goes on to, both by the frame's name in the animation (#2395).
/// </param>
/// <param name="AnimationReadsPast">
/// For every animation, by its name, what each frame past its end that the animation can reach reads, by the frame's name
/// in the animation (#2048).
/// </param>
/// <param name="AnimationPlaceLandings">
/// For every named place of Samus's animations, the frame it lands on in every animation other than the place's own, by
/// place name, then by animation name (#2048).
/// </param>
/// <param name="SpaceTimeOverwrites">
/// What SpaceTime's palette copy overwrites in progression, by program name, then by step position, then by the shot
/// place's name (<see cref="SamusOverreadFormat.ShotPlaceName"/>); null where the copy stops before progression.
/// </param>
/// <param name="ChainsawFuseLandings">Where the Chainsaw fuse moves each animation position, by program name, then by step position.</param>
/// <param name="PostureAdmissions">Whether the posture-transition admission admits weapons, by the name of each pose outside its table.</param>
/// <param name="UnauthoredBeamLaunchSpeeds">The speeds each Spazer and Plasma beam combination launches at, by combination name.</param>
/// <param name="UnauthoredBeamCooldowns">The cooldowns each Spazer and Plasma beam combination waits, by combination name.</param>
/// <param name="DrainedCrashDepartures">The angles a Shinespark crash finishing in each drained pose releases its echoes at, by pose name.</param>
internal sealed partial record SamusOverreadDocument(
    int Version,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> AnimationCueValueSteps,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> AnimationFramesAfterCues,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast>> AnimationReadsPast,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> AnimationPlaceLandings,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Overwrite?>>> SpaceTimeOverwrites,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor>> ChainsawFuseLandings,
    IReadOnlyDictionary<string, bool> PostureAdmissions,
    IReadOnlyDictionary<string, SamusOverreadDocument.LaunchSpeeds> UnauthoredBeamLaunchSpeeds,
    IReadOnlyDictionary<string, SamusOverreadDocument.Cooldowns> UnauthoredBeamCooldowns,
    IReadOnlyDictionary<string, SamusOverreadDocument.CrashDepartures> DrainedCrashDepartures)
{
    /// <summary>What a frame past an animation's end reads: a frame of an animation laid out after it (#2048).</summary>
    /// <param name="Animation">The name of the later animation.</param>
    /// <param name="Frame">The name of the frame of the later animation the read reaches, which names a step of it or a value one of its cues carries.</param>
    /// <param name="Moves">
    /// For each frame of the later animation the cue read there names, by that frame's name, the name of the frame of the
    /// animation read past that the cue moves it to; empty when the step read names no frame.
    /// </param>
    internal sealed partial record ReadPast([InstalledKey] string Animation, [InstalledKey] string Frame, IReadOnlyDictionary<string, string> Moves);

    /// <summary>The step the animation reads at a cue value's place: what the game does on reaching that frame (#1535).</summary>
    /// <param name="Kind">What the step does.</param>
    /// <param name="Ticks">A delay's ticks; null for any other kind.</param>
    /// <param name="Frames">
    /// The names of the frames of the pose's animation the step selects: a repeat's one frame, or the wall-jump selection's
    /// ordinary, Space Jump and Screw Attack frames; null for any other kind.
    /// </param>
    /// <param name="Items">The names of the items an item transition tests; null for any other kind.</param>
    /// <param name="Poses">
    /// The names of the poses a transition publishes: a plain or turn transition's one pose; an item transition's unequipped
    /// and equipped poses; an item and vertical-speed transition's unequipped grounded, unequipped airborne, equipped
    /// grounded and equipped airborne poses; a vertical-speed transition's grounded and airborne poses; null for any other
    /// kind.
    /// </param>
    internal sealed partial record CueValueStep([InstalledKey] string Kind, int? Ticks, [InstalledKey] IReadOnlyList<string>? Frames,
        [InstalledKey] IReadOnlyList<string>? Items, [InstalledKey] IReadOnlyList<string>? Poses);

    /// <summary>What the step at a cue value's place does, as the game does on reading that value as a step.</summary>
    internal enum CueValueStepKind
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

    /// <summary>
    /// What the copy turns on or off in progress, and the startup route it leaves when it reaches it (#1475), each by the
    /// identities the game knows it by (#1709).
    /// </summary>
    /// <param name="Events">The story events, by name, the copy decides.</param>
    /// <param name="ZebetitesDestroyed">The Zebetite count the copy leaves, 0 through 4, or null when it stops before deciding it.</param>
    /// <param name="AreaBosses">The defeated bosses the copy leaves, by area name, each as the names of its boss flags.</param>
    /// <param name="CollectedItems">The item locations, by name, the copy decides.</param>
    /// <param name="OpenedDoors">The doors, by name, the copy decides.</param>
    /// <param name="UsedStations">The used save stations and elevators, by name, the copy decides.</param>
    /// <param name="AreaMaps">Whether the copy leaves each area's map acquired, by area name.</param>
    /// <param name="SavedStartupRoute">The name of the saved startup route the copy leaves, or null when it stops before it.</param>
    internal sealed partial record Overwrite(
        DecidedMembers Events,
        int? ZebetitesDestroyed,
        IReadOnlyDictionary<string, IReadOnlyList<string>> AreaBosses,
        DecidedMembers CollectedItems,
        DecidedMembers OpenedDoors,
        DecidedMembers UsedStations,
        IReadOnlyDictionary<string, bool> AreaMaps,
        [InstalledKey] string? SavedStartupRoute);

    /// <summary>What an overwrite decides for one kind of progress: the members whose state it decides, and those it leaves on.</summary>
    /// <param name="Decided">The names of the members whose state the overwrite decides; every other member keeps its state.</param>
    /// <param name="Set">The names of the decided members the overwrite leaves on; the other decided members it turns off.</param>
    internal sealed partial record DecidedMembers([InstalledKey] IReadOnlyList<string> Decided, [InstalledKey] IReadOnlyList<string> Set);

    /// <summary>An animation position: an animation and its step.</summary>
    /// <param name="Animation">The animation's name.</param>
    /// <param name="Position">The step's position in the animation.</param>
    internal sealed partial record Cursor([InstalledKey] string Animation, int Position);

    /// <summary>The speeds a beam launches at, each in pixels per update with its fraction of a pixel kept exactly.</summary>
    /// <param name="Cardinal">The horizontal and vertical speed.</param>
    /// <param name="Diagonal">The diagonal speed.</param>
    internal sealed partial record LaunchSpeeds(decimal Cardinal, decimal Diagonal);

    /// <summary>The cooldowns a beam's shots wait, each 0 through 255 updates.</summary>
    /// <param name="Uncharged">After an uncharged shot fired with Charge equipped or on a fresh press.</param>
    /// <param name="Charged">After a charged shot.</param>
    /// <param name="HeldFire">After an uncharged shot repeated by holding the button.</param>
    internal sealed partial record Cooldowns(int Uncharged, int Charged, int HeldFire);

    /// <summary>The two angles a crash's echoes leave at, each 0 through 255 steps of the 256-step turn.</summary>
    /// <param name="First">The first echo's angle.</param>
    /// <param name="Second">The second echo's angle.</param>
    internal sealed partial record CrashDepartures(int First, int Second);
}
