using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One cue of Ridley's body programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Target">The program a goto cue continues at.</param>
/// <param name="FeetDistanceIndex">
/// The claw-height pose a <see cref="RidleyBodyCueKind.SetFeetDistance"/> cue selects: zero standing, one legs half
/// extended, two legs extended.
/// </param>
/// <param name="Displacement">How far a <see cref="RidleyBodyCueKind.Move"/> cue moves Ridley.</param>
internal readonly record struct RidleyBodyCue(
    RidleyBodyCueKind Kind,
    RidleyBodyProgram Target = default,
    int FeetDistanceIndex = 0,
    Offset16<RoomPixels> Displacement = default);
