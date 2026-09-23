namespace SuperMetroid.Core.Game;

/// <summary>Compiled physical movement definitions for the Fireflea enemy family.</summary>
internal static class FirefleaMovementDefinitions
{
    /// <summary>
    /// Fireflea movement radii at $A3:8D1D: eight word records selected by the
    /// high byte of enemy population parameter two. These define circular and
    /// vertical travel bounds, not sprite extents.
    /// </summary>
    /// <remarks>Issues #625 and #942 exact formula: 8*(i+1) for i=0..7. LookupTableResearch verifies all eight
    /// words against NTSC J/U v1.0 ROM, pinned assembly, and this table. Preserve the index bound.</remarks>
    private static ReadOnlySpan<ushort> Radii =>
    [
        8, 16, 24, 32, 40, 48, 56, 64,
    ];

    internal static ushort Radius(byte index)
    {
        if (index >= Radii.Length)
        {
            throw new InvalidDataException(
                $"Fireflea radius index {index} is outside the {Radii.Length}-entry native table.");
        }

        return Radii[index];
    }
}
