namespace SuperMetroid.Core.Game;

/// <summary>Cartridge wall-impact request made by Ridley_Func_113 at $A6:D914.</summary>
internal static class RidleyWallImpactDefinitions
{
    /// <summary>$A6:D914 compares the larger absolute 8.8 velocity against $0280 (2.5 pixels/frame).</summary>
    public const int MinimumSpeed = 0x0280;
    /// <summary>$A6:D914's non-Norfair branch requests room-shake type 33.</summary>
    public const ushort EarthquakeType = 33;
    /// <summary>$A6:D914 initializes the quake to twelve frames.</summary>
    public const ushort Duration = 12;
}
