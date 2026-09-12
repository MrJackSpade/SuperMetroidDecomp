namespace SuperMetroid.Core.Game;

/// <summary>Fixed launch/cooldown mechanics selected by three independent native RNG draws.</summary>
public static class PolypLaunchDefinitions
{
    /// <summary>$A2:B520, PolypData.cooldownTimer: eight durations, 16 through 72 frames.</summary>
    public const int CooldownReferenceAddress = 0xa2b520;
    /// <summary>$A2:B530, PolypData.projectileInitialYSpeedTableIndex: sixteen quadratic indices.</summary>
    public const int YIndexReferenceAddress = 0xa2b530;
    /// <summary>$A2:B550, PolypData.projectileXVelocity: eight positive then eight negative 8.8 speeds.</summary>
    public const int XVelocityReferenceAddress = 0xa2b550;

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
