using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What one room-object step reaches: the room-object owners, the room, the frame's inputs and the step's own outcome.
/// Family steps, per-frame steps and room-object behaviours all run against it.
/// </summary>
/// <param name="RoomObjects">The room's room-object owners.</param>
/// <param name="Level">The room's level data, which block draws and terrain changes write.</param>
/// <param name="Frame">What the room objects read from the rest of the update.</param>
/// <param name="Outcome">
/// The step's outcome, which its pickups, station actions and thrown projectiles join for the update to act on (#2514).
/// </param>
internal readonly partial record struct RoomObjectStepContext(
    IRoomObjects RoomObjects, RoomLevelData Level, RoomObjectFrameInputs Frame, RoomObjectStepOutcome Outcome)
{
    /// <summary>Draws <paramref name="draw"/> at the place's block through its family, as a timed draw does.</summary>
    internal void Draw(RoomObjectPlace place, RoomObjectDraw draw) => Ensure.NotNull(draw).Draw(Level, place.Block, RoomObjects.Drawing);

    /// <summary>Draws <paramref name="frame"/> of <paramref name="family"/> at the place's block.</summary>
    internal void Draw<TFrame>(RoomObjectPlace place, RoomObjectDrawFamily<TFrame> family, TFrame frame) where TFrame : struct =>
        Ensure.NotNull(family).Draw(Level, place.Block, RoomObjects.Drawing, frame);

    /// <summary>
    /// Redraws <paramref name="saved"/>, the terrain the place's object saved at its setup, at the place's block, as a
    /// respawning block grows back.
    /// </summary>
    internal void DrawSavedTerrain(RoomObjectPlace place, RoomTerrainBlock saved) =>
        RoomObjectLevelWrites.DrawTerrain(Level, place.Block, saved);

    /// <summary>Queues <paramref name="sound"/> at <paramref name="priority"/>.</summary>
    internal void QueueSound(SoundEffectId sound, SoundRequestPriority priority) =>
        RoomObjects.Sounds.Queue(sound, Priority: priority);

    /// <summary>
    /// Moves the place's origin by whole blocks in the room's two dimensions, as the game's move-room-object cues do; no
    /// authored move leaves the room's grid (#2572).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The move leaves the room's grid.</exception>
    internal void MoveOrigin(RoomObjectPlace place, Offset16<RoomBlocks> step) =>
        place.Block = Level.GridBlock(place.Block + step);
}
