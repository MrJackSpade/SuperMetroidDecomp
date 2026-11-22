namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The speed-boost echoes of one Dachora's body (#2147), in the order its room lists them after it (#2554). Each echo joins
/// as it is built, after the body, so the body places and times every echo it trails through the echo's place and state;
/// an echo built again in its place takes the place of the one it replaces.
/// </summary>
internal sealed partial class DachoraEchoTrail
{
    /// <summary>The joined echoes, in the order the room lists them.</summary>
    private readonly List<DachoraTrailEcho> _joined = [];

    /// <summary>The joined echoes, in the order the room lists them.</summary>
    internal IReadOnlyList<DachoraTrailEcho> Joined => _joined;

    /// <summary>Joins <paramref name="echo"/> to the trail as it is built, in the place of an echo built before in its place.</summary>
    /// <returns>The echo.</returns>
    internal DachoraEchoEntity Join(DachoraEchoEntity echo)
    {
        var joining = new DachoraTrailEcho(Ensure.NotNull(echo).Place, echo.State);
        int earlier = _joined.FindIndex(joined => joined.Place == joining.Place);
        if (earlier < 0)
            _joined.Add(joining);
        else
            _joined[earlier] = joining;
        return echo;
    }
}
