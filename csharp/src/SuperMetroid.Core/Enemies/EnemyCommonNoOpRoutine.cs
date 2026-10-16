namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The shared long-return routines at $804C that enemy headers name for "no AI". Only these banked addresses appear as
/// initialization or main-AI pointers without a species owning them; any other pointer is a species' routine.
/// </summary>
internal enum EnemyCommonNoOpRoutine
{
    /// <summary>No shared no-op: a species' routine or an untranslated one.</summary>
    None = 0,

    /// <summary><c>RTL_A2804C</c> at $A2:804C, named as an initialization or main-AI pointer.</summary>
    BankA2 = 0xa2804c,

    /// <summary><c>RTL_A3804C</c> at $A3:804C, the respawn placeholder's main AI.</summary>
    BankA3 = 0xa3804c,
}
