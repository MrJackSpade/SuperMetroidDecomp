using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    private static void WriteGameplayLayer(BinaryWriter writer, OrdinaryGameplayRenderLayer layer)
    {
        OrdinaryGameplayRegisters r = layer.Registers;
        WritePosition(writer, r.Bg1Scroll); WritePosition(writer, r.Bg2Scroll);
        WriteMapSize(writer, r.Bg2MapSize); writer.Write(r.Bg2TilemapWord);
        writer.Write(r.Bg1CharacterWord); writer.Write(r.Bg2CharacterWord); writer.Write(r.HudCharacterWord);
        writer.Write((byte)r.MainScreenLayers);
        WriteScrolls(writer, layer.Bg2LineScrolls.Horizontal);
        WriteScrolls(writer, layer.Bg2LineScrolls.Vertical);
        SnesWindowRegisters w = r.Windows;
        writer.Write(w.Window12Selection); writer.Write(w.Window34Selection); writer.Write(w.ObjectColorSelection);
        writer.Write(w.FirstLeft); writer.Write(w.FirstRight); writer.Write(w.SecondLeft); writer.Write(w.SecondRight);
        writer.Write(w.BackgroundLogic); writer.Write(w.ObjectColorLogic);
        writer.Write((byte)r.MainScreenWindowMask);
        writer.Write((byte)r.Bg2Mosaic.Size);
        WriteScrolls(writer, layer.MainScreenLayersByLine);
    }

    private static OrdinaryGameplayRenderLayer ReadGameplayLayer(BinaryReader reader, ushort version)
    {
        var registers = new OrdinaryGameplayRegisters(ReadPosition<RoomPixels>(reader),
            ReadPosition<Bg2Pixels>(reader), ReadMapSize(reader),
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
        return new(registers, new Bg2HdmaScrollTables(horizontal, vertical), mainScreen);
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
