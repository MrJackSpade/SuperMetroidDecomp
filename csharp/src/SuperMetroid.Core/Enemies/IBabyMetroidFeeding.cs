namespace SuperMetroid.Core.Enemies;

/// <summary>The Baby Metroid's feeding on its Tourian victim, as the victim asks about it when it is placed.</summary>
internal interface IBabyMetroidFeeding
{
    /// <summary>Whether the Baby Metroid that feeds on <paramref name="victim"/> has already fed, so its victim is gone.</summary>
    /// <param name="victim">The victim being placed.</param>
    bool HasAlreadyFedOn(RoomEnemyPlace victim);

    /// <summary>The drain the Baby Metroid placed directly before <paramref name="victim"/> made for it at its own setup.</summary>
    /// <param name="victim">The victim being placed.</param>
    /// <exception cref="InvalidDataException">The room places no Baby Metroid directly before the victim.</exception>
    Corpses.SidehopperDrain DrainOf(RoomEnemyPlace victim);
}
