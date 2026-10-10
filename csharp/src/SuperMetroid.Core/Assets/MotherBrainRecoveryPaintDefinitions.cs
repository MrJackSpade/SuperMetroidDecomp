using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Four narrowly reviewed paint choices used by room-light recovery. Their hues and
/// highlight intensity are chosen artwork; all fading and shared room shades calculate.
/// </summary>
internal static class MotherBrainRecoveryPaintDefinitions
{
    /// <summary>$AD:F283, recovery full-light BG palette3 slot1: light blue-violet band on the right doorway's curved rim.</summary>
    internal static readonly Bgr555 DoorwayRimLight = Bgr555.FromWord(0x72b2);

    /// <summary>$AD:F285, recovery full-light BG palette3 slot2: middle blue-violet band on the doorway rim, independently chosen hue.</summary>
    internal static readonly Bgr555 DoorwayRimMiddle = Bgr555.FromWord(0x71c7);

    /// <summary>$AD:F287, recovery full-light BG palette3 slot3: dark blue-violet band on the doorway rim.</summary>
    internal static readonly Bgr555 DoorwayRimDark = Bgr555.FromWord(0x4463);

    /// <summary>
    /// $AD:F29F/F2A1, duplicate full-light BG palette5 slots1/2. The same paint is
    /// the attack bomb-casing glint at $A9:94C8 (map $8D:830C, index11) and the two
    /// tube highlights at $A9:94F4/F6. Only these exact aliases share this choice;
    /// each independently edited installed output remains independent.
    /// </summary>
    internal static readonly Bgr555 SharedCasingHighlight = Bgr555.FromWord(0x6318);
}
