using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.WreckedShip;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The bank-$84 routine each shared pre-instruction ports, for comparing slots with native PLM RAM.</summary>
internal static class RoomObjectFrameBehaviorNativeRoutines
{
    /// <summary>The first of the Metroids-cleared table's nine one-byte no-op handlers (<c>$84:DAD5</c>).</summary>
    private const ushort MetroidsClearedFirstNoOp = 0xdad5;

    /// <summary>The first of its four sixteen-byte quota observers (<c>$84:DADE</c>).</summary>
    private const ushort MetroidsClearedFirstQuotaObserver = 0xdade;

    /// <summary>Every shared pre-instruction with its routine.</summary>
    private static readonly Dictionary<IRoomObjectFrameBehavior, ushort> Routines = Build();

    /// <summary>The routine <paramref name="frameBehavior"/> ports.</summary>
    /// <exception cref="InvalidOperationException">The instance is not a shared pre-instruction.</exception>
    internal static ushort Of(IRoomObjectFrameBehavior frameBehavior) =>
        Routines.TryGetValue(frameBehavior, out ushort routine)
            ? routine
            : throw new InvalidOperationException($"PLM pre-instruction {frameBehavior} has no native routine mapping.");

    /// <summary>The routine a Metroids-cleared PLM installs for <paramref name="roomArgument"/>.</summary>
    internal static ushort MetroidsCleared(ushort roomArgument)
    {
        int entry = RoomObjectRoomArgumentWords.Decode(RoomObjectHeaderId.MetroidRoomClearance, roomArgument) is MetroidRoomArgument metroids
            ? metroids.Entry : throw new InvalidDataException($"Metroids-cleared argument ${roomArgument:X4} decodes to no entry.");
        MetroidsClearedEvents.ResolveEvent(entry);
        return (ushort)(entry < 9 ? MetroidsClearedFirstNoOp + entry : MetroidsClearedFirstQuotaObserver + (entry - 9) * 16);
    }

    /// <summary>Maps every shared instance, rejecting an instance listed twice.</summary>
    private static Dictionary<IRoomObjectFrameBehavior, ushort> Build()
    {
        var routines = new Dictionary<IRoomObjectFrameBehavior, ushort>
        {
            [RoomObjectFrameBehaviors.Inert] = RoomObjectFrameBehaviorDefinitions.SpawnDefault,
            [TourianFrameBehaviors.OldTourianEscapeShaftWallWaitForSamus] = OldTourianEscapeShaftWallObjectDefinitionsConstants.WaitForSamusFrameBehavior,
            [DraygonCannonFrameBehaviors.MissileHit] = DraygonNativeWords.MissileHitFrameBehavior,
            [MotherBrainGlassFrameBehaviors.Hit] = MotherBrainGlassObjectProgramDefinitionsConstants.HitFrameBehavior,
            [BombTorizoHandFrameBehaviors.WakeIfSamusHasBombs] = BombTorizoHandObjectProgramDefinitionsConstants.WakeIfSamusHasBombsFrameBehavior,
            [ChozoStatueFrameBehaviors.WaitForLowerNorfairHand] = ChozoStatueObjectNativeRoutines.WaitForLowerNorfairHand,
            [ShaktoolRoomFrameBehaviors.Controller] = ShaktoolRoomObjectNativeProgram.FrameBehavior,
            [WreckedShipAtticFrameBehaviors.NoOpCallback] = WreckedShipAtticObjectRomData.NoOpCallback,
            [SamusEaterFrameBehaviors.Hold] = SamusEaterObjectDefinitionsConstants.HoldFrameBehavior,
            [DownwardGateFrameBehaviors.WakeIfTriggered] = RoomObjectFrameBehaviorNativeWords.DownwardGate.Word(DownwardGateFrameBehavior.WakeIfTriggered),
            [DownwardGateFrameBehaviors.WakeIfTriggeredOrSamusBelow] = RoomObjectFrameBehaviorNativeWords.DownwardGate.Word(DownwardGateFrameBehavior.WakeIfTriggeredOrSamusBelow),
            [DownwardGateFrameBehaviors.Inert] = RoomObjectFrameBehaviorNativeWords.DownwardGate.Word(DownwardGateFrameBehavior.Inert),
            [NoobTubeFrameBehaviors.WakeOnPowerBomb] = RoomObjectFrameBehaviorNativeWords.NoobTube.Word(NoobTubeObjectFrameBehavior.WakeOnPowerBomb),
            [NoobTubeFrameBehaviors.WakeOnAcceptedInput] = RoomObjectFrameBehaviorNativeWords.NoobTube.Word(NoobTubeObjectFrameBehavior.WakeOnAcceptedInput),
            [NoobTubeFrameBehaviors.Inactive] = RoomObjectFrameBehaviorNativeWords.NoobTube.Word(NoobTubeObjectFrameBehavior.Inactive),
        };
        foreach (SpeedBoosterEscapeFrameBehavior routine in Enum.GetValues<SpeedBoosterEscapeFrameBehavior>()
                     .Where(routine => routine != SpeedBoosterEscapeFrameBehavior.None))
            routines.Add(SpeedBoosterEscapeFrameBehaviors.Of(routine), RoomObjectFrameBehaviorNativeWords.SpeedBoosterEscape.Word(routine));
        for (ushort roomArgument = 0; roomArgument < 26; roomArgument += 2)
            routines.Add(TourianFrameBehaviors.MetroidsClearedFor(roomArgument / 2), MetroidsCleared(roomArgument));
        return routines;
    }
}
