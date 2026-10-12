using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    private void DrawKraidBlockInstruction(RoomLevelData level, BackgroundTilemapStreamer streamer, KraidRoomPlmDrawDefinitions.Draw draw, Position16<RoomBlocks> origin, PlmView view, RoomPlmKraidVisualCatalog? visuals)
    {
        for (int block = 0; block < draw.BlockCount; block++)
        {
            ushort physicalWord = draw.WordAt(block);
            ushort visualWord = visuals?.GetWord(draw.Pointer, 0, block)
                ?? new RoomLevelWord(physicalWord).VisualWord;
            DrawPlmWordAt(level, streamer, draw.Pointer, origin + new Offset16<RoomBlocks>(checked((short)block), 0), physicalWord, view, visualWord);
        }
    }
}
