namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The two routines Kago's main AI dispatches.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum KagoEnemyFunction
{
    /// <summary>Function_Kago_Nothing, $A8:AB7B: the first main-AI call only replaces variable A with its $AB81 return label; shell animation and shot-triggered effects remain independently owned.</summary>
    InstallNoOp = 1,
    /// <summary>Function_Kago_Nothing.return, $A8:AB81: persistent inert RTL target after the initial handoff; the instruction interpreter animates the stationary shell and its shot callback controls damage response and bug spawning.</summary>
    NoOp,
}
