namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The order in which a room's, or a frontend screen's, colour effects take their turns each update (#1592, #1598): the
/// order they started in. It is observable through Samus's heat shimmer: started first in every heated room, it takes its
/// turn before the room's heat glow that publishes the heat phase, so the shimmer, which restarts her suit's pulse on that
/// phase, follows a new phase one update late.
/// </summary>
/// <remarks>
/// Consulted when an effect starts, which joins after every running effect, and by the order in which every update of
/// the colour effects visits them. Evidence: the original game's colour effects take their turns from the first started
/// on, a room's FX record starts its effects in the record's order into a room with none running, and the heat shimmer's
/// condition reads the heat phase the glow's later visit writes. The effects whose turns gameplay observes, the heat
/// shimmer that burns Samus, the heat glow that paces it and the beacon that sounds, are all started by the room's FX
/// record and run until the room ends, so no ended effect's turn is ever handed to an effect started before them. Where
/// the original handed a later start an ended effect's place, or refused a start while eight effects ran, only the
/// colours shown differed: no effect started outside a room's FX record burns, sounds or paces the heat, and a record
/// starts at most eight.
/// </remarks>
[CompatibilityService("a room's colour effects take turns in the order they started, so the heat shimmer follows the heated room's glow's heat phase one update late")]
public static class ColourEffectTurnOrder
{
    /// <summary>Adds <paramref name="started"/> to <paramref name="running"/>, after every effect already running.</summary>
    /// <typeparam name="TEffect">A running effect.</typeparam>
    /// <param name="running">The running effects, in turn order.</param>
    /// <param name="started">The effect that starts now.</param>
    internal static void Join<TEffect>(List<TEffect> running, TEffect started)
    {
        Ensure.NotNull(running);
        running.Add(started);
    }
}
