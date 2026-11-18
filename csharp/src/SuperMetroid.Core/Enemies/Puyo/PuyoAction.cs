namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// The behaviour Puyo's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum PuyoAction
{
    /// <summary>Counts the hop cooldown down and starts a hop on the update it runs out.</summary>
    Grounded,
    /// <summary>Runs the current hop or collision-recovery behaviour through the airborne behaviour.</summary>
    Airborne,
}
