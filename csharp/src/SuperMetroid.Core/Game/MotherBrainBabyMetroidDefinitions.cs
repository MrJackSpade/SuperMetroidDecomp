namespace SuperMetroid.Core.Game;

/// <summary>Fixed physical spawn definitions for Mother Brain's cutscene Baby Metroid.</summary>
internal static class MotherBrainBabyMetroidDefinitions
{
    /// <summary>The cutscene Baby Metroid enemy header at <c>$A0:ECBF</c>.</summary>
    internal const ushort EnemyDefinition = 0xecbf;

    /// <summary>
    /// The single embedded population record at <c>$A9:BE28-$A9:BE37</c>. Baby init AI
    /// subsequently replaces the actor coordinates, but the complete record remains its
    /// physical spawn snapshot and supplies the native property word.
    /// </summary>
    internal static readonly RoomEnemyPopulationRecord SpawnPopulation =
        new(EnemyDefinition, 0x0180, 0x0040, 0xcfa2, 0x2800, 0, 0, 0);
}
