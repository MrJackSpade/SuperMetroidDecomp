namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The three bank-$84 bombed-reveal draw lists.</summary>
internal enum RoomPlmBombedRevealDraw : ushort
{
    /// <summary>DrawInst_1x1RespawningCrumbleBlock at $84:A49B; intact type-B parent $B0BC.</summary>
    CrumbleSingle = 0xa49b,
    /// <summary>DrawInst_PowerBombBlockBombed at $84:A4E7; intact type-C parent $C057.</summary>
    PowerBomb = 0xa4e7,
    /// <summary>DrawInst_SuperMissileBlockBombed at $84:A4ED; intact type-C parent $C09F.</summary>
    SuperMissile = 0xa4ed,
}
