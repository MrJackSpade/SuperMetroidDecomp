using SuperMetroid.Core.Spatial;
namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    private static Mode7GameplayRenderLayer ReadMode7Gameplay(BinaryReader reader, ushort version)
    {
        var m = ReadMode7Registers(reader, version);
        ushort hudMap = reader.ReadUInt16(), hudCharacters = reader.ReadUInt16();
        int hudLines = reader.ReadInt32();
        Mode1FloorBand? floor = ReadBoolean(reader)
            ? new(reader.ReadInt32(), reader.ReadUInt16(), reader.ReadUInt16(), ReadPosition<Bg2Pixels>(reader),
                ReadMapSize(reader)) : null;
        return new(m, hudMap, hudCharacters, hudLines, floor);
    }
}
