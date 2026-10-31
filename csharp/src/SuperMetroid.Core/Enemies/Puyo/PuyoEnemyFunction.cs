namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// The routine Puyo's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum PuyoEnemyFunction
{
    /// <summary>Decrements the hop cooldown and selects an airborne hop when the countdown becomes negative.</summary>
    Grounded = 1,
    /// <summary>Dispatches the current hop or collision-recovery routine through the airborne function word.</summary>
    Airborne,
}
