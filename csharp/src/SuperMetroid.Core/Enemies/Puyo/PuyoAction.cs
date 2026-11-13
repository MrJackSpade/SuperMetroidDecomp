namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// The behaviour Puyo's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum PuyoAction
{
    /// <summary>Decrements the hop cooldown and selects an airborne hop when the countdown becomes negative.</summary>
    Grounded = 1,
    /// <summary>Dispatches the current hop or collision-recovery behaviour through the airborne behaviour.</summary>
    Airborne,
}
