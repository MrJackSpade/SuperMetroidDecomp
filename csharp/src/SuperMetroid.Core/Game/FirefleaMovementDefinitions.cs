namespace SuperMetroid.Core.Game;

/// <summary>Compiled physical movement definitions for the Fireflea enemy family.</summary>
internal static class FirefleaMovementDefinitions
{
    /// <summary>
    /// Fireflea movement radii at $A3:8D1D: eight word records selected by the
    /// high byte of enemy population parameter two. These define circular and
    /// vertical travel bounds, not sprite extents.
    /// </summary>
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
