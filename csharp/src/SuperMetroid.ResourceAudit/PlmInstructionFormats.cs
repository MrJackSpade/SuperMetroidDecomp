using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Bank-$84 record layouts, not implementations of gameplay. Source-contract
/// guards keep these widths/control-flow edges tied to the actual interpreter.
/// Operands are offsets from the opcode; links are also future execution roots.
/// </summary>
internal sealed record PlmInstructionFormat(int Length, int[] Words, int[] Bytes,
    int[] Targets, bool FallThrough = true)
{
    /// <summary>Decodes a stream word; a word outside <see cref="RoomPlmInstruction"/> has no format.</summary>
    internal static PlmInstructionFormat? Get(ushort opcode) =>
        Enum.IsDefined((RoomPlmInstruction)opcode) ? Get((RoomPlmInstruction)opcode) : null;

    private static PlmInstructionFormat? Get(RoomPlmInstruction opcode) => opcode switch
    {
        RoomPlmInstruction.Delete or RoomPlmInstruction.Sleep =>
            new(2, [], [], [], false),
        RoomPlmInstruction.Goto => new(4, [2], [], [2], false),
        RoomPlmInstruction.DecrementTimerAndGoto or
        RoomPlmInstruction.GotoIfSamusHasNoBombs or
        RoomPlmInstruction.GotoIfDoorBitSet => new(4, [2], [], [2]),
        RoomPlmInstruction.LinkInstruction => new(4, [2], [], [2]),
        RoomPlmInstruction.GotoIfAreaBossBitSet or
        RoomPlmInstruction.IncrementDoorHitCounterAndGoto or
        RoomPlmInstruction.IncrementArgumentAndGotoIfGreaterOrEqual =>
            new(5, [3], [2], [3]),
        RoomPlmInstruction.GotoIfEventSet or
        RoomPlmInstruction.GotoIfRoomArgumentLess => new(6, [2, 4], [], [4]),
        RoomPlmInstruction.GotoIfSamusNear => new(6, [4], [2, 3], [4]),
        RoomPlmInstruction.InstallPreInstruction or
        RoomPlmInstruction.SpawnDownwardGateProjectile or
        RoomPlmInstruction.WakeDownwardGateProjectile or
        RoomPlmInstruction.SetEvent or
        RoomPlmInstruction.SpawnTorizoStatueBreaking or
        RoomPlmInstruction.ShootEyeDoorProjectile or
        RoomPlmInstruction.SpawnEyeDoorSweat => new(4, [2], [], []),
        RoomPlmInstruction.QueueSoundLibrary2Maximum1 or
        RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct or
        RoomPlmInstruction.QueueSoundLibrary2Maximum3 or
        RoomPlmInstruction.QueueSoundLibrary2Maximum6 or
        RoomPlmInstruction.QueueSoundLibrary3Maximum6 or
        RoomPlmInstruction.SetEightBitTimer or
        RoomPlmInstruction.SetPlmBtsFromByte => new(3, [], [2], []),
        RoomPlmInstruction.CopyFromRamToVram => new(9, [2, 4, 7], [6], []),
        RoomPlmInstruction.SpawnFourMotherBrainGlassShards => new(10, [2, 4, 6, 8], [], []),
        RoomPlmInstruction.ClearPreInstruction or
        RoomPlmInstruction.SetGreyDoorPreInstruction or
        RoomPlmInstruction.ClearDownwardGateTrigger or
        RoomPlmInstruction.SetBotwoonScrollsBlue or
        RoomPlmInstruction.MoveRightOneBlock or
        RoomPlmInstruction.MoveBotwoonPlmDownOneBlock or
        RoomPlmInstruction.SetPlmBtsToOne or
        RoomPlmInstruction.DrawPlmBlock or
        RoomPlmInstruction.DrawPlmBlockClone or
        RoomPlmInstruction.QueueSongOneMusicTrack or
        RoomPlmInstruction.EnableNoobTubeWaterPhysics or
        RoomPlmInstruction.SpawnNoobTubeCrack or
        RoomPlmInstruction.TriggerNoobTubeEarthquake or
        RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles or
        RoomPlmInstruction.LockSamus or
        RoomPlmInstruction.UnlockSamus or
        RoomPlmInstruction.SpawnTwoEyeDoorSmoke or
        RoomPlmInstruction.SpawnEyeDoorSmoke or
        RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight or
        RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft or
        RoomPlmInstruction.DamageDraygonCannonFacingRight or
        RoomPlmInstruction.DamageDraygonCannonFacingLeft or
        RoomPlmInstruction.SamusEaterDamage or
        RoomPlmInstruction.SamusEaterReleaseImmunity or
        RoomPlmInstruction.SetAnimalsEscapedEvent or
        RoomPlmInstruction.MoveRightFourBlocks or
        RoomPlmInstruction.TransformSpikesToSlopes or
        RoomPlmInstruction.RevertSlopesToSpikes or
        RoomPlmInstruction.SetLoweredAcidHeight or
        RoomPlmInstruction.MoveTourianAccessDown => new(2, [], [], []),
        _ => null,
    };
}
