namespace SuperMetroid.Core.Rooms;

/// <summary>Native bank-$8F callbacks accepted by a room header's inline state program.</summary>
/// <remarks>
/// These values are executable routine offsets consumed by <c>CallRoomDefStateSelect</c>,
/// not state IDs. The two unused retail routines are named so diagnostics can distinguish
/// known-but-untranslated cartridge code from a corrupt or genuinely unknown pointer.
/// </remarks>
public static class RoomStateSelectorCodes
{
    /// <summary><c>$8F:E5E6 Use_StatePointer_inX</c>: select the inline default state.</summary>
    public const ushort Finish = 0xe5e6;

    /// <summary><c>$8F:E5EB UNUSED_RoomStateCheck_Door_8FE5EB</c>.</summary>
    public const ushort UnusedDoor = 0xe5eb;

    /// <summary><c>$8F:E5FF RoomStateCheck_MainAreaBossIsDead</c>.</summary>
    public const ushort MainAreaBossIsDead = 0xe5ff;

    /// <summary><c>$8F:E612 RoomStateCheck_EventHasBeenSet</c>.</summary>
    public const ushort EventHasBeenSet = 0xe612;

    /// <summary><c>$8F:E629 RoomStateCheck_BossIsDead</c>.</summary>
    public const ushort BossIsDead = 0xe629;

    /// <summary><c>$8F:E640 UNUSED_RoomStateCheck_Morphball_8FE640</c>.</summary>
    public const ushort UnusedMorphBall = 0xe640;

    /// <summary><c>$8F:E652 RoomStateCheck_MorphballAndMissiles</c>.</summary>
    public const ushort MorphBallAndMissiles = 0xe652;

    /// <summary><c>$8F:E669 RoomStateCheck_PowerBombs</c>.</summary>
    public const ushort PowerBombs = 0xe669;
}

/// <summary>Fixed operands embedded by selector routines rather than by room data.</summary>
public static class RoomStateSelectorOperands
{
    /// <summary>
    /// <c>RoomStateCheck_MainAreaBossIsDead</c> always tests bit zero of the area's boss byte.
    /// </summary>
    public const byte MainAreaBossMask = 0x01;
}
