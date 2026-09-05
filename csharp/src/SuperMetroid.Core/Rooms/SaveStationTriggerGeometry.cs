namespace SuperMetroid.Core.Rooms;

/// <summary>Coordinate operands used by PlmSetup_B76B_SaveStationTrigger ($84:B590).</summary>
internal static class SaveStationTriggerGeometry
{
    /// <summary>The native setup subtracts eight pixels from Samus X before comparing block columns.</summary>
    public const ushort HorizontalProbeOffset = 8;

    /// <summary>Four logical right shifts convert the wrapped 16-bit probe coordinate to a 16-pixel block column.</summary>
    public const int BlockCoordinateShift = 4;
}
