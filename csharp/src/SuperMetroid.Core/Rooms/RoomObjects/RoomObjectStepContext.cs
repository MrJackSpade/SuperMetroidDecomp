using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What one room-object handler pass reaches: the room-object owners, the room and the frame's inputs. Family steps, per-frame steps and
/// typed programs all run against it.
/// </summary>
/// <param name="RoomObjects">The room's room-object owners.</param>
/// <param name="Level">The room's level data, which draws and terrain commands change.</param>
/// <param name="Frame">What the handler reads from the rest of the frame.</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct RoomObjectStepContext(
    IRoomObjects RoomObjects, RoomLevelData Level, RoomObjectFrameInputs Frame)
{
    /// <summary>Draws <paramref name="draw"/> at the slot's block, as a timed draw does.</summary>
    internal void Draw(RoomObjectSlot slot, RoomObjectDraw draw) => RoomObjects.Drawing.Draw(Level, slot.Block, draw);

    /// <summary>
    /// Redraws the slot's saved terrain at its block, as the draw-block step restores a
    /// respawning block.
    /// </summary>
    /// <exception cref="InvalidOperationException">The slot's object saved no block to restore.</exception>
    internal void DrawRestoredBlock(RoomObjectSlot slot) =>
        RoomObjectLevelWrites.DrawTerrain(Level, slot.Block, slot.RestoreTerrain
            ?? throw new InvalidOperationException($"The room object at block {slot.Block} restores a block its setup never saved."));

    /// <summary>Queues <paramref name="sound"/> at <paramref name="priority"/>.</summary>
    internal void QueueSound(SoundEffectId sound, SoundRequestPriority priority) =>
        RoomObjects.Sounds.Queue(sound, Priority: priority);

    /// <summary>
    /// Moves the slot's origin by whole blocks, as the game's move-room-object cues do: through the room's blocks in
    /// reading order (<see cref="RoomLevelData.ObjectBlockStep"/>).
    /// </summary>
    internal void MoveOrigin(RoomObjectSlot slot, Offset16<RoomBlocks> step) =>
        slot.Block = Level.ObjectBlockStep(slot.Block, step);
}
