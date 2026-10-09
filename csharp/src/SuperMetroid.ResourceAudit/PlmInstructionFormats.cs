using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Bank-$84 record layouts, not implementations of gameplay. Source-contract
/// guards keep these widths/control-flow edges tied to the actual interpreter.
/// Operands are offsets from the opcode; links are also future execution roots.
/// </summary>
/// <param name="Length">Encoded instruction width in bytes.</param>
/// <param name="Words">Offsets of 16-bit operands in the instruction.</param>
/// <param name="Bytes">Offsets of 8-bit operands in the instruction.</param>
/// <param name="Targets">Offsets containing instruction-list targets.</param>
/// <param name="FallThrough">Whether control proceeds to the following instruction after this opcode.</param>
internal sealed record PlmInstructionFormat(int Length, int[] Words, int[] Bytes,
    int[] Targets, bool FallThrough = true)
{
    /// <summary>Returns the reviewed operand layout for a supported bank-$84 opcode.</summary>
    internal static PlmInstructionFormat? Get(ushort opcode) => opcode switch
    {
        RoomPlmInstructionCodes.Delete or RoomPlmInstructionCodes.Sleep =>
            new(2, [], [], [], false),
        RoomPlmInstructionCodes.Goto => new(4, [2], [], [2], false),
        RoomPlmInstructionCodes.DecrementTimerAndGoto or
        RoomPlmInstructionCodes.GotoIfSamusHasNoBombs or
        RoomPlmInstructionCodes.GotoIfDoorBitSet => new(4, [2], [], [2]),
        RoomPlmInstructionCodes.LinkInstruction => new(4, [2], [], [2]),
        RoomPlmInstructionCodes.GotoIfAreaBossBitSet or
        RoomPlmInstructionCodes.IncrementDoorHitCounterAndGoto or
        RoomPlmInstructionCodes.IncrementArgumentAndGotoIfGreaterOrEqual =>
            new(5, [3], [2], [3]),
        RoomPlmInstructionCodes.GotoIfEventSet or
        RoomPlmInstructionCodes.GotoIfRoomArgumentLess => new(6, [2, 4], [], [4]),
        RoomPlmInstructionCodes.GotoIfSamusNear => new(6, [4], [2, 3], [4]),
        RoomPlmInstructionCodes.InstallPreInstruction or
        RoomPlmInstructionCodes.SpawnDownwardGateProjectile or
        RoomPlmInstructionCodes.WakeDownwardGateProjectile or
        RoomPlmInstructionCodes.SetEvent or
        RoomPlmInstructionCodes.SpawnTorizoStatueBreaking or
        RoomPlmInstructionCodes.ShootEyeDoorProjectile or
        RoomPlmInstructionCodes.SpawnEyeDoorSweat => new(4, [2], [], []),
        RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1 or
        RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1Direct or
        RoomPlmInstructionCodes.QueueSoundLibrary2Maximum3 or
        RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6 or
        RoomPlmInstructionCodes.QueueSoundLibrary3Maximum6 or
        RoomPlmInstructionCodes.SetEightBitTimer or
        RoomPlmInstructionCodes.SetPlmBtsFromByte => new(3, [], [2], []),
        RoomPlmInstructionCodes.CopyFromRamToVram => new(9, [2, 4, 7], [6], []),
        RoomPlmInstructionCodes.SpawnFourMotherBrainGlassShards => new(10, [2, 4, 6, 8], [], []),
        RoomPlmInstructionCodes.ClearPreInstruction or
        RoomPlmInstructionCodes.SetGreyDoorPreInstruction or
        RoomPlmInstructionCodes.ClearDownwardGateTrigger or
        RoomPlmInstructionCodes.SetBotwoonScrollsBlue or
        RoomPlmInstructionCodes.MoveRightOneBlock or
        RoomPlmInstructionCodes.MoveBotwoonPlmDownOneBlock or
        RoomPlmInstructionCodes.SetPlmBtsToOne or
        RoomPlmInstructionCodes.DrawPlmBlock or
        RoomPlmInstructionCodes.DrawPlmBlockClone or
        RoomPlmInstructionCodes.QueueSongOneMusicTrack or
        RoomPlmInstructionCodes.EnableNoobTubeWaterPhysics or
        RoomPlmInstructionCodes.SpawnNoobTubeCrack or
        RoomPlmInstructionCodes.TriggerNoobTubeEarthquake or
        RoomPlmInstructionCodes.SpawnNoobTubeShardsAndBubbles or
        RoomPlmInstructionCodes.LockSamus or
        RoomPlmInstructionCodes.UnlockSamus or
        RoomPlmInstructionCodes.SpawnTwoEyeDoorSmoke or
        RoomPlmInstructionCodes.SpawnEyeDoorSmoke or
        RoomPlmInstructionCodes.MoveUpAndMakeBlueDoorFacingRight or
        RoomPlmInstructionCodes.MoveUpAndMakeBlueDoorFacingLeft or
        RoomPlmInstructionCodes.DamageDraygonCannonFacingRight or
        RoomPlmInstructionCodes.DamageDraygonCannonFacingLeft or
        SamusEaterPlmRomData.DamageInstruction or
        SamusEaterPlmRomData.ReleaseImmunityInstruction or
        EscapeAnimalPlmRomData.SetEscapedEventInstruction or
        CrateriaMainstreetEscapePassagePlmDefinitions.MoveRightFourBlocks or
        ChozoStatuePlmRomData.TransformSpikesToSlopes or
        ChozoStatuePlmRomData.RevertSlopesToSpikes or
        ChozoStatuePlmRomData.SetLoweredAcidHeight or
        TourianStatueRomData.MoveAccessDown => new(2, [], [], []),
        _ => null,
    };
}
