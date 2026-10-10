namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>Draws each cell of a compiled Kraid block instruction, applying visual overrides without replacing its physical level words.</summary>
    /// <param name="level">Room map whose cells receive the draw instruction's physical words.</param>
    /// <param name="streamer">Tilemap streamer used to publish the resulting draw.</param>
    /// <param name="draw">Compiled instruction describing the cells and their native physical words.</param>
    /// <param name="originX">Horizontal room-cell coordinate where the instruction begins.</param>
    /// <param name="originY">Vertical room-cell coordinate for the instruction's row.</param>
    /// <param name="layer1XPosition">Current horizontal Layer 1 camera position.</param>
    /// <param name="layer1YPosition">Current vertical Layer 1 camera position.</param>
    /// <param name="bg1XOffset">Horizontal offset applied when mapping Layer 1 into the streamed background.</param>
    /// <param name="visuals">Optional catalog of appearance-only overrides for the compiled instruction.</param>
    private void DrawKraidBlockInstruction(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        KraidRoomPlmDrawDefinitions.Draw draw,
        int originX, int originY,
        ushort layer1XPosition, ushort layer1YPosition, ushort bg1XOffset,
        RoomPlmKraidVisualCatalog? visuals)
    {
        for (int block = 0; block < draw.BlockCount; block++)
        {
            ushort physicalWord = draw.WordAt(block);
            ushort visualWord = visuals?.GetWord(draw.Pointer, 0, block)
                ?? new RoomLevelWord(physicalWord).VisualWord;
            DrawPlmWordAt(level, streamer, draw.Pointer, originX + block, originY,
                physicalWord, layer1XPosition, layer1YPosition, bg1XOffset, visualWord);
        }
    }
}
