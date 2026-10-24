namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// An enemy whose frames show a rotting corpse's rot (#1406). The common enemy draw asks the bound entity for the rot it
/// selected, and names the picture of each frame drawn over the corpse's rows showing it.
/// </summary>
internal interface IShowsCorpseRot : IEnemyEntity
{
    /// <summary>The rot the enemy's frames show now.</summary>
    CorpseRotView ShownRot { get; }
}
