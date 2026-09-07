using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    private static void WriteGameplayLayer(BinaryWriter writer, OrdinaryGameplayRenderLayer layer)
    {
        OrdinaryGameplayRegisters r = layer.Registers;
        writer.Write(r.Bg1X); writer.Write(r.Bg1Y); writer.Write(r.Bg2X); writer.Write(r.Bg2Y);
        writer.Write(r.Bg2WidthTiles); writer.Write(r.Bg2HeightTiles); writer.Write(r.Bg2TilemapWord);
        writer.Write(r.Bg1CharacterWord); writer.Write(r.Bg2CharacterWord); writer.Write(r.HudCharacterWord);
        writer.Write((byte)r.MainScreenLayers);
        WriteScrolls(writer, layer.HorizontalScrolls);
        WriteScrolls(writer, layer.VerticalScrolls);
    }

    private static OrdinaryGameplayRenderLayer ReadGameplayLayer(BinaryReader reader)
    {
        var registers = new OrdinaryGameplayRegisters(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadInt32(), reader.ReadInt32(),
            reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(),
            (SnesMainScreenLayers)reader.ReadByte());
        return new(registers, ReadScrolls(reader), ReadScrolls(reader));
    }

    private static void WriteScrolls(BinaryWriter writer, ReadOnlySpan<ushort> values)
    {
        writer.Write(!values.IsEmpty);
        foreach (ushort value in values) writer.Write(value);
    }

    private static ushort[] ReadScrolls(BinaryReader reader)
    {
        if (!ReadBoolean(reader)) return [];
        var values = new ushort[SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels];
        for (int i = 0; i < values.Length; i++) values[i] = reader.ReadUInt16();
        return values;
    }
}
