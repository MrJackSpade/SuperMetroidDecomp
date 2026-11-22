namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The speed-boost echoes of one Dachora's body (#2147), in the order its room lists them after it (#2554). Each echo joins
/// as it is built, after the body, so the body places and times every echo it trails through the echo itself; an echo
/// built again in its place takes the place of the one it replaces.
/// </summary>
internal sealed partial class DachoraEchoTrail
{
    /// <summary>The joined echoes, in the order the room lists them.</summary>
    private readonly List<DachoraEchoEntity> _joined = [];

    /// <summary>The joined echoes, in the order the room lists them.</summary>
    internal IReadOnlyList<DachoraEchoEntity> Joined => _joined;

    /// <summary>Joins <paramref name="echo"/> to the trail as it is built, in the place of an echo built before in its place.</summary>
    /// <returns>The echo.</returns>
    internal DachoraEchoEntity Join(DachoraEchoEntity echo)
    {
        int earlier = _joined.FindIndex(joined => joined.Place == Ensure.NotNull(echo).Place);
        if (earlier < 0)
            _joined.Add(echo);
        else
            _joined[earlier] = echo;
        return echo;
    }
}
