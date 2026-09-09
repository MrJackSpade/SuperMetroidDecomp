namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    private static void WriteMode7Gameplay(BinaryWriter writer, Mode7GameplayRenderLayer layer)
    {
        Mode7RenderRegisters m = layer.Registers;
        writer.Write(m.MatrixA); writer.Write(m.MatrixB); writer.Write(m.MatrixC); writer.Write(m.MatrixD);
        writer.Write(m.CenterX); writer.Write(m.CenterY);
        writer.Write(m.HorizontalOffset); writer.Write(m.VerticalOffset); writer.Write((byte)Mode7OverflowPolicy.FromRegisters(m));
        writer.Write(layer.HudTilemapWord); writer.Write(layer.HudCharacterWord); writer.Write(layer.HudScanlines);
        writer.Write(layer.Floor.HasValue);
        if (layer.Floor is { } f)
        {
            writer.Write(f.FirstScanline); writer.Write(f.TilemapWord); writer.Write(f.CharacterWord);
            writer.Write(f.HorizontalScroll); writer.Write(f.VerticalScroll);
            writer.Write(f.MapWidthTiles); writer.Write(f.MapHeightTiles);
        }
    }

    private static Mode7GameplayRenderLayer ReadMode7Gameplay(BinaryReader reader, ushort version)
    {
        var m = ReadMode7Registers(reader, version);
        ushort hudMap = reader.ReadUInt16(), hudCharacters = reader.ReadUInt16();
        int hudLines = reader.ReadInt32();
        Mode1FloorBand? floor = ReadBoolean(reader)
            ? new(reader.ReadInt32(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(),
                reader.ReadInt32(), reader.ReadInt32()) : null;
        return new(m, hudMap, hudCharacters, hudLines, floor);
    }
}
