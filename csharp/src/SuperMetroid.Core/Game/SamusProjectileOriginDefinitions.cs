namespace SuperMetroid.Core.Game;

/// <summary>Physical beam/missile muzzle positions, independent of charge-flare artwork.</summary>
internal static class SamusProjectileOriginDefinitions
{
    /// <summary>$90:C204..C253 ProjectileOriginOffsetsByDirection: physical muzzle coordinates for each named firing direction and running/Moonwalk mode.</summary>
    private static (short X, short Y) Muzzle(bool running, SamusProjectileDirection direction) => (running, direction) switch
    {
        (false, SamusProjectileDirection.UpFacingRight) => (2, -8),
        (false, SamusProjectileDirection.UpRight) => (13, -13),
        (false, SamusProjectileDirection.Right) => (11, 1),
        (false, SamusProjectileDirection.DownRight) => (13, 4),
        (false, SamusProjectileDirection.DownFacingRight) => (2, 13),
        (false, SamusProjectileDirection.DownFacingLeft) => (-5, 13),
        (false, SamusProjectileDirection.DownLeft) => (-14, 4),
        (false, SamusProjectileDirection.Left) => (-11, 1),
        (false, SamusProjectileDirection.UpLeft) => (-19, -19),
        (false, SamusProjectileDirection.UpFacingLeft) => (-2, -8),
        (true, SamusProjectileDirection.UpFacingRight) => (2, -8),
        (true, SamusProjectileDirection.UpRight) => (15, -16),
        (true, SamusProjectileDirection.Right) => (15, -2),
        (true, SamusProjectileDirection.DownRight) => (13, 1),
        (true, SamusProjectileDirection.DownFacingRight) => (2, 13),
        (true, SamusProjectileDirection.DownFacingLeft) => (-5, 13),
        (true, SamusProjectileDirection.DownLeft) => (-13, 1),
        (true, SamusProjectileDirection.Left) => (-13, -2),
        (true, SamusProjectileDirection.UpLeft) => (-15, -16),
        (true, SamusProjectileDirection.UpFacingLeft) => (-2, -8),
        _ => throw new InvalidOperationException("Muzzle source owner must contain a named direction."),
    };


    internal static (short X, short Y) Read(bool running, ushort direction)
    {
        int offset = (direction & 0x0f) * sizeof(ushort);
        int x = running ? SamusProjectileRomData.Origins.RunningX : SamusProjectileRomData.Origins.DefaultX;
        int y = running ? SamusProjectileRomData.Origins.RunningY : SamusProjectileRomData.Origins.DefaultY;
        return (ReadWord(x + offset), ReadWord(y + offset));
    }

    private static short ReadWord(int address)
    {
        int offset = address - SamusProjectileRomData.Origins.DefaultX;
        if (offset >= 0 && offset < 40 * sizeof(ushort) && (offset & 1) == 0)
        {
            int word = offset / sizeof(ushort);
            var muzzle = Muzzle(word >= 20, SamusProjectileDirections.At(word % 10));
            return word % 20 < 10 ? muzzle.X : muzzle.Y;
        }
        // Resolve addresses before table ownership: low-nibble directions ten through
        // fifteen cross into adjacent rows, and running Y eventually reaches cooldowns.
        return unchecked((short)(SamusProjectileCooldownDefinitions.ReadByte(address) |
            (SamusProjectileCooldownDefinitions.ReadByte(address + 1) << 8)));
    }
}
