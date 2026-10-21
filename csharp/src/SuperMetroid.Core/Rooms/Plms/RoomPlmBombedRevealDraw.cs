namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The three bombed-reveal draw lists.</summary>
internal enum RoomPlmBombedRevealDraw
{
    /// <summary>DrawInst_1x1RespawningCrumbleBlock; intact type-B parent $B0BC.</summary>
    CrumbleSingle,
    /// <summary>DrawInst_PowerBombBlockBombed; intact type-C parent $C057.</summary>
    PowerBomb,
    /// <summary>DrawInst_SuperMissileBlockBombed; intact type-C parent $C09F.</summary>
    SuperMissile,
}
