using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Portable little-endian display fixtures independent of assembly MVID, CLR object
/// graphs and GPU resources. Older supported versions retain their original semantics.
/// </summary>
public static partial class RenderFrameSnapshotCodec
{
    /// <summary>Encodes a complete immutable display snapshot into a newly allocated, bounded little-endian SMFRAME packet at the current format version.</summary>
    /// <returns>Portable bytes containing frame identity, composition, required PPU memory, and brightness passes; no CLR object identities or GPU resources are serialized.</returns>
    public static byte[] Serialize(RenderFrameSnapshot frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        // Every packet carries a complete VRAM image; start large enough to never regrow for it.
        using var stream = new MemoryStream(SnesPpuLayout.VramByteCount + 16 * 1024);
        Serialize(frame, stream);
        return stream.ToArray();
    }

    /// <summary>Writes the packet encoding to <paramref name="output"/>, e.g. a hash for comparison.</summary>
    public static void Serialize(RenderFrameSnapshot frame, Stream output)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(output);
        var counted = new CountingStream(output);
        using var writer = new BinaryWriter(counted, System.Text.Encoding.UTF8, leaveOpen: true);
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
                writer.Write((byte)Mode7OverflowPolicy.FromRegisters(bg));
            }
            writer.Write(mode7.Gradient.Length != 0);
            foreach (var line in mode7.Gradient)
            {
                writer.Write(line.Red); writer.Write(line.Green); writer.Write(line.Blue); writer.Write(line.Control);
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
        if (counted.Written > RenderPacketFormat.MaximumPacketBytes)
            throw new InvalidDataException("Display fixture exceeds the bounded packet size.");
    }

    /// <summary>Forwards writes and counts them, so any destination gets the packet size bound.</summary>
    /// <param name="inner">Destination stream receiving packet bytes; this wrapper does not own or close it.</param>
    private sealed class CountingStream(Stream inner) : Stream
    {
        /// <summary>Total bytes successfully forwarded to the destination.</summary>
        public long Written { get; private set; }
        /// <summary>Always false because packet serialization does not read from the destination.</summary>
        public override bool CanRead => false;
        /// <summary>Always false because packet serialization does not reposition the destination.</summary>
        public override bool CanSeek => false;
        /// <summary>Always true; writes are forwarded to the wrapped destination.</summary>
        public override bool CanWrite => true;
        /// <summary>Unsupported because the wrapper only tracks bytes written.</summary>
        public override long Length => throw new NotSupportedException();
        /// <summary>Unsupported because the wrapper does not expose destination position.</summary>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        /// <summary>Forwards a flush request to the destination.</summary>
        public override void Flush() => inner.Flush();
        /// <summary>Unsupported because this stream is a write-only packet counter.</summary>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        /// <summary>Unsupported because this stream does not seek.</summary>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <summary>Unsupported because the wrapper cannot alter destination length.</summary>
        public override void SetLength(long value) => throw new NotSupportedException();
        /// <summary>Forwards a byte-array write and adds its length to the packet byte count.</summary>
        public override void Write(byte[] buffer, int offset, int count) { inner.Write(buffer, offset, count); Written += count; }
        /// <summary>Forwards a span write and adds its length to the packet byte count.</summary>
        public override void Write(ReadOnlySpan<byte> buffer) { inner.Write(buffer); Written += buffer.Length; }
        /// <summary>Forwards a single byte and increments the packet byte count.</summary>
        public override void WriteByte(byte value) { inner.WriteByte(value); Written++; }
    }

    /// <summary>Reads a packet held in an array directly, without first copying it.</summary>
    public static RenderFrameSnapshot Deserialize(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length > RenderPacketFormat.MaximumPacketBytes)
            throw new InvalidDataException("Display fixture exceeds the bounded packet size.");
        using var stream = new MemoryStream(bytes, writable: false);
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
                        ? ReadMode7Registers(reader, version)
                        : null;
                    var gradient = Array.Empty<Frontend.TitleGradientLine>();
                    if (version >= RenderPacketFormat.TitleGradientVersion && ReadBoolean(reader))
                    {
                        gradient = new Frontend.TitleGradientLine[SnesPpuLayout.ScreenHeightPixels];
                        for (int i = 0; i < gradient.Length; i++)
                            gradient[i] = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    }
                    frame = new(identity, new Mode7ObjRenderSnapshot(memory, bg, obsel, brightness, gradient), fades);
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

    /// <summary>Writes VRAM, little-endian CGRAM words, OAM upload bytes, and modeled sprite count in packet order.</summary>
    /// <param name="writer">Packet writer receiving the memory payload.</param>
    /// <param name="memory">PPU memory image to serialize.</param>
    private static void WriteMemory(BinaryWriter writer, PpuMemorySnapshot memory)
    {
        writer.Write(memory.Vram);
        foreach (ushort color in memory.Cgram) writer.Write(color);
        writer.Write(memory.Oam);
        writer.Write(memory.ModeledSpriteCount);
    }

    /// <summary>Reads the fixed-size PPU memory payload and modeled sprite count from a packet.</summary>
    /// <param name="reader">Reader positioned at the first VRAM byte.</param>
    /// <returns>Reconstructed VRAM, CGRAM, and OAM snapshot.</returns>
    private static PpuMemorySnapshot ReadMemory(BinaryReader reader)
    {
        byte[] vram = ReadExact(reader, SnesPpuLayout.VramByteCount);
        var cgram = new ushort[SnesPpuLayout.CgramColorCount];
        for (int i = 0; i < cgram.Length; i++) cgram[i] = reader.ReadUInt16();
        byte[] oam = ReadExact(reader, SnesPpuLayout.OamUploadByteCount);
        return new(vram, cgram, oam, reader.ReadInt32());
    }

    /// <summary>Reads exactly the requested number of bytes from the packet stream.</summary>
    /// <param name="reader">Packet reader supplying the bytes.</param>
    /// <param name="count">Required byte count.</param>
    /// <returns>The complete byte sequence.</returns>
    /// <exception cref="EndOfStreamException">The packet ends before the requested count is available.</exception>
    private static byte[] ReadExact(BinaryReader reader, int count)
    {
        byte[] result = reader.ReadBytes(count);
        if (result.Length != count) throw new EndOfStreamException("Truncated display fixture.");
        return result;
    }

    /// <summary>Writes an operation count after enforcing the format's configured maximum.</summary>
    /// <param name="writer">Packet writer receiving the 32-bit count.</param>
    /// <param name="count">Number of encoded operations.</param>
    /// <exception cref="InvalidDataException">The count exceeds the packet format limit.</exception>
    private static void WriteCount(BinaryWriter writer, int count)
    {
        if (count > RenderPacketFormat.MaximumOperations)
            throw new InvalidDataException("Too many display operations for this format version.");
        writer.Write(count);
    }

    /// <summary>Reads and validates a nonnegative operation count from the packet.</summary>
    /// <param name="reader">Packet reader positioned at the count.</param>
    /// <returns>The validated count.</returns>
    /// <exception cref="InvalidDataException">The encoded count is negative or above the supported maximum.</exception>
    private static int ReadCount(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > RenderPacketFormat.MaximumOperations)
            throw new InvalidDataException($"Invalid display operation count {count}.");
        return count;
    }

    /// <summary>Reads a canonical one-byte boolean, rejecting values outside zero and one.</summary>
    /// <param name="reader">Packet reader positioned at the boolean byte.</param>
    /// <returns>The decoded boolean value.</returns>
    /// <exception cref="InvalidDataException">The encoded byte is not zero or one.</exception>
    private static bool ReadBoolean(BinaryReader reader) => reader.ReadByte() switch
    {
        0 => false,
        1 => true,
        _ => throw new InvalidDataException("Noncanonical display boolean."),
    };

    /// <summary>Reads Mode 7 transform and overflow fields, applying compatibility rules for the packet version.</summary>
    /// <param name="reader">Packet reader positioned at the register payload.</param>
    /// <param name="version">Format version that determines whether wrap overflow is supported.</param>
    /// <returns>The decoded Mode 7 register state.</returns>
    /// <exception cref="InvalidDataException">The overflow mode is unknown or uses wrap in a version that predates support.</exception>
    private static Mode7RenderRegisters ReadMode7Registers(BinaryReader reader, ushort version)
    {
        short a = reader.ReadInt16(), b = reader.ReadInt16(), c = reader.ReadInt16(), d = reader.ReadInt16();
        short x = reader.ReadInt16(), y = reader.ReadInt16(), h = reader.ReadInt16(), v = reader.ReadInt16();
        var policy = (Mode7OverflowMode)reader.ReadByte();
        if (policy > Mode7OverflowMode.Wrap || (policy == Mode7OverflowMode.Wrap && version < RenderPacketFormat.Mode7WrapVersion))
            throw new InvalidDataException("Unsupported Mode 7 overflow operation in display fixture.");
        // The legacy boolean encoded exactly transparent=0 / character-zero=1.
        return new(a, b, c, d, x, y, h, v, policy == Mode7OverflowMode.CharacterZero, policy == Mode7OverflowMode.Wrap);
    }
}
