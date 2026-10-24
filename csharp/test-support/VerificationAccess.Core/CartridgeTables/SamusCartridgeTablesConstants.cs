using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge values and helpers of <see cref="SamusSourceDefinitions"/> that only verification reads.</summary>
[CpuBusBoundary("catalog: Samus cartridge tables")]
internal static class SamusCartridgeTablesConstants
{
    /// <summary>$90:DD05: the movement-type weapon-admission handler pointer table.</summary>
    internal const int HudMovementHandlers = 0x90dd05;
    /// <summary>$90:DDAA: the posture-transition admission flags; zero enters the standard handler.</summary>
    internal const int HudTransitionFlags = 0x90ddaa;
}
