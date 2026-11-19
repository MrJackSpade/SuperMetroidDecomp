namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// The behaviour Puyo's main AI runs: grounded, or airborne on the rising or the falling half of its speed curve (#2512).
/// Its values are arbitrary.
/// </summary>
public enum PuyoAction
{
    /// <summary>Counts the hop cooldown down and starts a hop on the update it runs out.</summary>
    Grounded,
    /// <summary>
    /// Runs the current hop or collision-recovery behaviour through the airborne behaviour, rising: the curve's upward
    /// speeds move it, and a ceiling it strikes starts its drop. Each hop starts rising.
    /// </summary>
    Rising,
    /// <summary>
    /// Runs the current hop or collision-recovery behaviour through the airborne behaviour, falling from the hop's apex:
    /// the curve's downward speeds move it, and the ground it meets lands it.
    /// </summary>
    Falling,
}
