using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The critters-escape PLMs: the shootable rescue wall, its reaction list, and the Crateria passage that opens only when the animals escaped.</summary>
[RoomLifetime]
internal sealed class EscapeAnimalPlms : ICrittersEscapeReaction
{
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>Native PLM slot pool searched from the final slot toward the first.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds the animal-rescue PLM behaviors to room events and the shared slot table.</summary>
    internal EscapeAnimalPlms(IGlobalEventFlags? events, PlmSlotTable slotTable)
    {
        _events = events;
        _slotTable = slotTable;
    }
    /// <summary>Runs $84:B9C5: make the three-block rescue wall shootable, preserving artwork.</summary>
    public void SetupCrittersEscapeBlock(RoomLevelData level, int blockIndex)
    {
        // Hardcoded spawn cannot execute setup when every native PLM slot is occupied.
        if (_slotTable.IsFull)
            throw new InvalidOperationException("Animal rescue wall setup has no free PLM slot.");
        PlmLevelWrites.WritePlmCollisionAndBehavior(level, blockIndex, EscapeAnimalWallDefinitions.OriginCollisionType,
            EscapeAnimalWallDefinitions.ReactionBehavior);
        for (int row = 1; row <= 2; row++)
            PlmLevelWrites.WritePlmCollisionAndBehavior(level, blockIndex + row * level.Size.X,
                EscapeAnimalWallDefinitions.ExtensionCollisionType,
                EscapeAnimalWallDefinitions.ExtensionBehavior);
    }

    /// <summary>
    /// Runs room setup $8F:9194's Spawn_Hardcoded_PLM of $84:BB30 at block ($3D,$0B). Setup
    /// $84:BB09 clears the PLM ID unless event $0F (critters escaped) is set, so the passage
    /// stays shut when the animals were left behind.
    /// </summary>
    /// <returns>False when no PLM remains: the event is unset or every slot is occupied.</returns>
    public bool TrySpawnCrateriaMainstreetEscapePassage(RoomLevelData level)
    {
        Ensure.NotNull(level);
        IGlobalEventFlags events = _events ?? throw new InvalidOperationException(
            "Crateria mainstreet escape passage requires the room event owner.");
        if (_slotTable.IsFull)
            return false;
        if (!events.Has(EventNumber.CrittersEscaped))
            return false;
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = level.GetBlockIndex(
            new(CrateriaMainstreetEscapePassagePlmDefinitions.BlockX, CrateriaMainstreetEscapePassagePlmDefinitions.BlockY));
        slot.Header = PlmHeaderId.CrateriaMainstreetEscapePassage;
        slot.LoopTimer = 0;
        slot.StartAnimation(new ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram.CrateriaPassage));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        return true;
    }

    /// <summary>Runs $84:B978 and schedules its ROM animation/event list in native slot order.</summary>
    public bool TrySpawnCrittersEscapeReaction(RoomLevelData level, int blockIndex,
        SamusProjectileType projectileType)
    {
        // The grapple beam's touch is rejected; every projectile, beams and bombs alike, is accepted.
        if (projectileType.IsNone)
            return false;
        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.StartAnimation(new ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram.EscapeAnimalReaction));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        // The reaction keeps the wall's collision type and shows the reaction block; until its list runs, the block
        // is deactivated, staying solid only when it was.
        slot.RestoreTerrain = block.Terrain.WithAppearance(EscapeAnimalWallDefinitions.ReactionAppearance);
        level.SetTerrain(blockIndex, slot.RestoreTerrain.Deactivated());
        return true;
    }
}
