using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

/// <summary>Defines the installed Phantoon color resource contract.</summary>
public static class PhantoonColorFormat
{
    /// <summary>Canonical Phantoon color asset file name.</summary>
    public const string FileName = "phantoon-colors.json";
    /// <summary>Supported Phantoon color schema version.</summary>
    public const int Version = 1;

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
