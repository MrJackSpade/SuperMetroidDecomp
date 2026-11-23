using SuperMetroid.AssetExtraction.Enemies.Artwork;
namespace SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

/// <summary>Defines the installed Phantoon color resource contract.</summary>
public static partial class PhantoonColorFormat
{
    /// <summary>Canonical Phantoon color asset file name.</summary>
    public const string FileName = "phantoon-colors.json";
    /// <summary>Supported Phantoon color schema version: 2 keys each health band's and powered part's colours (#2565).</summary>
    public const int Version = 2;

    /// <summary>The colours in each health palette.</summary>
    public const int HealthBandColorCount = 16;

    /// <summary>The colours of the materialization fade-out target.</summary>
    public const int FadeOutCount = 16;

    /// <summary>The colours of each powered part's target: one colour group's sixteen.</summary>
    public const int PoweredPartColourCount = 16;

    /// <summary>
    /// The parts of the Wrecked Ship its power-on fades after Phantoon's death (#1464, #1512), each with the key the
    /// installed targets name it by. Each fades all of its colours toward its own target colours.
    /// </summary>
    internal static IReadOnlyDictionary<WreckedShipPoweredPart, string> PoweredPartKeys { get; } = new Dictionary<WreckedShipPoweredPart, string>
    {
        [WreckedShipPoweredPart.Shared] = "shared",
        [WreckedShipPoweredPart.GreenDoors] = "green-doors",
        [WreckedShipPoweredPart.MagentaDoors] = "magenta-doors",
        [WreckedShipPoweredPart.BlueDoors] = "blue-doors",
        [WreckedShipPoweredPart.Foreground] = "foreground",
        [WreckedShipPoweredPart.Background] = "background",
        [WreckedShipPoweredPart.Lights] = "lights",
    };
}
