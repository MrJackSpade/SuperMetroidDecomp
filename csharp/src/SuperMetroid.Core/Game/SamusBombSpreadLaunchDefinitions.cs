namespace SuperMetroid.Core.Game;

/// <summary>Fixed Bomb Spread mechanics, not editable explosion artwork or animation.</summary>
internal static class SamusBombSpreadLaunchDefinitions
{
    /// <summary>$90:D8CF..D8F6 BombSpreadData: five fuse, X direction/magnitude, whole-Y and fractional-Y records consumed by $90:D849.</summary>
    private static readonly BombSpreadLaunchDefinition[] Launches =
    [
        new(120, 0x8100, 0, 0),
        new(110, 0x8080, 1, 0),
        new(100, 0x0000, 2, 0x8000),
        new(110, 0x0080, 1, 0),
        new(120, 0x0100, 0, 0),
    ];

    internal static BombSpreadLaunchDefinition ForSlot(int index) => Launches[index];
}

/// <summary>One physical bomb's fuse and initial velocity; X retains the native direction bit rather than signed-short interpretation.</summary>
internal readonly record struct BombSpreadLaunchDefinition(
    ushort FuseTimer, ushort XVelocity, ushort YSpeed, ushort YSubspeed);
