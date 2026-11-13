namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The two behaviours Kago's main AI dispatches.
/// Its values are arbitrary.
/// </summary>
public enum KagoEnemyFunction
{
    /// <summary>The first main-AI call only replaces itself with <see cref="NoOp"/>; shell animation and shot-triggered effects remain independently owned.</summary>
    InstallNoOp = 1,
    /// <summary>Persistent inert target after the initial handoff; the shared enemy animation animates the stationary shell and its shot callback controls damage response and bug spawning.</summary>
    NoOp,
}
