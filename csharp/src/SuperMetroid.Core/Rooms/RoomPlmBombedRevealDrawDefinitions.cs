namespace SuperMetroid.Core.Rooms;

/// <summary>Non-breaking reveals of gated shootable parents in bank $84.</summary>
internal static class RoomPlmBombedRevealDrawDefinitions
{
    /// <summary>DrawInst_1x1RespawningCrumbleBlock at $84:A49B; intact type-B parent $B0BC.</summary>
    internal const ushort CrumbleSingle = 0xa49b;
    /// <summary>DrawInst_PowerBombBlockBombed at $84:A4E7; intact type-C parent $C057.</summary>
    internal const ushort PowerBomb = 0xa4e7;
    /// <summary>DrawInst_SuperMissileBlockBombed at $84:A4ED; intact type-C parent $C09F.</summary>
    internal const ushort SuperMissile = 0xa4ed;

    /// <summary>Resolves a compiled bombed-reveal pointer to the single intact parent tile used for its redraw.</summary>
    /// <param name="pointer">Bank-$84 draw pointer for a supported crumble, Power Bomb, or Super Missile reveal.</param>
    /// <param name="draw">The one-tile reveal draw list when found; otherwise the default draw list.</param>
    /// <returns><see langword="true"/> when the pointer is one of the three compiled reveal layouts.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (pointer is PowerBomb or SuperMissile or CrumbleSingle)
        {
            ushort word = pointer switch { PowerBomb => 0xc057, SuperMissile => 0xc09f, _ => 0xb0bc };
            draw = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(1, new ushort[] { word }, 0, 0) });
            return true;
        }
        draw = default;
        return false;
    }
}
