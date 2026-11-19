namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The events a room's enemies have published since their pass, or their room's load, began (#2562), in the order they
/// published them. Each pass and each load starts a new record, so a paused or frozen enemy never repeats an event; the
/// record is what the pass returns.
/// </summary>
internal sealed class EnemyPassEvents
{
    /// <summary>The events published since the pass or the load began.</summary>
    private List<EnemyPassEvent> _published = [];

    /// <summary>The events published since the pass or the load began, in publication order.</summary>
    internal IReadOnlyList<EnemyPassEvent> Published => _published;

    /// <summary>Starts a pass's or a load's record; the previous record, which an earlier result holds, is left as it was.</summary>
    internal void Begin() => _published = [];

    /// <summary>Publishes <paramref name="passEvent"/> after the events published before it.</summary>
    internal void Publish(EnemyPassEvent passEvent) => _published.Add(Ensure.NotNull(passEvent));
}
