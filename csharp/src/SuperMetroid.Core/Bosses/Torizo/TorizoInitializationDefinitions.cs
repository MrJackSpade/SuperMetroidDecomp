using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Compiled parallel records used by the shared Bomb/Golden Torizo initializer.</summary>
internal static class TorizoInitializationDefinitions
{
    /// <summary>
    /// $AA:C95F..C976, Torizo_Init parallel X/Y, instruction, property and radius tables.
    /// First entries select Bomb Torizo, including InstList_Torizo_BombTorizo_Initial_0
    /// ($AA:B879). The property word is ORed into the population's existing properties.
    /// </summary>
    internal static TorizoInitializationDefinition Bomb => new(new(219, 179), TorizoBodyProgram.BombTorizoInitial0, EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen, new(18, 48));

    /// <summary>
    /// Second entries of $AA:C95F..C976 select Golden Torizo, including
    /// InstList_GoldenTorizo_Initial_0 ($AA:C9CB). Its shorter Y radius is authored data.
    /// </summary>
    internal static TorizoInitializationDefinition Golden => new(new(424, 144), TorizoBodyProgram.GoldenInitial0, EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen, new(18, 41));
}
