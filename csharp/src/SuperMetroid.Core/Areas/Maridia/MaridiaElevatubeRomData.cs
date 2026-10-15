namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>Immediate values and entry point owned by Maridia's elevatube cartridge code.</summary>
internal static class MaridiaElevatubeRomData
{
    /// <summary>Whole X coordinate written on every room-main call.</summary>
    public const ushort SamusCenterX = 0x0080;

    /// <summary>South-entry RoomMainASMVar2.</summary>
    public const ushort SouthStartingPosition = 0x09c0;

    /// <summary>South-entry RoomMainASMVar3, or -1.0 pixels/frame in signed 8.8.</summary>
    public const ushort SouthStartingVelocity = 0xff00;

    /// <summary>South-entry RoomMainASMVar4, or -0.125 pixels/frame² in signed 8.8.</summary>
    public const ushort SouthAcceleration = 0xffe0;

    /// <summary>North-entry RoomMainASMVar2.</summary>
    public const ushort NorthStartingPosition = 0x0040;

    /// <summary>North-entry RoomMainASMVar3, or +1.0 pixels/frame in signed 8.8.</summary>
    public const ushort NorthStartingVelocity = 0x0100;

    /// <summary>North-entry RoomMainASMVar4, or +0.125 pixels/frame² in signed 8.8.</summary>
    public const ushort NorthAcceleration = 0x0020;

    /// <summary>Bias and absolute velocity limit used by $8F:E2F1.</summary>
    public const ushort MaximumSpeed = 0x0e20;

    /// <summary>Exclusive biased comparison endpoint used by $8F:E2F4.</summary>
    public const ushort SpeedRangeExclusiveEnd = 0x1c41;
}
