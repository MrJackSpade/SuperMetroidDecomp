namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// An enemy whose frames show its own rotting corpse's rows as its rot has left them (#1539). The common enemy draw asks
/// the bound entity for those rows and draws each of its frames that shows the corpse whole through them.
/// </summary>
internal interface IShowsCorpseRotRows : IEnemyEntity
{
    /// <summary>The rows the enemy's corpse frames show now, every row itself before the corpse's first rot call.</summary>
    CorpseRotRows ShownRotRows { get; }
}
