namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The horizontal radius a power bomb's sweep continues with after the sweep kills an enemy (#1472): zero after an
/// ordinary kill, so the rest of that update's sweep reaches no further enemy horizontally, and 16384 pixels after
/// killing an enemy that respawns, so the rest of the sweep reaches every later enemy horizontally. The vertical
/// radius is unchanged.
/// </summary>
/// <remarks>
/// Consulted by the power-bomb sweep before it tests each enemy, after an earlier enemy in the same sweep died.
/// Evidence: the death routines (<c>EnemyDeath</c>, <c>RinkaDeath</c>) leave the dying enemy's respawn property in the
/// working word the sweep at <c>$A0:A306</c> keeps its horizontal radius in, and the sweep never reloads it.
/// </remarks>
[CompatibilityService("a kill inside a power bomb's sweep shrinks its horizontal reach to nothing, or widens it to the whole room after a respawning enemy")]
public static class PowerBombRadiusAfterKill
{
    /// <summary>The horizontal radius after killing an enemy that respawns.</summary>
    private const int AfterRespawningKill = 1 << 14;

    /// <summary>The horizontal radius the sweep continues with.</summary>
    /// <param name="killedEnemyRespawns">Whether the enemy the sweep killed respawns.</param>
    public static int Of(bool killedEnemyRespawns) => killedEnemyRespawns ? AfterRespawningKill : 0;
}
