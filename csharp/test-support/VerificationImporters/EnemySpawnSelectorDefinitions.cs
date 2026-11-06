using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Choot;

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

    /// <summary>
    /// A Boyon's bounce speed multiplier, by its first parameter's low byte: the values double through the fourth, then
    /// rise by 2, 3, 3 and 4.
    /// </summary>
    public static readonly int[] BoyonSpeedMultipliers = [1, 2, 4, 8, 10, 13, 16, 20];

    /// <summary>
    /// A Boyon's jump height in 256ths of a pixel, by its first parameter's high byte: 48 pixels, rising 16 pixels a
    /// step.
    /// </summary>
    public static readonly int[] BoyonJumpHeights = [12288, 16384, 20480, 24576, 28672, 32768, 36864, 40960, 45056];

    /// <summary>
    /// A Bull's acceleration and deceleration intervals, in updates, by its first parameter: acceleration is three
    /// more than the selector, deceleration advances every two selectors, holding two updates once more at selector four.
    /// </summary>
    public static readonly BullSpeedIntervals[] BullIntervals =
    [
        new(3, 1), new(4, 1), new(5, 2), new(6, 2), new(7, 2), new(8, 3), new(9, 3),
        new(10, 4), new(11, 4), new(12, 5), new(13, 5), new(14, 6), new(15, 6),
    ];

    /// <summary>A Bull's maximum speed in 256ths of a pixel per update, by its second parameter: one pixel a step from just under four.</summary>
    public static readonly int[] BullMaximumSpeeds = [1023, 1279, 1535, 1791, 2047, 2303, 2559, 2815];

    /// <summary>How far, in pixels, a Cacatac patrols to either side, by its second parameter's low byte: one block, then four through eight.</summary>
    public static readonly int[] CacatacTravelDistances = [16, 64, 80, 96, 112, 128];

    /// <summary>A Choot's falling path, by its first parameter's high byte.</summary>
    public static readonly ChootFallingPath[] ChootFallingPaths =
        [ChootFallingPath.Normal, ChootFallingPath.Wide, ChootFallingPath.VeryWide, ChootFallingPath.Slow, ChootFallingPath.VerySlow];

    /// <summary>A Fireflea's radius in pixels, by its second parameter's high byte: eight pixels a step.</summary>
    public static readonly int[] FirefleaRadii = [8, 16, 24, 32, 40, 48, 56, 64];
}
