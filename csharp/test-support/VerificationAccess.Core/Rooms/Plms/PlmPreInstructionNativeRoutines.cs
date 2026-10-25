using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The bank-$84 routine each shared pre-instruction ports, for comparing slots with native PLM RAM.</summary>
internal static class PlmPreInstructionNativeRoutines
{
    /// <summary>The first of the Metroids-cleared table's nine one-byte no-op handlers (<c>$84:DAD5</c>).</summary>
    private const ushort MetroidsClearedFirstNoOp = 0xdad5;

    /// <summary>The first of its four sixteen-byte quota observers (<c>$84:DADE</c>).</summary>
    private const ushort MetroidsClearedFirstQuotaObserver = 0xdade;

    /// <summary>Every shared pre-instruction with its routine.</summary>
    private static readonly Dictionary<IPlmPreInstruction, ushort> Routines = Build();

    /// <summary>The routine <paramref name="preInstruction"/> ports.</summary>
    /// <exception cref="InvalidOperationException">The instance is not a shared pre-instruction.</exception>
    internal static ushort Of(IPlmPreInstruction preInstruction) =>
        Routines.TryGetValue(preInstruction, out ushort routine)
            ? routine
            : throw new InvalidOperationException($"PLM pre-instruction {preInstruction} has no native routine mapping.");

    /// <summary>The routine a Metroids-cleared PLM installs for <paramref name="roomArgument"/>.</summary>
    internal static ushort MetroidsCleared(ushort roomArgument)
    {
        MetroidsClearedPlmRomData.ResolveEvent(roomArgument);
        int entry = roomArgument / 2;
        return (ushort)(entry < 9 ? MetroidsClearedFirstNoOp + entry : MetroidsClearedFirstQuotaObserver + (entry - 9) * 16);
    }

    /// <summary>Maps every shared instance, rejecting an instance listed twice.</summary>
    private static Dictionary<IPlmPreInstruction, ushort> Build()
    {
        var routines = new Dictionary<IPlmPreInstruction, ushort>
        {
            [PlmPreInstructions.Inert] = PlmPreInstructionDefinitions.SpawnDefault,
            [TourianPreInstructions.OldTourianEscapeShaftWallWaitForSamus] = OldTourianEscapeShaftWallPlmDefinitionsConstants.WaitForSamusPreInstruction,
            [DraygonCannonPreInstructions.MissileHit] = DraygonCannonRomDataConstants.MissileHitPreInstruction,
            [MotherBrainGlassPreInstructions.Hit] = MotherBrainGlassPlmProgramDefinitionsConstants.HitPreInstruction,
            [BombTorizoHandPreInstructions.WakeIfSamusHasBombs] = BombTorizoHandPlmProgramDefinitionsConstants.WakeIfSamusHasBombsPreInstruction,
            [ChozoStatuePreInstructions.WaitForLowerNorfairHand] = ChozoStatuePlmNativeRoutines.WaitForLowerNorfairHand,
            [ShaktoolRoomPreInstructions.Controller] = ShaktoolRoomPlmNativeProgram.PreInstruction,
            [WreckedShipAtticPreInstructions.NoOpCallback] = WreckedShipAtticPlmRomData.NoOpCallback,
            [SamusEaterPreInstructions.Hold] = SamusEaterPlmRomDataConstants.HoldPreInstruction,
            [ColoredDoorPreInstructions.WakeOnPowerBomb] = (ushort)ColoredDoorPreInstruction.WakeOnPowerBomb,
            [ColoredDoorPreInstructions.WakeOnMissile] = (ushort)ColoredDoorPreInstruction.WakeOnMissile,
            [ColoredDoorPreInstructions.WakeOnSuperMissile] = (ushort)ColoredDoorPreInstruction.WakeOnSuperMissile,
            [EyeDoorPreInstructions.WakeWhenDoorBitSet] = (ushort)EyeDoorPreInstruction.WakeWhenDoorBitSet,
            [EyeDoorPreInstructions.MissileHit] = (ushort)EyeDoorPreInstruction.MissileHit,
            [DownwardGatePreInstructions.WakeIfTriggered] = (ushort)DownwardGatePreInstruction.WakeIfTriggered,
            [DownwardGatePreInstructions.WakeIfTriggeredOrSamusBelow] = (ushort)DownwardGatePreInstruction.WakeIfTriggeredOrSamusBelow,
            [DownwardGatePreInstructions.Inert] = (ushort)DownwardGatePreInstruction.Inert,
            [NoobTubePreInstructions.WakeOnPowerBomb] = (ushort)NoobTubePlmPreInstruction.WakeOnPowerBomb,
            [NoobTubePreInstructions.WakeOnAcceptedInput] = (ushort)NoobTubePlmPreInstruction.WakeOnAcceptedInput,
            [NoobTubePreInstructions.Inactive] = (ushort)NoobTubePlmPreInstruction.Inactive,
        };
        foreach (GreyDoorPreInstruction routine in Enum.GetValues<GreyDoorPreInstruction>().Where(routine => routine != GreyDoorPreInstruction.None))
            routines.Add(GreyDoorPreInstructions.Of(routine), (ushort)routine);
        foreach (SpeedBoosterEscapePreInstruction routine in Enum.GetValues<SpeedBoosterEscapePreInstruction>()
                     .Where(routine => routine != SpeedBoosterEscapePreInstruction.None))
            routines.Add(SpeedBoosterEscapePreInstructions.Of(routine), (ushort)routine);
        for (ushort roomArgument = 0; roomArgument < 26; roomArgument += 2)
            routines.Add(TourianPreInstructions.MetroidsClearedFor(roomArgument), MetroidsCleared(roomArgument));
        return routines;
    }
}
