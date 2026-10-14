namespace SuperMetroid.Core.Game;

/// <summary>
/// The two indirect main-AI destinations used by Kago definition <c>$E7FF</c>. Keeping the
/// ROM pointers as enum values makes a debugger watch directly comparable with variable A.
/// </summary>
public enum KagoEnemyFunction : ushort
{
    /// <summary>Function_Kago_Nothing, $A8:AB7B: the first main-AI call only replaces variable A with its $AB81 return label; shell animation and shot-triggered effects remain independently owned.</summary>
    InstallNoOp = 0xab7b,
    /// <summary>Function_Kago_Nothing.return, $A8:AB81: persistent inert RTL target after the initial handoff; the instruction interpreter animates the stationary shell and its shot callback controls damage response and bug spawning.</summary>
    NoOp = 0xab81,
}
