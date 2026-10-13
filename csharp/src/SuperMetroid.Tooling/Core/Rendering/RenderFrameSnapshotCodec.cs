using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Portable little-endian display fixtures independent of assembly MVID, CLR object
/// graphs and GPU resources. Older supported versions retain their original semantics.
/// </summary>
[PpuLayoutBoundary("codec: render-frame snapshot serialization (VRAM, CGRAM and OAM images)")]
public static class RenderFrameSnapshotCodec
{
    /// <summary>Reads a packet held in an array directly, without first copying it.</summary>
    public static RenderFrameSnapshot Deserialize(byte[] bytes)
    {
        Ensure.NotNull(bytes);
        if (bytes.Length > RenderPacketFormat.MaximumPacketBytes)
            throw new InvalidDataException("Display fixture exceeds the bounded packet size.");
        using var stream = new MemoryStream(bytes, writable: false);
        using var reader = new BinaryReader(stream);
        try
        {
            if (!ReadExact(reader, RenderPacketFormat.Signature.Length).SequenceEqual(RenderPacketFormat.Signature))
                throw new InvalidDataException("Not an SMFRAME display fixture.");
            ushort version = reader.ReadUInt16();
            if (version is < RenderPacketFormat.FirstSupportedVersion or > RenderPacketFormat.Version)
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

    /// <summary>Reads the packet's complete VRAM, CGRAM, OAM, and modeled sprite count.</summary>
    private static PpuMemorySnapshot ReadMemory(BinaryReader reader)
    {
        byte[] vram = ReadExact(reader, SnesPpuLayout.VramByteCount);
        var cgram = new Bgr555[SnesPpuLayout.CgramColorCount];
        for (int i = 0; i < cgram.Length; i++) cgram[i] = Bgr555.FromWord(reader.ReadUInt16());
        byte[] oam = ReadExact(reader, SnesPpuLayout.OamUploadByteCount);
        return new(vram, cgram, oam, reader.ReadInt32());
    }

    /// <summary>Reads the required byte count or rejects a truncated display packet.</summary>
    private static byte[] ReadExact(BinaryReader reader, int count)
    {
        byte[] result = reader.ReadBytes(count);
        if (result.Length != count) throw new EndOfStreamException("Truncated display fixture.");
        return result;
    }

    /// <summary>Reads an operation count within the packet format's bound.</summary>
    private static int ReadCount(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        if (count is < 0 or > RenderPacketFormat.MaximumOperations)
            throw new InvalidDataException($"Invalid display operation count {count}.");
        return count;
    }

    /// <summary>Accepts only the canonical byte values zero and one for packet booleans.</summary>
    private static bool ReadBoolean(BinaryReader reader) => reader.ReadByte() switch
    {
        0 => false,
        1 => true,
        _ => throw new InvalidDataException("Noncanonical display boolean."),
    };

    /// <summary>Reads a Mode 7 matrix, center, scroll, and overflow policy allowed by this version.</summary>
    private static Mode7RenderRegisters ReadMode7Registers(BinaryReader reader, ushort version)
    {
        short a = reader.ReadInt16(), b = reader.ReadInt16(), c = reader.ReadInt16(), d = reader.ReadInt16();
        Offset16<Mode7Pixels> center = ReadOffset<Mode7Pixels>(reader), scroll = ReadOffset<Mode7Pixels>(reader);
        var policy = (Mode7OverflowMode)reader.ReadByte();
        if (policy > Mode7OverflowMode.Wrap || (policy == Mode7OverflowMode.Wrap && version < RenderPacketFormat.Mode7WrapVersion))
            throw new InvalidDataException("Unsupported Mode 7 overflow operation in display fixture.");
        // The legacy boolean encoded exactly transparent=0 / character-zero=1.
        return new(a, b, c, d, center, scroll, policy == Mode7OverflowMode.CharacterZero, policy == Mode7OverflowMode.Wrap);
    }

    /// <summary>Reads unsigned X and Y 16-bit position components.</summary>
    private static Position16<TSpace> ReadPosition<TSpace>(BinaryReader reader) where TSpace : ISpatialSpace =>
        new(reader.ReadUInt16(), reader.ReadUInt16());

    /// <summary>Reads signed X and Y 16-bit offset components.</summary>
    private static Offset16<TSpace> ReadOffset<TSpace>(BinaryReader reader) where TSpace : ISpatialSpace =>
        new(reader.ReadInt16(), reader.ReadInt16());

    /// <summary>Reads tilemap dimensions from checked 32-bit packet fields.</summary>
    private static Extent16<TilemapCells> ReadMapSize(BinaryReader reader) =>
        new(checked((ushort)reader.ReadInt32()), checked((ushort)reader.ReadInt32()));

    /// <summary>Restores gameplay scroll, map, window, mosaic, and line-mask data supported by the packet version.</summary>
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

    /// <summary>Reads optional gameplay-height line-scroll words.</summary>
    private static ushort[] ReadScrolls(BinaryReader reader)
    {
        if (!ReadBoolean(reader)) return [];
        var values = new ushort[SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels];
        for (int i = 0; i < values.Length; i++) values[i] = reader.ReadUInt16();
        return values;
    }

    /// <summary>Dispatches a layer tag to its version-gated payload parser.</summary>
    private static RenderLayer ReadLayer(BinaryReader reader, ushort version, bool childScene = false) => (RenderPacketLayerKind)reader.ReadByte() switch
    {
        RenderPacketLayerKind.XrayGameplay when version >= RenderPacketFormat.XrayGameplayVersion => ReadXrayGameplay(reader, version),
        RenderPacketLayerKind.WindowedScene when version >= RenderPacketFormat.WindowedSceneLayerVersion && !childScene => ReadWindowedScene(reader, version),
        RenderPacketLayerKind.BgSubscreenAdd when version >= RenderPacketFormat.WindowedSceneLayerVersion => ReadSubscreen(reader, version),
        RenderPacketLayerKind.Bg4SubscreenAdd when version >= RenderPacketFormat.Bg4SubscreenAddVersion => ReadSubscreen(reader, version, true),
        RenderPacketLayerKind.ScrolledBg4SubscreenAdd when version >= RenderPacketFormat.ScrolledSubscreenVersion => ReadSubscreen(reader, version, true, true),
        RenderPacketLayerKind.Mode7Gameplay when version >= RenderPacketFormat.Mode7GameplayLayerVersion => ReadMode7Gameplay(reader, version),
        RenderPacketLayerKind.BgColorMath when version >= RenderPacketFormat.BgColorMathLayerVersion => ReadBgColorMath(reader),
        RenderPacketLayerKind.MessageBox when version >= RenderPacketFormat.MessageLayerVersion => ReadMessageLayer(reader),
        RenderPacketLayerKind.ScanlineColorAdd when version >= RenderPacketFormat.ScanlineColorLayerVersion =>
            ReadColorWindows(reader),
        RenderPacketLayerKind.OrdinaryGameplay when version >= RenderPacketFormat.OrdinaryGameplayLayerVersion =>
            ReadGameplayLayer(reader, version),
        RenderPacketLayerKind.Bg2Viewport when version >= RenderPacketFormat.Bg2ViewportLayerVersion =>
            new Bg2BppViewportRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(),
                ReadBoolean(reader), ReadPriority(reader)),
        RenderPacketLayerKind.FixedColorAdd when version >= RenderPacketFormat.FixedColorLayerVersion =>
            new FixedColorAddRenderLayer(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
        RenderPacketLayerKind.Mode7 when version >= RenderPacketFormat.Mode7LayerVersion => new Mode7RenderLayer(
            ReadMode7Registers(reader, version)),
        RenderPacketLayerKind.Mode7ObjSubtract when version >= RenderPacketFormat.Mode7ObjSubtractVersion => new Mode7RenderLayer(
            ReadMode7Registers(reader, version), true),
        RenderPacketLayerKind.Mode7Bg1Add when version >= RenderPacketFormat.Mode7Bg1AddVersion => new Mode7RenderLayer(
            ReadMode7Registers(reader, version), AddBg1Subscreen: true),
        RenderPacketLayerKind.Obj => ReadObjLayer(reader, version, false),
        RenderPacketLayerKind.ObjSubscreenAdd when version >= RenderPacketFormat.ObjSubscreenAddVersion => ReadObjLayer(reader, version, true),
        RenderPacketLayerKind.ObjPriority => ReadObjPriorityLayer(reader, version),
        RenderPacketLayerKind.Bg4Bpp => new Bg4BppRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(),
            ReadPosition<BgPlanePixels>(reader), ReadMapSize(reader), ReadPriority(reader)),
        RenderPacketLayerKind.Bg2Bpp => new Bg2BppRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadInt32(), ReadBoolean(reader)),
        _ => throw new InvalidDataException("Unknown layer kind in display fixture."),
    };

    /// <summary>Reads one OBJ priority pass and its optional fixed color.</summary>
    private static ObjPriorityRenderLayer ReadObjPriorityLayer(BinaryReader reader, ushort version)
    {
        byte priority = reader.ReadByte();
        FixedColorAddRenderLayer? color = null;
        if (version >= RenderPacketFormat.ObjPriorityFixedColorVersion && ReadBoolean(reader))
            color = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(priority, color);
    }

    /// <summary>Reads an OBJ composition pass with optional subscreen addition and fixed color.</summary>
    private static ObjRenderLayer ReadObjLayer(BinaryReader reader, ushort version, bool add)
    {
        FixedColorAddRenderLayer? fixedColor = null;
        if (version >= RenderPacketFormat.ObjFixedColorVersion && ReadBoolean(reader))
            fixedColor = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(add, fixedColor);
    }

    /// <summary>Restores X-ray windows, color math, and optional BG2 or BG3 subscreen data.</summary>
    private static GameplayColorMathRenderLayer ReadXrayGameplay(BinaryReader reader, ushort version)
    {
        var gameplay = ReadGameplayLayer(reader, version);
        var lines = new XrayWindowLine[Hardware.SnesPpuLayout.ScreenHeightPixels];
        for (int i = 0; i < lines.Length; i++) lines[i] = new(reader.ReadByte(), reader.ReadByte());
        bool reveal = ReadBoolean(reader);
        var control = (SnesColorMathControl)reader.ReadByte();
        bool addSubscreen = ReadBoolean(reader);
        byte red = reader.ReadByte(), green = reader.ReadByte(), blue = reader.ReadByte();
        Bg2BppColorMathRenderLayer? sub = null;
        if (ReadBoolean(reader))
        {
            if ((RenderPacketLayerKind)reader.ReadByte() != RenderPacketLayerKind.BgColorMath)
                throw new InvalidDataException("X-ray subscreen must be a BG3 plane.");
            sub = ReadBgColorMath(reader);
        }
        bool bg2 = version >= RenderPacketFormat.Bg2GameplaySubscreenVersion && ReadBoolean(reader);
        if (bg2 && sub is not null && version < RenderPacketFormat.CombinedGameplaySubscreenVersion)
            throw new InvalidDataException("Combined BG2/BG3 subscreen requires display fixture version 27.");
        return new(gameplay, lines, reveal, new SuperMetroid.Core.Rendering.ColorMathRegisters { ColorMath = control, AddSubscreen = addSubscreen, FixedRed = red, FixedGreen = green, FixedBlue = blue }, sub, bg2);
    }

    /// <summary>Reads a bounded nested scene with its own PPU memory and layer list.</summary>
    private static WindowedSceneRenderLayer ReadWindowedScene(BinaryReader reader, ushort version)
    {
        int left = reader.ReadInt32(), top = reader.ReadInt32(), right = reader.ReadInt32(), bottom = reader.ReadInt32();
        PpuMemorySnapshot memory = ReadMemory(reader);
        byte obsel = reader.ReadByte(), brightness = reader.ReadByte();
        var layers = new RenderLayer[ReadCount(reader)];
        for (int i = 0; i < layers.Length; i++) layers[i] = ReadLayer(reader, version, childScene: true);
        return new(new(memory, layers, obsel, brightness), left, top, right, bottom);
    }

    /// <summary>Reads a BG subscreen addition with optional coverage, OBJ participation, and per-line scroll.</summary>
    private static BgSubscreenAddRenderLayer ReadSubscreen(BinaryReader reader, ushort version, bool fourBpp = false, bool scrolled = false)
    {
        ushort map = reader.ReadUInt16(), characters = reader.ReadUInt16();
        Bg4BppRenderLayer? coverage = null;
        if (ReadBoolean(reader))
        {
            // Require the fixed-size BG4 descriptor before parsing any recursive data.
            if ((RenderPacketLayerKind)reader.ReadByte() != RenderPacketLayerKind.Bg4Bpp)
                throw new InvalidDataException("Subscreen coverage must be a BG4 descriptor.");
            coverage = new(reader.ReadUInt16(), reader.ReadUInt16(), ReadPosition<BgPlanePixels>(reader),
                ReadMapSize(reader), ReadPriority(reader));
        }
        var layer = new BgSubscreenAddRenderLayer(map, characters, coverage, fourBpp,
            version >= RenderPacketFormat.SubscreenObjectsVersion && ReadBoolean(reader),
            version >= RenderPacketFormat.SubscreenMainObjectsVersion && ReadBoolean(reader),
            scrolled ? reader.ReadUInt16() : (ushort)0);
        if (version >= RenderPacketFormat.SubscreenLineScrollVersion && ReadBoolean(reader))
        {
            var scrolls = new Position16<BgPlanePixels>[224];
            for (int line = 0; line < scrolls.Length; line++)
                scrolls[line] = ReadPosition<BgPlanePixels>(reader);
            layer = layer.WithScrolls(scrolls);
        }
        return layer;
    }

    /// <summary>Reads a two-bit BG color-math plane and its scanline scroll positions.</summary>
    private static Bg2BppColorMathRenderLayer ReadBgColorMath(BinaryReader reader)
    {
        ushort map = reader.ReadUInt16(), characters = reader.ReadUInt16();
        int height = reader.ReadInt32(), firstLine = reader.ReadInt32();
        var operation = (ExpandedColorMathOperation)reader.ReadByte();
        var lines = new Position16<BgPlanePixels>[Hardware.SnesPpuLayout.ScreenHeightPixels];
        for (int y = 0; y < lines.Length; y++) lines[y] = ReadPosition<BgPlanePixels>(reader);
        return new(map, characters, height, firstLine, operation, lines);
    }

    /// <summary>Validates message row count before reading its tilemap and corner radius.</summary>
    private static MessageBoxRenderLayer ReadMessageLayer(BinaryReader reader)
    {
        int rows = reader.ReadByte();
        int radius = reader.ReadByte();
        if (rows is < Game.GameplayMessageRomData.Layout.MinimumRows or > Game.GameplayMessageRomData.Layout.MaximumRows)
            throw new InvalidDataException("Invalid message row count in display fixture.");
        var tiles = new ushort[rows * Game.GameplayMessageRomData.Layout.TilemapWidth];
        for (int i = 0; i < tiles.Length; i++) tiles[i] = reader.ReadUInt16();
        return new(tiles, radius);
    }

    /// <summary>Reads one color-add window for every PPU scanline.</summary>
    private static ScanlineColorAddRenderLayer ReadColorWindows(BinaryReader reader)
    {
        var windows = new ColorAddWindow[Hardware.SnesPpuLayout.ScreenHeightPixels];
        for (int y = 0; y < windows.Length; y++)
            windows[y] = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(windows);
    }

    /// <summary>Decodes a tile-priority filter as all, low, or high.</summary>
    private static bool? ReadPriority(BinaryReader reader) => (RenderPacketPriority)reader.ReadByte() switch
    {
        RenderPacketPriority.All => null,
        RenderPacketPriority.Low => false,
        RenderPacketPriority.High => true,
        _ => throw new InvalidDataException("Unknown tile-priority selector in display fixture."),
    };

    /// <summary>Reads gameplay Mode 7 registers, HUD transfer positions, and optional floor band.</summary>
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
