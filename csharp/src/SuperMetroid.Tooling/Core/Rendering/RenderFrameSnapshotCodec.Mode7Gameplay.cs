namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    /// <summary>Writes a gameplay Mode 7 layer's registers, HUD configuration, and optional floor band in snapshot wire order.</summary>
    /// <param name="writer">Destination for the layer payload in the enclosing render-frame snapshot.</param>
    /// <param name="layer">Resolved Mode 7 gameplay composition to serialize.</param>
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

    /// <summary>Reconstructs a gameplay Mode 7 layer from its snapshot payload.</summary>
    /// <param name="reader">Source positioned at the start of the Mode 7 gameplay layer payload.</param>
    /// <param name="version">Snapshot format version used to decode the Mode 7 registers.</param>
    /// <returns>The layer's transform, HUD settings, and optional floor band.</returns>
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
