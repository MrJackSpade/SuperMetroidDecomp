using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Writes <see cref="RenderFrameSnapshot"/>s as SMFRAME packets that <see cref="RenderFrameSnapshotCodec"/> reads back.
/// Only tests produce packets: codec round trips, legacy-version fixtures and the native PPU probe's input.
/// </summary>
[PpuLayoutBoundary("codec: render-frame snapshot serialization (VRAM, CGRAM and OAM images)")]
internal static class RenderFrameSnapshotWriter
{
    /// <summary>Encodes a complete immutable display snapshot into a newly allocated, bounded little-endian SMFRAME packet at the current format version.</summary>
    /// <returns>Portable bytes containing frame identity, composition, required PPU memory, and brightness passes; no CLR object identities or GPU resources are serialized.</returns>
    public static byte[] Serialize(RenderFrameSnapshot frame)
    {
        Ensure.NotNull(frame);
        // Every packet carries a complete VRAM image; start large enough to never regrow for it.
        using var stream = new MemoryStream(SnesPpuLayout.VramByteCount + 16 * 1024);
        Serialize(frame, stream);
        return stream.ToArray();
    }

    /// <summary>Writes the packet encoding to <paramref name="output"/>, e.g. a hash for comparison.</summary>
    public static void Serialize(RenderFrameSnapshot frame, Stream output)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(output);
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
                WriteOffset(writer, bg.Center);
                WriteOffset(writer, bg.Scroll);
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
    private sealed class CountingStream(Stream inner) : Stream
    {
        /// <summary>Gets the number of bytes written.</summary>
        public long Written { get; private set; }
        /// <summary>Reports that reading is unsupported.</summary>
        public override bool CanRead => false;
        /// <summary>Reports that seeking is unsupported.</summary>
        public override bool CanSeek => false;
        /// <summary>Reports that writing is supported.</summary>
        public override bool CanWrite => true;
        /// <summary>Throws because stream length is unsupported.</summary>
        public override long Length => throw new NotSupportedException();
        /// <summary>Throws because stream positioning is unsupported.</summary>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        /// <summary>Flushes the underlying destination stream.</summary>
        public override void Flush() => inner.Flush();
        /// <summary>Throws because this stream does not support reading.</summary>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        /// <summary>Throws because this stream does not support seeking.</summary>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <summary>Throws because this stream does not support resizing.</summary>
        public override void SetLength(long value) => throw new NotSupportedException();
        /// <summary>Writes a byte array segment and adds its length to the byte count.</summary>
        public override void Write(byte[] buffer, int offset, int count) { inner.Write(buffer, offset, count); Written += count; }
        /// <summary>Writes a byte span and adds its length to the byte count.</summary>
        public override void Write(ReadOnlySpan<byte> buffer) { inner.Write(buffer); Written += buffer.Length; }
        /// <summary>Writes one byte and increments the byte count.</summary>
        public override void WriteByte(byte value) { inner.WriteByte(value); Written++; }
    }

    /// <summary>write memory.</summary>
    private static void WriteMemory(BinaryWriter writer, PpuMemorySnapshot memory)
    {
        writer.Write(memory.Vram);
        foreach (Bgr555 color in memory.Cgram) writer.Write(color.ToWord());
        writer.Write(memory.Oam);
        writer.Write(memory.ModeledSpriteCount);
    }

    /// <summary>write count.</summary>
    private static void WriteCount(BinaryWriter writer, int count)
    {
        if (count > RenderPacketFormat.MaximumOperations)
            throw new InvalidDataException("Too many display operations for this format version.");
        writer.Write(count);
    }

    // The packet format stores each spatial value as its X then Y component at the original
    // field widths: unsigned words for positions, signed words for Mode 7 offsets and 32-bit
    // integers for tilemap sizes.
    /// <summary>write position.</summary>
    private static void WritePosition<TSpace>(BinaryWriter writer, Position16<TSpace> position)
        where TSpace : ISpatialSpace
    {
        writer.Write(position.X);
        writer.Write(position.Y);
    }

    /// <summary>write offset.</summary>
    private static void WriteOffset<TSpace>(BinaryWriter writer, Offset16<TSpace> offset) where TSpace : ISpatialSpace
    {
        writer.Write(offset.X);
        writer.Write(offset.Y);
    }

    /// <summary>write map size.</summary>
    private static void WriteMapSize(BinaryWriter writer, Extent16<TilemapCells> size)
    {
        writer.Write((int)size.X);
        writer.Write((int)size.Y);
    }

    /// <summary>write gameplay layer.</summary>
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

    /// <summary>write scrolls.</summary>
    private static void WriteScrolls(BinaryWriter writer, ReadOnlySpan<ushort> values)
    {
        writer.Write(!values.IsEmpty);
        foreach (ushort value in values) writer.Write(value);
    }

    /// <summary>write layer.</summary>
    private static void WriteLayer(BinaryWriter writer, RenderLayer layer)
    {
        switch (layer)
        {
            case GameplayColorMathRenderLayer gameplayXray:
                writer.Write((byte)RenderPacketLayerKind.XrayGameplay);
                WriteGameplayLayer(writer, gameplayXray.Gameplay);
                foreach (XrayWindowLine line in gameplayXray.Lines) { writer.Write(line.Left); writer.Write(line.Right); }
                writer.Write(gameplayXray.RevealBlocks); writer.Write((byte)gameplayXray.ColorMath);
                writer.Write(gameplayXray.AddSubscreen);
                writer.Write(gameplayXray.FixedRed); writer.Write(gameplayXray.FixedGreen); writer.Write(gameplayXray.FixedBlue);
                writer.Write(gameplayXray.Subscreen is not null);
                if (gameplayXray.Subscreen is { } bg3) WriteLayer(writer, bg3);
                writer.Write(gameplayXray.SubscreenUsesBg2);
                break;
            case WindowedSceneRenderLayer window:
                writer.Write((byte)RenderPacketLayerKind.WindowedScene);
                writer.Write(window.Left); writer.Write(window.Top); writer.Write(window.Right); writer.Write(window.Bottom);
                WriteMemory(writer, window.Scene.Memory);
                writer.Write(window.Scene.ObjectSelection); writer.Write(window.Scene.Brightness);
                WriteCount(writer, window.Scene.Layers.Length);
                foreach (RenderLayer child in window.Scene.Layers) WriteLayer(writer, child);
                break;
            case BgSubscreenAddRenderLayer sub:
                writer.Write((byte)(sub.VerticalScroll != 0 ? RenderPacketLayerKind.ScrolledBg4SubscreenAdd
                    : sub.FourBpp ? RenderPacketLayerKind.Bg4SubscreenAdd : RenderPacketLayerKind.BgSubscreenAdd));
                writer.Write(sub.TilemapWord); writer.Write(sub.CharacterWord); writer.Write(sub.MainCoverage is not null);
                if (sub.MainCoverage is { } coverage) WriteLayer(writer, coverage);
                writer.Write(sub.IncludeObjects);
                writer.Write(sub.MainObjects);
                if (sub.VerticalScroll != 0) writer.Write(sub.VerticalScroll);
                writer.Write(!sub.Scrolls.IsEmpty);
                foreach (Position16<BgPlanePixels> line in sub.Scrolls) WritePosition(writer, line);
                break;
            case Mode7GameplayRenderLayer gameplay7:
                writer.Write((byte)RenderPacketLayerKind.Mode7Gameplay);
                WriteMode7Gameplay(writer, gameplay7);
                break;
            case Bg2BppColorMathRenderLayer bgMath:
                writer.Write((byte)RenderPacketLayerKind.BgColorMath);
                writer.Write(bgMath.TilemapWord); writer.Write(bgMath.CharacterWord);
                writer.Write(bgMath.MapHeightTiles); writer.Write(bgMath.FirstScanline); writer.Write((byte)bgMath.Operation);
                foreach (Position16<BgPlanePixels> line in bgMath.Scrolls) WritePosition(writer, line);
                break;
            case MessageBoxRenderLayer message:
                writer.Write((byte)RenderPacketLayerKind.MessageBox);
                writer.Write((byte)message.RowCount); writer.Write((byte)message.RadiusPixels);
                foreach (ushort tile in message.Tilemap) writer.Write(tile);
                break;
            case ScanlineColorAddRenderLayer windows:
                writer.Write((byte)RenderPacketLayerKind.ScanlineColorAdd);
                foreach (ColorAddWindow line in windows.Windows)
                {
                    writer.Write(line.Left); writer.Write(line.Right);
                    writer.Write(line.Red); writer.Write(line.Green); writer.Write(line.Blue);
                }
                break;
            case OrdinaryGameplayRenderLayer gameplay:
                writer.Write((byte)RenderPacketLayerKind.OrdinaryGameplay);
                WriteGameplayLayer(writer, gameplay);
                break;
            case Bg2BppViewportRenderLayer bg:
                writer.Write((byte)RenderPacketLayerKind.Bg2Viewport);
                writer.Write(bg.TilemapWord); writer.Write(bg.CharacterWord); writer.Write(bg.VerticalScroll);
                writer.Write(bg.TransparentColorZero);
                writer.Write((byte)(bg.Priority is null ? RenderPacketPriority.All
                    : bg.Priority.Value ? RenderPacketPriority.High : RenderPacketPriority.Low));
                break;
            case FixedColorAddRenderLayer color:
                writer.Write((byte)RenderPacketLayerKind.FixedColorAdd);
                writer.Write(color.Red); writer.Write(color.Green); writer.Write(color.Blue);
                break;
            case Mode7RenderLayer layer7:
                writer.Write((byte)(layer7.AddBg1Subscreen ? RenderPacketLayerKind.Mode7Bg1Add
                    : layer7.SubtractObjSubscreen ? RenderPacketLayerKind.Mode7ObjSubtract : RenderPacketLayerKind.Mode7));
                Mode7RenderRegisters m = layer7.Registers;
                writer.Write(m.MatrixA); writer.Write(m.MatrixB); writer.Write(m.MatrixC); writer.Write(m.MatrixD);
                WriteOffset(writer, m.Center);
                WriteOffset(writer, m.Scroll);
                writer.Write((byte)Mode7OverflowPolicy.FromRegisters(m));
                break;
            case ObjRenderLayer objLayer:
                writer.Write((byte)(objLayer.AddToScreen ? RenderPacketLayerKind.ObjSubscreenAdd : RenderPacketLayerKind.Obj));
                writer.Write(objLayer.FixedColor is not null);
                if (objLayer.FixedColor is { } fixedObj)
                {
                    writer.Write(fixedObj.Red); writer.Write(fixedObj.Green); writer.Write(fixedObj.Blue);
                }
                break;
            case ObjPriorityRenderLayer obj:
                writer.Write((byte)RenderPacketLayerKind.ObjPriority);
                writer.Write(obj.Priority);
                writer.Write(obj.FixedColor is not null);
                if (obj.FixedColor is { } priorityColor)
                {
                    writer.Write(priorityColor.Red); writer.Write(priorityColor.Green); writer.Write(priorityColor.Blue);
                }
                break;
            case Bg4BppRenderLayer bg:
                writer.Write((byte)RenderPacketLayerKind.Bg4Bpp);
                writer.Write(bg.TilemapWord); writer.Write(bg.CharacterWord);
                WritePosition(writer, bg.Scroll);
                WriteMapSize(writer, bg.MapSize);
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

    /// <summary>write mode7 gameplay.</summary>
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
}
