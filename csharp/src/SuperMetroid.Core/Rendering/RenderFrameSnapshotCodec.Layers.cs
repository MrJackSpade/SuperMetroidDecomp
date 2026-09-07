namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    private static void WriteLayer(BinaryWriter writer, RenderLayer layer)
    {
        switch (layer)
        {
            case ObjRenderLayer:
                writer.Write((byte)RenderPacketLayerKind.Obj);
                break;
            case ObjPriorityRenderLayer obj:
                writer.Write((byte)RenderPacketLayerKind.ObjPriority);
                writer.Write(obj.Priority);
                break;
            case Bg4BppRenderLayer bg:
                writer.Write((byte)RenderPacketLayerKind.Bg4Bpp);
                writer.Write(bg.TilemapWord); writer.Write(bg.CharacterWord);
                writer.Write(bg.HorizontalScroll); writer.Write(bg.VerticalScroll);
                writer.Write(bg.MapWidthTiles); writer.Write(bg.MapHeightTiles);
                writer.Write((byte)(bg.Priority is null ? RenderPacketPriority.All
                    : bg.Priority.Value ? RenderPacketPriority.High : RenderPacketPriority.Low));
                break;
            case Bg2BppRenderLayer bg:
                writer.Write((byte)RenderPacketLayerKind.Bg2Bpp);
                writer.Write(bg.TilemapWord); writer.Write(bg.CharacterWord);
                writer.Write(bg.RowCount); writer.Write(bg.Priority);
                break;
            default:
                throw new InvalidDataException("Unsupported layer in display fixture.");
        }
    }

    private static RenderLayer ReadLayer(BinaryReader reader) => (RenderPacketLayerKind)reader.ReadByte() switch
    {
        RenderPacketLayerKind.Obj => new ObjRenderLayer(),
        RenderPacketLayerKind.ObjPriority => new ObjPriorityRenderLayer(reader.ReadByte()),
        RenderPacketLayerKind.Bg4Bpp => new Bg4BppRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadInt32(), reader.ReadInt32(), ReadPriority(reader)),
        RenderPacketLayerKind.Bg2Bpp => new Bg2BppRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadInt32(), ReadBoolean(reader)),
        _ => throw new InvalidDataException("Unknown layer kind in display fixture."),
    };

    private static bool? ReadPriority(BinaryReader reader) => (RenderPacketPriority)reader.ReadByte() switch
    {
        RenderPacketPriority.All => null,
        RenderPacketPriority.Low => false,
        RenderPacketPriority.High => true,
        _ => throw new InvalidDataException("Unknown tile-priority selector in display fixture."),
    };
}
