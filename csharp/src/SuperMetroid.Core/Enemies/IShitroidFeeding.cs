namespace SuperMetroid.Core.Enemies;

/// <summary>The Shitroid's feeding on its Tourian victim, as the victim asks about it when it is placed.</summary>
internal interface IShitroidFeeding
{
    /// <summary>Whether the Shitroid that feeds on <paramref name="victim"/> has already fed, so its victim is gone.</summary>
    /// <param name="victim">The victim being placed.</param>
    bool HasAlreadyFedOn(RoomEnemyPlace victim);
}
