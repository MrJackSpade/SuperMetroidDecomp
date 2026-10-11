using SuperMetroid.Core.Spatial;
namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    private static void WriteMode7Gameplay(BinaryWriter writer, Mode7GameplayRenderLayer layer)
    {
        Mode7RenderRegisters m = layer.Registers;
        writer.Write(m.MatrixA); writer.Write(m.MatrixB); writer.Write(m.MatrixC); writer.Write(m.MatrixD);
        WriteOffset(writer, m.Center);
        WriteOffset(writer, m.Scroll); writer.Write((byte)Mode7OverflowPolicy.FromRegisters(m));
        writer.Write(layer.HudTilemapWord); writer.Write(layer.HudCharacterWord); writer.Write(layer.HudScanlines);
        writer.Write(layer.Floor.HasValue);
        if (layer.Floor is { } f)
        {
            writer.Write(f.FirstScanline); writer.Write(f.TilemapWord); writer.Write(f.CharacterWord);
            WritePosition(writer, f.Scroll);
            WriteMapSize(writer, f.MapSize);
        }
    }

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
