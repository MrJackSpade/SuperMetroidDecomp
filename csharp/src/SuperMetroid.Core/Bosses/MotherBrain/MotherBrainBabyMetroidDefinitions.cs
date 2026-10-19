using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawn and music identities for Mother Brain's cutscene Baby Metroid.</summary>
internal static class MotherBrainBabyMetroidDefinitions
{
    /// <summary>$A9:CC65, Function_BabyMetroidCutscene_PlaySamusTheme: data command $FF48 loads the Theme of Samus.</summary>
    internal const byte SamusThemeMusicData = 0x48;

    /// <summary>$A9:CC6C, Function_BabyMetroidCutscene_PlaySamusTheme: track $0005 follows the theme data upload.</summary>
    internal const byte SamusThemeMusicTrack = 5;

    /// <summary>$AD:F3D3: the first of the room-lights palettes the Baby restores; each earlier one is 56 bytes before.</summary>
    internal const uint RoomLightsPaletteSource = 0xadf3d3;

    /// <summary>Bytes in each room-lights palette: fourteen colors for each of two destinations.</summary>
    internal const uint RoomLightsPaletteBytes = 56;

    /// <summary>
    /// The single embedded population record at <c>$A9:BE28-$A9:BE37</c>. Baby init AI
    /// subsequently replaces the actor coordinates, but the complete record remains its
    /// physical spawn snapshot and supplies the native property word.
    /// </summary>
    internal static readonly RoomEnemyPopulationRecord SpawnPopulation =
        new(EnemyDefinitionId.BabyMetroidCutscene, new(0x0180, 0x0040), 0xcfa2,
            new EnemyPropertyWord(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen), new EnemyExtraPropertyWord(EnemyExtraProperties.None), 0, 0);
}
