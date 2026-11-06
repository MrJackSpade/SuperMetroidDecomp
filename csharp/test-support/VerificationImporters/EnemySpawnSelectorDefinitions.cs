using SuperMetroid.Core.Enemies.Atomic;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The authored tables a population entry's selectors index (#1519), in the cartridge's order: what each selector value
/// means, so <see cref="EnemySpawnSettingsCodec"/> decodes the entry into the values its species reads. The cartridge
/// checks compare these with its own tables.
/// </summary>
internal static class EnemySpawnSelectorDefinitions
{
    /// <summary>An Atomic's spinning animation, by its first parameter.</summary>
    public static readonly AtomicProgram[] AtomicSpins =
        [AtomicProgram.UpRight, AtomicProgram.UpLeft, AtomicProgram.DownLeft, AtomicProgram.DownRight];

    /// <summary>How far, in pixels, a Cacatac patrols to either side, by its second parameter's low byte: one block, then four through eight.</summary>
    public static readonly int[] CacatacTravelDistances = [16, 64, 80, 96, 112, 128];

    /// <summary>A Fireflea's radius in pixels, by its second parameter's high byte: eight pixels a step.</summary>
    public static readonly int[] FirefleaRadii = [8, 16, 24, 32, 40, 48, 56, 64];
}
