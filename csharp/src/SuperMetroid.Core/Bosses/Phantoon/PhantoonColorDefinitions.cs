using SuperMetroid.Core.Enemies.Artwork;
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
    /// The parts of the Wrecked Ship its power-on fades after Phantoon's death (#1464, #1512), in the order the
    /// installed targets list them. Each fades all of its colours toward its own target colours.
    /// </summary>
    internal static readonly WreckedShipPoweredPart[] PoweredParts =
    [
        WreckedShipPoweredPart.Shared,
        WreckedShipPoweredPart.GreenDoors,
        WreckedShipPoweredPart.MagentaDoors,
        WreckedShipPoweredPart.BlueDoors,
        WreckedShipPoweredPart.Foreground,
        WreckedShipPoweredPart.Background,
        WreckedShipPoweredPart.Lights,
    ];
}
