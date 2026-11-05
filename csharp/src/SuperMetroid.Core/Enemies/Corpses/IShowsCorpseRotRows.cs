using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// An enemy whose frames show a rotting corpse's rows as its rot has left them (#1531). The common enemy draw asks the
/// bound entity for those rows and draws its frame through them.
/// </summary>
internal interface IShowsCorpseRotRows : IEnemyEntity
{
    /// <summary>The rows the enemy's frames show now.</summary>
    CorpseRotRows ShownRotRows { get; }
}
