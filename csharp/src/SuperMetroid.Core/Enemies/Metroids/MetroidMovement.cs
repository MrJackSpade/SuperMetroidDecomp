namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The ordinary Metroid's movement: each update it moves in one of these four ways.
/// </summary>
public enum MetroidMovement
{
    /// <summary>Chases Samus: pulled toward her X and above her Y (<see cref="MetroidBehaviorDefinitions.AimAboveSamus"/>), harder the further it is, at no more than <see cref="MetroidBehaviorDefinitions.FastestChase"/>.</summary>
    Homing,
    /// <summary>Closes on Samus: seeks her at close range, entered on touch, until it is near enough to latch on.</summary>
    ClosingOnSamus,
    /// <summary>Holds on to Samus: it stays at her X, above her Y by <see cref="MetroidBehaviorDefinitions.AimAboveSamus"/>, and does not move of its own accord.</summary>
    AttachedToSamus,
    /// <summary>Shaken off Samus by a Power Bomb: it makes its escape's four moves, then chases her again.</summary>
    PowerBombEscape,
}
