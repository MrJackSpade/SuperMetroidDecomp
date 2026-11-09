using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Tourian PLMs: the access gate, the Metroids-cleared states, and the old Tourian escape-shaft wall.</summary>
[RoomLifetime]
internal sealed class TourianObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects? _enemies;
    /// <summary>Forty-slot PLM table scanned from the final slot during hardcoded spawns.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds Tourian room callbacks to the active room's services and PLM slots.</summary>
    internal TourianObjects(IRoomSamusAccess? samus, IGlobalEventFlags? events, IRoomObjectEnemyEffects? enemies, RoomObjectSlotTable slotTable)
    {
        _samus = samus;
        _events = events;
        _enemies = enemies;
        _slotTable = slotTable;
    }
    /// <summary>
    /// Applies the Metroids-cleared setup: select one frame behaviour through the exact
    /// room-argument table and leave the actor on its permanent sleeping instruction list.
    /// </summary>
    internal static void SetupMetroidsClearedSlot(RoomObjectSlot slot)
    {
        slot.FrameBehavior = TourianFrameBehaviors.MetroidsClearedFor((slot.Argument as MetroidRoomArgument
            ?? throw new InvalidDataException($"Metroids-cleared PLM at block {slot.BlockIndex} was placed without its entry.")).Entry);
        slot.StartAnimation(new RoomObjectSharedAnimation(RoomObjectSharedProgram.Sleep));
    }

    /// <summary>
    /// Runs the selected frame behaviour before the ordinary timer/list pass, as the PLM
    /// pass does. Arguments below 18 select handlers that do nothing. The four active
    /// handlers compare the complete enemy-death word with the room's required-kill byte and
    /// repeatedly mark their idempotent event once the quota is reached.
    /// </summary>
    internal void RunMetroidsClearedObserver(StoryEvent eventNumber, int enemyDeaths, int enemyDeathQuota)
    {
        if (enemyDeaths < enemyDeathQuota)
            return;

        (_events ?? throw new InvalidOperationException(
            "Metroids-cleared PLM reached its kill quota without a room event writer.")).Set(eventNumber);
    }

    /// <summary>
    /// Runs the room setup that spawns the PLM at block (16,135), whose setup
    /// clears the block's collision bits before the list starts on timer one.
    /// </summary>
    /// <returns>False only when all 40 PLM slots are occupied.</returns>
    public bool TrySpawnOldTourianEscapeShaftWall(RoomLevelData level)
    {
        Ensure.NotNull(level);
        int blockIndex = level.GetBlockIndex(
            new(OldTourianEscapeShaftWallObjectDefinitions.BlockX, OldTourianEscapeShaftWallObjectDefinitions.BlockY));
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.RestoreTerrain = RoomObjectSlotDefinitions.ClearedRestoreTerrain;
        slot.Header = RoomObjectHeaderId.OldTourianEscapeShaftFakeWall;
        slot.LoopTimer = WrappingCountdown.Start(0);
        DoorObjects.SetupDoorTransitionDeactivatedSlot(level, slot);
        slot.StartAnimation(new ScriptedRoomObjectAnimation(ScriptedRoomObjectProgram.OldTourianWallWait));
        slot.FrameHold = 1;
        return true;
    }

    /// <summary>
    /// Runs the fake wall's frame behaviour. Once Samus is below and right of the target, the wake advances
    /// the list past its sleep and leaves carry clear, which spawns the explosion; while
    /// Samus is not strictly below and right of the target it returns carry set and nothing
    /// happens.
    /// </summary>
    internal void RunOldTourianEscapeShaftWallWait(RoomObjectSlot slot)
    {
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Old Tourian escape-shaft wall has no live Samus owner.");
        if (OldTourianEscapeShaftWallObjectDefinitions.WakeTargetX >= samus.Position.X.Whole ||
            OldTourianEscapeShaftWallObjectDefinitions.WakeTargetY >= samus.Position.Y.Whole)
            return;

        // The wake advances past the sleep and arms timer one; unlike other wakes it
        // leaves the loop timer alone.
        (slot.Animation ?? throw new InvalidOperationException("The old Tourian escape wall waits without its program.")).WakePastSleep();
        slot.FrameHold = 1;
        (_enemies ?? throw new InvalidOperationException(
            "Old Tourian escape-shaft wall has no enemy-projectile owner.")).SpawnOldTourianEscapeShaftWallExplosion();
    }
    /// <summary>Spawns the clear or crumble animation at block (6,12).</summary>
    public bool TrySpawnTourianAccess(RoomLevelData level, TourianAccessFloor floor)
    {
        TourianAccessObjectDefinition definition = TourianAccessFloors.ForState(floor);
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = definition.Header;
        slot.BlockIndex = level.GetBlockIndex(new(6, 12));
        slot.StartAnimation(new TourianAccessObjectAnimation(definition.Program));
        slot.FrameHold = 1;
        return true;
    }
}
