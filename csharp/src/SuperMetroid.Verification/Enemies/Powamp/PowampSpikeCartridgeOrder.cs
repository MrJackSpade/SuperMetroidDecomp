namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>The order the cartridge's spike tables list the eight directions in, the oracle for the typed spike data.</summary>
internal static class PowampSpikeCartridgeOrder
{
    /// <summary>
    /// The directions in the cartridge's table order, clockwise from up; the cartridge's burst fires them from the last of
    /// these back to the first.
    /// </summary>
    internal static CompassDirection[] Directions =>
    [
        CompassDirection.Up, CompassDirection.UpRight, CompassDirection.Right, CompassDirection.DownRight,
        CompassDirection.Down, CompassDirection.DownLeft, CompassDirection.Left, CompassDirection.UpLeft,
    ];
}
