namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>The counter phases on which the broken glass tube's crack and falling debris flicker (#1606).</summary>
internal static class NoobTubeCadenceDefinitions
{
    /// <summary>
    /// The phase of the enemy projectiles' frame counter on which the tube's crack is hidden: set on
    /// odd updates, so the crack shows on the even ones.
    /// </summary>
    internal static readonly UpdateCounterField CrackHidden = new(0, 1);

    /// <summary>
    /// The phase of the enemy projectiles' frame counter on which a flickering shard or bubble shows
    /// at its own column: set on odd updates. While clear a reflected shard shows mirrored across the tube and a
    /// hiding one is hidden.
    /// </summary>
    internal static readonly UpdateCounterField DebrisAtOwnColumn = new(0, 1);
}
