namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The horizontal radius a power bomb's sweep continues with after the sweep kills an enemy (#1472, #1476): zero after an
/// ordinary kill, so the rest of that update's sweep reaches no further enemy horizontally, and 16384 pixels after
/// killing an enemy that respawns, so the rest of the sweep reaches every later enemy horizontally. The vertical radius
/// is unchanged. A kill outside a sweep changes nothing: each sweep starts by forgetting earlier kills.
/// </summary>
/// <remarks>
/// Told of every enemy death as the death starts (<see cref="RecordKill"/>), and consulted by the power-bomb sweep before
/// it tests each enemy (<see cref="HorizontalRadius"/>), which applies a kill made by the previous enemy's reaction.
/// Evidence: the death routines (<c>EnemyDeath</c>, <c>RinkaDeath</c>) leave the dying enemy's respawn property in the
/// working value the sweep at <c>$A0:A306</c> keeps its horizontal radius in, and the sweep never reloads it.
/// </remarks>
[CompatibilityService("a kill inside a power bomb's sweep shrinks its horizontal reach to nothing, or widens it to the whole room after a respawning enemy")]
public sealed class PowerBombRadiusAfterKill
{
    /// <summary>The horizontal radius after killing an enemy that respawns.</summary>
    private const int AfterRespawningKill = 1 << 14;

    /// <summary>The radius the latest kill left the sweep with, or null when no kill has happened since it was last applied.</summary>
    private int? _radiusLeftByKill;

    /// <summary>Records a kill, which a sweep in progress continues from.</summary>
    /// <param name="killedEnemyRespawns">Whether the enemy killed respawns.</param>
    public void RecordKill(bool killedEnemyRespawns) => _radiusLeftByKill = killedEnemyRespawns ? AfterRespawningKill : 0;

    /// <summary>Forgets any kill made outside the sweep that is starting or ending.</summary>
    public void Forget() => _radiusLeftByKill = null;

    /// <summary>
    /// The horizontal radius the sweep tests its next enemy with: the radius the latest kill left, or the sweep's own when
    /// no enemy has died since the last test. The kill is applied once.
    /// </summary>
    /// <param name="sweepRadius">The horizontal radius the sweep has been testing with.</param>
    public int HorizontalRadius(int sweepRadius)
    {
        int radius = _radiusLeftByKill ?? sweepRadius;
        _radiusLeftByKill = null;
        return radius;
    }
}
