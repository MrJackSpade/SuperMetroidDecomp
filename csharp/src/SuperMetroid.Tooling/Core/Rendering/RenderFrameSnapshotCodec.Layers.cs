using SuperMetroid.Core.Spatial;
namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
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

    private static ObjPriorityRenderLayer ReadObjPriorityLayer(BinaryReader reader, ushort version)
    {
        byte priority = reader.ReadByte();
        FixedColorAddRenderLayer? color = null;
        if (version >= RenderPacketFormat.ObjPriorityFixedColorVersion && ReadBoolean(reader))
            color = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(priority, color);
    }

    private static ObjRenderLayer ReadObjLayer(BinaryReader reader, ushort version, bool add)
    {
        FixedColorAddRenderLayer? fixedColor = null;
        if (version >= RenderPacketFormat.ObjFixedColorVersion && ReadBoolean(reader))
            fixedColor = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(add, fixedColor);
    }

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

    private static WindowedSceneRenderLayer ReadWindowedScene(BinaryReader reader, ushort version)
    {
        int left = reader.ReadInt32(), top = reader.ReadInt32(), right = reader.ReadInt32(), bottom = reader.ReadInt32();
        PpuMemorySnapshot memory = ReadMemory(reader);
        byte obsel = reader.ReadByte(), brightness = reader.ReadByte();
        var layers = new RenderLayer[ReadCount(reader)];
        for (int i = 0; i < layers.Length; i++) layers[i] = ReadLayer(reader, version, childScene: true);
        return new(new(memory, layers, obsel, brightness), left, top, right, bottom);
    }

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

    private static Bg2BppColorMathRenderLayer ReadBgColorMath(BinaryReader reader)
    {
        ushort map = reader.ReadUInt16(), characters = reader.ReadUInt16();
        int height = reader.ReadInt32(), firstLine = reader.ReadInt32();
        var operation = (ExpandedColorMathOperation)reader.ReadByte();
        var lines = new Position16<BgPlanePixels>[Hardware.SnesPpuLayout.ScreenHeightPixels];
        for (int y = 0; y < lines.Length; y++) lines[y] = ReadPosition<BgPlanePixels>(reader);
        return new(map, characters, height, firstLine, operation, lines);
    }

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

    private static ScanlineColorAddRenderLayer ReadColorWindows(BinaryReader reader)
    {
        var windows = new ColorAddWindow[Hardware.SnesPpuLayout.ScreenHeightPixels];
        for (int y = 0; y < windows.Length; y++)
            windows[y] = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(windows);
    }

    private static bool? ReadPriority(BinaryReader reader) => (RenderPacketPriority)reader.ReadByte() switch
    {
        RenderPacketPriority.All => null,
        RenderPacketPriority.Low => false,
        RenderPacketPriority.High => true,
        _ => throw new InvalidDataException("Unknown tile-priority selector in display fixture."),
    };
}
