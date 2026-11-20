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
    private readonly IStoryEvents _events;
    /// <summary>The room's room objects, which the escape passage and the rescue wall's reaction join.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>What the struck rescue wall shows: its installed picture (#1580).</summary>
    private readonly IRoomObjectDrawing _drawing;

    /// <summary>Binds the animal-rescue room-object behaviors to room events, the shared room-object places and the installed pictures.</summary>
    internal EscapeAnimalObjects(IStoryEvents events, RoomObjectPlaceList places, IRoomObjectDrawing drawing)
    {
        _events = events;
        _places = places;
        _drawing = Ensure.NotNull(drawing);
    }
    /// <summary>
    /// Runs the room setup that spawns the rescue wall's setup object at <paramref name="origin"/> (#2495), whose setup turns
    /// that block into the wall's shootable origin and the two below it into blocks that extend it. Only each block's
    /// collision kind and behaviour change: its picture and what it shows stay. The object holds its place until the room
    /// objects first update, and leaves the room then.
    /// </summary>
    /// <returns>
    /// False when the room is full; nothing is spawned and the wall's blocks stay as they were then
    /// (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </returns>
    public bool TrySpawnCrittersEscapeWall(RoomLevelData level, Position16<RoomBlocks> origin)
    {
        Ensure.NotNull(level);
        if (_places.Spawn() is not { } place)
            return false;

        place.Block = origin;
        WriteWallBlock(level, origin, EscapeAnimalWallDefinitions.OriginCollisionType, EscapeAnimalWallDefinitions.ReactionBehavior);
        for (int row = 1; row < EscapeAnimalWallDefinitions.HeightInBlocks; row++)
            WriteWallBlock(level, origin + new Offset16<RoomBlocks>(0, row), EscapeAnimalWallDefinitions.ExtensionCollisionType,
                EscapeAnimalWallDefinitions.ExtensionBehavior);
        place.Become(new CrittersEscapeWallSetupObject());
        return true;
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
        if (_places.IsFull)
            return false;
        if (!_events.Has(StoryEvent.CrittersEscaped))
            return false;
        if (_places.Spawn() is not { } place)
            return false;

        place.Block = level.GridBlock(
            new(CrateriaMainstreetEscapePassageObjectDefinitions.BlockX, CrateriaMainstreetEscapePassageObjectDefinitions.BlockY));
        place.Become(new TerrainChangeObject(ScriptedTerrainChangeDefinitions.CrateriaEscapePassage));
        return true;
    }

    /// <summary>Spawns the rescue wall's reaction, which breaks the wall open at its place in the room's update order.</summary>
    public bool TrySpawnCrittersEscapeReaction(RoomLevelData level, Position16<RoomBlocks> position,
        BlockStrike strike)
    {
        // The grapple beam's touch is rejected; every projectile, beams and bombs alike, is accepted.
        if (strike is BlockStrike.GrappleTouch)
            return false;
        if (_places.Spawn() is not { } place)
            return false;

        place.Block = position;
        place.Become(new CritterWallBreakthroughObject());
        RoomCollisionBlock block = level.GetCollisionBlock(position);
        // The reaction shows the struck wall's installed picture over the wall's collision type; until its sequence runs, the
        // block is deactivated, staying solid only when it was. The wall breaks open for good, so it saves no block to
        // restore.
        level.SetTerrain(position, block.Terrain.WithAppearance(_drawing.SetupBlock(RoomObjectSetupBlock.CritterWallStruck)).Deactivated());
        return true;
    }

    /// <summary>
    /// Gives one of the rescue wall's blocks its collision kind and behaviour, as the wall's setup rewrites only a block's
    /// type and behaviour: the block keeps its picture and what it shows.
    /// </summary>
    private static void WriteWallBlock(RoomLevelData level, Position16<RoomBlocks> block, RoomCollisionType collision,
        RoomBlockBehavior behavior)
    {
        level.SetCollision(block, collision);
        level.SetBehavior(block, behavior);
    }
}
