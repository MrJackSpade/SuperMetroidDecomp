namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The shared long-return routines at $804C that enemy headers name for "no AI". An initialization or main-AI pointer
/// naming one of these does nothing; any other pointer is a species' routine.
/// </summary>
internal enum EnemyCommonNoOpRoutine
{
    /// <summary>No shared no-op: a species' routine or an untranslated one.</summary>
    None = 0,

    /// <summary><c>RTL_A2804C</c> at $A2:804C, named as an initialization or main-AI pointer.</summary>
    BankA2 = 0xa2804c,

    /// <summary><c>RTL_A3804C</c> at $A3:804C, the respawn placeholder's initialization and main AI.</summary>
    BankA3 = 0xa3804c,

    /// <summary><c>RTL_AA804C</c> at $AA:804C, the Tourian statue ghost's initialization and main AI.</summary>
    BankAA = 0xaa804c,
}
