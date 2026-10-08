namespace SuperMetroid.Core.Game;

/// <summary>Fixed launch/cooldown mechanics selected by three independent native RNG draws.</summary>
public static class PolypLaunchDefinitions
{

    /// <summary>Uses RNG bits 1..3; bit zero and the upper bits do not select a record.</summary>
    public static ushort Cooldown(ushort random) => (ushort)(16 + ((random & 0x0e) >> 1) * 8);

    /// <summary>Uses RNG bits 1..4 to select quadratic speed indices 28 through 43.</summary>
    public static ushort InitialYIndex(ushort random) => (ushort)(28 + ((random & 0x1e) >> 1));

    /// <summary>Preserves the native sign grouping and sixteen-bit representation.</summary>
    public static ushort XVelocity(ushort random)
    {
        int index = (random & 0x1e) >> 1;
        int magnitude = 0x60 + (index & 7) * 0x10;
        return unchecked((ushort)(index < 8 ? magnitude : -magnitude));
    }
}
