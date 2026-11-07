using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Compiled parallel records used by the shared Bomb/Golden Torizo initializer.</summary>
internal static class TorizoInitializationDefinitions
{
    /// <summary>
    /// The Torizo initializer's parallel X/Y, instruction, property and radius tables.
    /// First entries select Bomb Torizo, including its first initial body program.
    /// The property word is ORed into the population's existing properties.
    /// </summary>
    internal static TorizoInitializationDefinition Bomb => new(new(219, 179), TorizoBodyProgram.BombTorizoInitial0, EnemyProperties.RunsAnimationScript | EnemyProperties.ProcessOffScreen, new(18, 48));

    /// <summary>
    /// The second initialization entries select Golden Torizo, including
    /// its first initial body program. Its shorter Y radius is authored data.
    /// </summary>
    internal static TorizoInitializationDefinition Golden => new(new(424, 144), TorizoBodyProgram.GoldenInitial0, EnemyProperties.RunsAnimationScript | EnemyProperties.ProcessOffScreen, new(18, 41));
}
