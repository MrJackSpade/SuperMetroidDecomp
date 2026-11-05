namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// An enemy whose frames show a rotting corpse's rows as its rot has left them (#1531). The common enemy draw asks the
/// bound entity for those rows and draws each of its frames that shows the corpse through them.
/// </summary>
internal interface IShowsCorpseRotRows : IEnemyEntity
{
    /// <summary>The rows the enemy's corpse frames show now; none before the corpse's rows have been shown rotting.</summary>
    CorpseRotRows? ShownRotRows { get; }
}
