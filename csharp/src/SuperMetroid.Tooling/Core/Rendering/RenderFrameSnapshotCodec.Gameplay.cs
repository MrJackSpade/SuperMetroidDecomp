using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    /// <summary>Serializes gameplay registers, scroll tables, window state, mosaic size, and per-line main-screen layers.</summary>
    /// <param name="writer">Destination for the fields in the render-packet gameplay-layer format.</param>
    /// <param name="layer">Gameplay render layer whose state is written in the codec's established field order.</param>
    private static void WriteGameplayLayer(BinaryWriter writer, OrdinaryGameplayRenderLayer layer)
    {
        OrdinaryGameplayRegisters r = layer.Registers;
        writer.Write(r.Bg1X); writer.Write(r.Bg1Y); writer.Write(r.Bg2X); writer.Write(r.Bg2Y);
        writer.Write(r.Bg2WidthTiles); writer.Write(r.Bg2HeightTiles); writer.Write(r.Bg2TilemapWord);
        writer.Write(r.Bg1CharacterWord); writer.Write(r.Bg2CharacterWord); writer.Write(r.HudCharacterWord);
        writer.Write((byte)r.MainScreenLayers);
        WriteScrolls(writer, layer.HorizontalScrolls);
        WriteScrolls(writer, layer.VerticalScrolls);
        SnesWindowRegisters w = r.Windows;
        writer.Write(w.Window12Selection); writer.Write(w.Window34Selection); writer.Write(w.ObjectColorSelection);
        writer.Write(w.FirstLeft); writer.Write(w.FirstRight); writer.Write(w.SecondLeft); writer.Write(w.SecondRight);
        writer.Write(w.BackgroundLogic); writer.Write(w.ObjectColorLogic);
        writer.Write((byte)r.MainScreenWindowMask);
        writer.Write((byte)r.Bg2Mosaic.Size);
        WriteScrolls(writer, layer.MainScreenLayersByLine);
    }

    /// <summary>Reads gameplay-layer state, using the packet version to determine which later fields are present.</summary>
    /// <param name="reader">Source positioned at the gameplay-layer fields.</param>
    /// <param name="version">Render-packet version controlling compatibility reads for windows, mosaic, and per-line layers.</param>
    /// <returns>The decoded registers and scroll tables, with absent versioned fields supplied by their defaults.</returns>
    private static OrdinaryGameplayRenderLayer ReadGameplayLayer(BinaryReader reader, ushort version)
    {
        var registers = new OrdinaryGameplayRegisters(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadInt32(), reader.ReadInt32(),
            reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(),
            (SnesMainScreenLayers)reader.ReadByte());
        ushort[] horizontal = ReadScrolls(reader), vertical = ReadScrolls(reader);
        if (version >= RenderPacketFormat.GameplayWindowVersion)
        {
            registers = registers with
            {
                Windows = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
                    reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                MainScreenWindowMask = (SnesMainScreenLayers)reader.ReadByte(),
            };
        }
        if (version >= RenderPacketFormat.GameplayMosaicVersion)
            registers = registers with { Bg2Mosaic = new(reader.ReadByte()) };
        ushort[] mainScreen = version >= RenderPacketFormat.GameplayMainScreenHdmaVersion ? ReadScrolls(reader) : [];
        return new(registers, horizontal, vertical, mainScreen);
    }

    /// <summary>Writes a presence marker followed by the supplied per-line scroll values.</summary>
    /// <param name="writer">Destination stream for the marker and values.</param>
    /// <param name="values">Scroll words to serialize; an empty span is encoded as absent.</param>
    private static void WriteScrolls(BinaryWriter writer, ReadOnlySpan<ushort> values)
    {
        writer.Write(!values.IsEmpty);
        foreach (ushort value in values) writer.Write(value);
    }

    /// <summary>Reads a scroll table encoded with the codec's presence marker and fixed gameplay-line length.</summary>
    /// <param name="reader">Source positioned at the scroll-table presence marker.</param>
    /// <returns>An empty array when absent, or one scroll word for each gameplay scanline.</returns>
    private static ushort[] ReadScrolls(BinaryReader reader)
    {
        if (!ReadBoolean(reader)) return [];
        var values = new ushort[SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels];
        for (int i = 0; i < values.Length; i++) values[i] = reader.ReadUInt16();
        return values;
    }
}
