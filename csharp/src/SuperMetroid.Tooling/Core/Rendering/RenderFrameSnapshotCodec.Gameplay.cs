using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
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

    private static ushort[] ReadScrolls(BinaryReader reader)
    {
        if (!ReadBoolean(reader)) return [];
        var values = new ushort[SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels];
        for (int i = 0; i < values.Length; i++) values[i] = reader.ReadUInt16();
        return values;
    }
}
