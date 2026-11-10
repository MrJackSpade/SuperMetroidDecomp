namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The order in which a room's, or a frontend screen's, colour effects take their turns each update (#1592): from the
/// lowest of eight turns up, a starting effect taking the lowest turn no running effect holds, and a start while all
/// eight are held is ignored. It is observable in two ways. Samus's heat shimmer, started first in every heated room,
/// takes its turn before the Norfair glow that publishes the heat phase, so the shimmer, and the heat damage's phase
/// with it, follows a new phase one update late. Where two effects show the same colours in one update, the later turn's
/// record is the one that shows, as when Mother Brain's death starts Tourian's escape red flash over the room's glowing
/// Arkanoid blocks and orbs.
/// </summary>
/// <remarks>
/// Consulted when an effect starts (its turn and its place among the running effects) and by the order in which every
/// update of the colour effects visits them. Evidence: the original colour-effect handler visits its eight effect places
/// from the last down, and a new effect takes the last free place, or none while every place is taken; the heat
/// shimmer's condition reads the heat phase the glow's later visit writes.
/// </remarks>
[CompatibilityService("a room's colour effects take turns from the lowest of eight turns up, so the heat shimmer follows the Norfair glow's heat phase one update late")]
public static class ColourEffectTurnOrder
{
    /// <summary>The turns there are: at most this many effects run at once.</summary>
    public const int TurnCount = 8;

    /// <summary>The turn an effect starting now takes: the lowest turn no running effect holds.</summary>
    /// <typeparam name="TEffect">A running effect.</typeparam>
    /// <param name="running">The running effects.</param>
    /// <param name="turnOf">The turn a running effect holds.</param>
    /// <returns>The turn, or null while every turn is held, so the start is ignored.</returns>
    public static int? TurnForStart<TEffect>(List<TEffect> running, Func<TEffect, int> turnOf)
    {
        Ensure.NotNull(running);
        Ensure.NotNull(turnOf);
        for (int turn = 0; turn < TurnCount; turn++)
        {
            if (!running.Exists(effect => turnOf(effect) == turn))
                return turn;
        }
        return null;
    }

    /// <summary>Adds <paramref name="started"/> to <paramref name="running"/>, which stays in turn order, lowest first.</summary>
    /// <typeparam name="TEffect">A running effect.</typeparam>
    /// <param name="running">The running effects, in turn order.</param>
    /// <param name="started">The effect that started on the turn <see cref="TurnForStart"/> gave it.</param>
    /// <param name="turnOf">The turn a running effect holds.</param>
    public static void Join<TEffect>(List<TEffect> running, TEffect started, Func<TEffect, int> turnOf)
    {
        Ensure.NotNull(running);
        Ensure.NotNull(turnOf);
        int turn = turnOf(started);
        int place = running.FindIndex(effect => turnOf(effect) > turn);
        running.Insert(place < 0 ? running.Count : place, started);
    }
}
