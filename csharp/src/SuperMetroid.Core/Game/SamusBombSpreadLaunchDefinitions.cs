namespace SuperMetroid.Core.Game;

/// <summary>Fixed Bomb Spread mechanics, not editable explosion artwork or animation.</summary>
internal static class SamusBombSpreadLaunchDefinitions
{
    /// <summary>
    /// $90:D8CF..D8F6 BombSpreadData: five symmetric launches. Fuse length increases
    /// ten frames per step from center; outward X speed grows by half a pixel/frame.
    /// Y rises two-and-a-half at center, one beside it, and zero at the edges.
    /// </summary>
    internal static BombSpreadLaunchDefinition ForSlot(int index)
    {
        if ((uint)index >= 5) throw new IndexOutOfRangeException();
        int radius = Math.Abs(index - 2);
        return new((ushort)(100 + radius * 10),
            (ushort)((index < 2 ? 0x8000 : 0) | radius * 128),
            (ushort)(2 - radius), (ushort)(radius == 0 ? 0x8000 : 0));
    }
}

/// <summary>One physical bomb's fuse and initial velocity; X retains the native direction bit rather than signed-short interpretation.</summary>
internal readonly record struct BombSpreadLaunchDefinition(
    ushort FuseTimer, ushort XVelocity, ushort YSpeed, ushort YSubspeed);
