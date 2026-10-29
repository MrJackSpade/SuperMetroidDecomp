using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The shapes of Phantoon's colour targets: its health palettes, its materialization fade and the ship's power-on.</summary>
public static class PhantoonColorDefinitions
{
    /// <summary>The health-dependent body palettes, one per health band.</summary>
    public const int HealthBandCount = 8;

    /// <summary>The colours in each health palette.</summary>
    public const int HealthBandColorCount = 16;

    /// <summary>The colours of the materialization fade-out target.</summary>
    public const int FadeOutCount = 16;

    /// <summary>
    /// The background sets the Wrecked Ship's power-on fades after Phantoon's death (#1464): every set but the sixth tile
    /// set, which holds Phantoon's own body. Each fades all of its colours toward its own target colours; the order of this
    /// list is only the order the content identity hashes the targets in.
    /// </summary>
    internal static readonly BackgroundColourSet[] PowerOnSets =
    [
        BackgroundColourSet.Hud,
        BackgroundColourSet.HudItemsAndEffects,
        BackgroundColourSet.TilesFirst,
        BackgroundColourSet.TilesSecond,
        BackgroundColourSet.TilesThird,
        BackgroundColourSet.TilesFourth,
        BackgroundColourSet.TilesFifth,
    ];
}
