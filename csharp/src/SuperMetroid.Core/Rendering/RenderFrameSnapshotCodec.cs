using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Portable little-endian display fixtures independent of assembly MVID, CLR object
/// graphs and GPU resources. Older supported versions retain their original semantics.
/// </summary>
public static partial class RenderFrameSnapshotCodec
{
    public static byte[] Serialize(RenderFrameSnapshot frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(RenderPacketFormat.Signature);
        writer.Write(RenderPacketFormat.Version);
        writer.Write(frame.Identity.Sequence);
        writer.Write(frame.Identity.Generation);
        writer.Write(frame.Identity.SimulationFrame);
        WriteCount(writer, frame.BrightnessPasses.Length);
        writer.Write(frame.BrightnessPasses);
        if (frame.SolidColor is { } color)
        {
            writer.Write((byte)RenderPacketKind.Solid);
            writer.Write(color.R); writer.Write(color.G); writer.Write(color.B); writer.Write(color.A);
        }
        else if (frame.Mode7 is { } mode7)
        {
            writer.Write((byte)RenderPacketKind.Mode7Obj);
            WriteMemory(writer, mode7.Memory);
            writer.Write(mode7.ObjectSelection);
            writer.Write(mode7.Brightness);
            writer.Write(mode7.Background.HasValue);
            if (mode7.Background is { } bg)
            {
                writer.Write(bg.MatrixA); writer.Write(bg.MatrixB); writer.Write(bg.MatrixC); writer.Write(bg.MatrixD);
                writer.Write(bg.CenterX); writer.Write(bg.CenterY);
                writer.Write(bg.HorizontalOffset); writer.Write(bg.VerticalOffset);
                writer.Write(bg.FillOutsideWithCharacterZero);
            }
        }
        else if (frame.Layers is { } layers)
        {
            writer.Write((byte)RenderPacketKind.Layered);
            WriteMemory(writer, layers.Memory);
            writer.Write(layers.ObjectSelection);
            writer.Write(layers.Brightness);
            WriteCount(writer, layers.Layers.Length);
            foreach (RenderLayer layer in layers.Layers) WriteLayer(writer, layer);
        }
        else throw new InvalidDataException("Display fixture has no supported composition.");
        writer.Flush();
        if (stream.Length > RenderPacketFormat.MaximumPacketBytes)
            throw new InvalidDataException("Display fixture exceeds the bounded packet size.");
        return stream.ToArray();
    }

    public static RenderFrameSnapshot Deserialize(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length > RenderPacketFormat.MaximumPacketBytes)
            throw new InvalidDataException("Display fixture exceeds the bounded packet size.");
        using var stream = new MemoryStream(bytes.ToArray(), writable: false);
        using var reader = new BinaryReader(stream);
        try
        {
            if (!ReadExact(reader, RenderPacketFormat.Signature.Length).AsSpan().SequenceEqual(RenderPacketFormat.Signature))
                throw new InvalidDataException("Not an SMFRAME display fixture.");
            ushort version = reader.ReadUInt16();
            if (version < RenderPacketFormat.FirstSupportedVersion || version > RenderPacketFormat.Version)
                throw new InvalidDataException($"Unsupported display fixture version {version}.");
            var identity = new RenderFrameIdentity(reader.ReadInt64(), reader.ReadInt64(), reader.ReadUInt16());
            byte[] fades = ReadExact(reader, ReadCount(reader));
            var kind = (RenderPacketKind)reader.ReadByte();
            RenderFrameSnapshot frame;
            if (kind == RenderPacketKind.Solid)
                frame = new(identity, new Rgba32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()), fades);
            else if (kind is RenderPacketKind.Mode7Obj or RenderPacketKind.Layered)
            {
                PpuMemorySnapshot memory = ReadMemory(reader);
                byte obsel = reader.ReadByte();
                byte brightness = reader.ReadByte();
                if (kind == RenderPacketKind.Mode7Obj)
                {
                    Mode7RenderRegisters? bg = ReadBoolean(reader)
                        ? new Mode7RenderRegisters(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(),
                            reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), ReadBoolean(reader))
                        : null;
                    frame = new(identity, new Mode7ObjRenderSnapshot(memory, bg, obsel, brightness), fades);
                }
                else
                {
                    var layers = new RenderLayer[ReadCount(reader)];
                    for (int i = 0; i < layers.Length; i++) layers[i] = ReadLayer(reader, version);
                    frame = new(identity, new LayeredRenderSnapshot(memory, layers, obsel, brightness), fades);
                }
            }
            else throw new InvalidDataException($"Unknown display composition kind {(byte)kind}.");
            if (stream.Position != stream.Length)
                throw new InvalidDataException("Trailing bytes after the complete display fixture.");
            return frame;
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException("Invalid display fixture field.", exception);
        }
    }

    private static void WriteMemory(BinaryWriter writer, PpuMemorySnapshot memory)
    {
        writer.Write(memory.Vram);
        foreach (ushort color in memory.Cgram) writer.Write(color);
        writer.Write(memory.Oam);
        writer.Write(memory.ModeledSpriteCount);
    }

    private static PpuMemorySnapshot ReadMemory(BinaryReader reader)
    {
        byte[] vram = ReadExact(reader, SnesPpuLayout.VramByteCount);
        var cgram = new ushort[SnesPpuLayout.CgramColorCount];
        for (int i = 0; i < cgram.Length; i++) cgram[i] = reader.ReadUInt16();
        byte[] oam = ReadExact(reader, SnesPpuLayout.OamUploadByteCount);
        return new(vram, cgram, oam, reader.ReadInt32());
    }

    private static byte[] ReadExact(BinaryReader reader, int count)
    {
        byte[] result = reader.ReadBytes(count);
        if (result.Length != count) throw new EndOfStreamException("Truncated display fixture.");
        return result;
    }

    private static void WriteCount(BinaryWriter writer, int count)
    {
        if (count > RenderPacketFormat.MaximumOperations)
            throw new InvalidDataException("Too many display operations for this format version.");
        writer.Write(count);
    }

    private static int ReadCount(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > RenderPacketFormat.MaximumOperations)
            throw new InvalidDataException($"Invalid display operation count {count}.");
        return count;
    }

    private static bool ReadBoolean(BinaryReader reader) => reader.ReadByte() switch
    {
        0 => false,
        1 => true,
        _ => throw new InvalidDataException("Noncanonical display boolean."),
    };
}
