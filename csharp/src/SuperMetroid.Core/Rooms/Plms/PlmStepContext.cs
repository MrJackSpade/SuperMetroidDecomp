using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// What one PLM handler pass reaches: the PLM owners, the room and the frame's inputs. Family steps, pre-instructions and
/// typed programs all run against it.
/// </summary>
/// <param name="Plms">The room's PLM owners.</param>
/// <param name="Level">The room's level data, which draws and terrain commands change.</param>
/// <param name="Frame">What the handler reads from the rest of the frame.</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct PlmStepContext(
    IRoomPlms Plms, RoomLevelData Level, PlmFrameInputs Frame)
{
    /// <summary>Draws <paramref name="draw"/> at the slot's block, as a timed draw does.</summary>
    internal void Draw(PlmSlot slot, PlmDraw draw) => Plms.Drawing.Draw(Level, slot.BlockIndex, draw);

    /// <summary>
    /// Redraws the slot's saved terrain at its block, as <c>$84:8B17 Instruction_PLM_DrawPLMBlock</c> restores a
    /// respawning block.
    /// </summary>
    internal void DrawRestoredBlock(PlmSlot slot) =>
        PlmLevelWrites.DrawTerrain(Level, slot.BlockIndex, slot.RestoreTerrain);

    /// <summary>Queues <paramref name="sound"/> under the native queue cap <paramref name="priority"/>.</summary>
    internal void QueueSound(SoundEffectId sound, SoundRequestPriority priority) =>
        Plms.Sounds.Queue(sound, Priority: priority);

    /// <summary>Moves the slot's origin by whole blocks, as the cartridge's move-PLM instructions do.</summary>
    internal void MoveOrigin(PlmSlot slot, int columns, int rows) =>
        slot.BlockIndex = checked(slot.BlockIndex + columns + rows * Level.Size.X);
}
