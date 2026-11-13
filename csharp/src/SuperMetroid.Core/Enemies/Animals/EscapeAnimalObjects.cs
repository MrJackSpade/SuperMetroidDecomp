using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The critters-escape room objects: the shootable rescue wall, its reaction list, and the Crateria passage that opens only when the animals escaped.</summary>
[RoomLifetime]
internal sealed class EscapeAnimalObjects : ICrittersEscapeReaction
{
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>The room's room objects, which the escape passage and the rescue wall's reaction join.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>What the struck rescue wall shows: its installed picture (#1580).</summary>
    private readonly IRoomObjectDrawing _drawing;

    /// <summary>Binds the animal-rescue room-object behaviors to room events, the shared slot table and the installed pictures.</summary>
    internal EscapeAnimalObjects(IGlobalEventFlags? events, RoomObjectSlotTable slotTable, IRoomObjectDrawing drawing)
    {
        _events = events;
        _slotTable = slotTable;
        _drawing = Ensure.NotNull(drawing);
    }
    /// <summary>Makes the three-block rescue wall standing down from <paramref name="origin"/> shootable, preserving artwork.</summary>
    public void SetupCrittersEscapeBlock(RoomLevelData level, Position16<RoomBlocks> origin)
    {
        // The wall's setup runs only once its spawn has taken a place, which a full room never gives.
        if (_slotTable.IsFull)
            throw new InvalidOperationException("Animal rescue wall setup has no free room-object slot.");
        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, origin, EscapeAnimalWallDefinitions.OriginCollisionType,
            EscapeAnimalWallDefinitions.ReactionBehavior);
        for (int row = 1; row <= 2; row++)
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, origin + new Offset16<RoomBlocks>(0, row),
                EscapeAnimalWallDefinitions.ExtensionCollisionType,
                EscapeAnimalWallDefinitions.ExtensionBehavior);
    }

    /// <summary>
    /// Runs the room setup that spawns the escape wall at block (61,11). Its setup
    /// removes the wall unless the critters-escaped event is set, so the passage
    /// stays shut when the animals were left behind.
    /// </summary>
    /// <returns>
    /// False when no room object remains: the event is unset or the room is full (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </returns>
    public bool TrySpawnCrateriaMainstreetEscapePassage(RoomLevelData level)
    {
        Ensure.NotNull(level);
        IGlobalEventFlags events = _events ?? throw new InvalidOperationException(
            "Crateria mainstreet escape passage requires the room event owner.");
        if (_slotTable.IsFull)
            return false;
        if (!events.Has(StoryEvent.CrittersEscaped))
            return false;
        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = level.GridBlock(
            new(CrateriaMainstreetEscapePassageObjectDefinitions.BlockX, CrateriaMainstreetEscapePassageObjectDefinitions.BlockY));
        slot.Become(new TerrainChangeObject(ScriptedTerrainChangeDefinitions.CrateriaEscapePassage));
        return true;
    }

    /// <summary>Spawns the rescue wall's reaction, which breaks the wall open at its place in the room's update order.</summary>
    public bool TrySpawnCrittersEscapeReaction(RoomLevelData level, Position16<RoomBlocks> position,
        SamusProjectileType projectileType)
    {
        // The grapple beam's touch is rejected; every projectile, beams and bombs alike, is accepted.
        if (projectileType.IsNone)
            return false;
        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = position;
        slot.Become(new CritterWallBreakthroughObject());
        RoomCollisionBlock block = level.GetCollisionBlock(position);
        // The reaction shows the struck wall's installed picture over the wall's collision type; until its sequence runs, the
        // block is deactivated, staying solid only when it was. The wall breaks open for good, so it saves no block to
        // restore.
        level.SetTerrain(position, block.Terrain.WithAppearance(_drawing.SetupBlock(RoomObjectSetupBlock.CritterWallStruck)).Deactivated());
        return true;
    }
}
