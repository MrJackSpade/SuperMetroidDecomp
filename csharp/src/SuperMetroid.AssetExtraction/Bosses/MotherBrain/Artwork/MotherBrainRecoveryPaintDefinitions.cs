using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Four narrowly reviewed paint choices used by room-light recovery. Their hues and
/// highlight intensity are chosen artwork; all fading and shared room shades calculate.
/// </summary>
internal static class MotherBrainRecoveryPaintDefinitions
{
    /// <summary>Recovery full-light background palette 3 slot 1: light blue-violet band on the right doorway's curved rim.</summary>
    internal static readonly RgbColor DoorwayRimLight = new(148, 173, 231);

    /// <summary>Recovery full-light background palette 3 slot 2: middle blue-violet band on the doorway rim, independently chosen hue.</summary>
    internal static readonly RgbColor DoorwayRimMiddle = new(57, 115, 231);

    /// <summary>Recovery full-light background palette 3 slot 3: dark blue-violet band on the doorway rim.</summary>
    internal static readonly RgbColor DoorwayRimDark = new(24, 24, 140);

    /// <summary>
    /// Duplicate full-light background palette5 slots1/2. The same paint is
    /// the attack bomb-casing glint (index11 of the bomb frames) and the two
    /// tube highlights. Only these exact aliases share this choice;
    /// each independently edited installed output remains independent.
    /// </summary>
    internal static readonly RgbColor SharedCasingHighlight = new(198, 198, 198);
}
