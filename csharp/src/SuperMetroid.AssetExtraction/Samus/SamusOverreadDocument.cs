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
/// <param name="TrailFramesAtListStart">The animation frame a trail reads from a shot whose list has just started, by program name.</param>
/// <param name="TrailPlacementsPastRecords">
/// What each trail placement list yields past its run of placements, by list name, then by the frame read; null where
/// the read reaches scratch memory.
/// </param>
/// <param name="SpaceTimeOverwrites">
/// What SpaceTime's palette copy overwrites in progression, by program name, then by step position, then by slot
/// ordinal; null where the copy stops before progression.
/// </param>
/// <param name="ChainsawFuseLandings">Where the Chainsaw fuse moves each animation position, by program name, then by step position.</param>
/// <param name="ChargeFlareDelaysBelowPrograms">The delays a charge-flare frame reads below the flare's programs, nearest first.</param>
/// <param name="PostureAdmissions">Whether the posture-transition admission admits weapons, by the name of each pose outside its table.</param>
/// <param name="SpeedBoosterOverrunColours">The colours Gravity copies on each Speed Booster step past its cycle, by step name.</param>
internal sealed record SamusOverreadDocument(
    int Version,
    IReadOnlyDictionary<string, SamusOverreadDocument.Continuation?> AnimationContinuations,
    IReadOnlyDictionary<string, int> TrailFramesAtListStart,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Placement?>> TrailPlacementsPastRecords,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<SamusOverreadDocument.Overwrite?>>> SpaceTimeOverwrites,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.Cursor>> ChainsawFuseLandings,
    IReadOnlyList<int> ChargeFlareDelaysBelowPrograms,
    IReadOnlyDictionary<string, bool> PostureAdmissions,
    IReadOnlyDictionary<string, IReadOnlyList<PaletteRgb5>> SpeedBoosterOverrunColours)
{
    /// <summary>The program laid out after another, named by a pose that animates by it.</summary>
    /// <param name="Pose">The name of a pose that animates by the later program.</param>
    /// <param name="Shift">How many frames after the earlier program it starts.</param>
    internal sealed record Continuation(string Pose, int Shift);

    /// <summary>Where a trail's two sparkles start, as signed offsets from the projectile.</summary>
    /// <param name="Left">The left sparkle's offset.</param>
    /// <param name="Right">The right sparkle's offset.</param>
    internal sealed record Placement(Offset16<RoomPixels> Left, Offset16<RoomPixels> Right);

    /// <summary>What the copy turns on or off in progress, and the loading state it leaves when it reaches it (#1475).</summary>
    /// <param name="Events">The story events, by number, the copy decides.</param>
    /// <param name="AreaBosses">The defeated bosses the copy leaves, by area name, each as the names of its boss flags.</param>
    /// <param name="ChozoOrbs">The Chozo orbs, by room-data number, the copy decides.</param>
    /// <param name="CollectedItems">The item locations, by room-data number, the copy decides.</param>
    /// <param name="OpenedDoors">The doors, by room-data number, the copy decides.</param>
    /// <param name="UsedStations">The used stations, by station number, the copy decides, by area name.</param>
    /// <param name="AreaMaps">Whether the copy leaves each area's map acquired, by area name.</param>
    /// <param name="SavedLoadingGameState">The saved loading state written, or null when the copy stops before it.</param>
    internal sealed record Overwrite(
        FlagOverwrite Events,
        IReadOnlyDictionary<string, IReadOnlyList<string>> AreaBosses,
        FlagOverwrite ChozoOrbs,
        FlagOverwrite CollectedItems,
        FlagOverwrite OpenedDoors,
        IReadOnlyDictionary<string, FlagOverwrite> UsedStations,
        IReadOnlyDictionary<string, bool> AreaMaps,
        int? SavedLoadingGameState);

    /// <summary>An animation position: a program and its step.</summary>
    /// <param name="Program">The program's name.</param>
    /// <param name="Position">The step's position in the program.</param>
    internal sealed record Cursor(string Program, int Position);
}
