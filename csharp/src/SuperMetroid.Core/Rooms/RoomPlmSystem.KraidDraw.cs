namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
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
