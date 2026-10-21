using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Tourian PLMs: the access gate, the Metroids-cleared states, and the old Tourian escape-shaft wall.</summary>
[RoomLifetime]
internal sealed class TourianPlms
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomPlmEnemyEffects? _enemies;
    /// <summary>Native forty-slot PLM table scanned from the final slot during hardcoded spawns.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds Tourian room callbacks to the active room's services and PLM slots.</summary>
    internal TourianPlms(IRoomSamusAccess? samus, IGlobalEventFlags? events, IRoomPlmEnemyEffects? enemies, PlmSlotTable slotTable)
    {
        _samus = samus;
        _events = events;
        _enemies = enemies;
        _slotTable = slotTable;
    }
    /// <summary>
    /// Applies setup <c>$84:DB1E</c>: select one bank-$84 pre-instruction through the exact
    /// room-argument table and leave the actor on its permanent sleeping instruction list.
    /// </summary>
    internal static void SetupMetroidsClearedSlot(PlmSlot slot)
    {
        slot.PreInstruction = TourianPreInstructions.MetroidsClearedFor(slot.RoomArgument);
        slot.StartAnimation(new RoomPlmSharedAnimation(RoomPlmSharedProgram.Sleep));
    }

    /// <summary>
    /// Runs the selected pre-instruction before the ordinary timer/list pass, matching
    /// <c>PLM_Handler</c>. Arguments below $12 are deliberate RTS handlers. The four active
    /// handlers compare the complete enemy-death word with the room's required-kill byte and
    /// repeatedly mark their idempotent event once the quota is reached.
    /// </summary>
    internal void RunMetroidsClearedObserver(EventNumber eventNumber, int enemyDeaths, int enemyDeathQuota)
    {
        if (enemyDeaths < enemyDeathQuota)
            return;

        (_events ?? throw new InvalidOperationException(
            "Metroids-cleared PLM reached its kill quota without a room event writer.")).Set(eventNumber);
    }

    /// <summary>
    /// Runs room setup $8F:91A9: Spawn_Hardcoded_PLM at block ($10,$87), whose setup
    /// $84:B3C1 clears the block's collision bits before the list starts on timer one.
    /// </summary>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnOldTourianEscapeShaftWall(RoomLevelData level)
    {
        Ensure.NotNull(level);
        int blockIndex = level.GetBlockIndex(
            new(OldTourianEscapeShaftWallPlmDefinitions.BlockX, OldTourianEscapeShaftWallPlmDefinitions.BlockY));
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.RestoreLevelWord = 0;
        slot.HeaderPointer = PlmHeaderId.OldTourianEscapeShaftFakeWall;
        slot.LoopTimer = 0;
        DoorPlms.SetupDoorTransitionDeactivatedSlot(level, slot);
        slot.StartAnimation(new ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram.OldTourianWallWait));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        return true;
    }

    /// <summary>
    /// Ports pre-instruction $84:B927. WakePLMIfSamusIsBelowRightOfTarget ($84:B8FD) wakes
    /// the list past its sleep and leaves carry clear, which spawns the explosion; while
    /// Samus is not strictly below and right of the target it returns carry set and nothing
    /// happens.
    /// </summary>
    internal void RunOldTourianEscapeShaftWallWait(PlmSlot slot)
    {
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Old Tourian escape-shaft wall has no live Samus owner.");
        if (OldTourianEscapeShaftWallPlmDefinitions.WakeTargetX >= samus.Position.X.Whole ||
            OldTourianEscapeShaftWallPlmDefinitions.WakeTargetY >= samus.Position.Y.Whole)
            return;

        // $84:B90B-$B914 advances past the sleep and arms timer one; unlike other wakes it
        // leaves the loop timer alone.
        (slot.Animation ?? throw new InvalidOperationException("The old Tourian escape wall waits without its program.")).WakePastSleep();
        slot.InstructionTimer = WrappingCountdown.Start(1);
        (_enemies ?? throw new InvalidOperationException(
            "Old Tourian escape-shaft wall has no enemy-projectile owner.")).SpawnOldTourianEscapeShaftWallExplosion();
    }
    /// <summary>$88:DB8A/DC69 spawn the native clear/crumble lists at block (6,12).</summary>
    public bool TrySpawnTourianAccess(RoomLevelData level, TourianAccessFloor floor)
    {
        TourianAccessPlmDefinition definition = TourianAccessPlmDefinitions.ForState(floor);
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.HeaderPointer = definition.HeaderPointer;
        slot.BlockIndex = level.GetBlockIndex(new(6, 12));
        slot.StartAnimation(new TourianAccessPlmAnimation(definition.Program));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        return true;
    }
}
