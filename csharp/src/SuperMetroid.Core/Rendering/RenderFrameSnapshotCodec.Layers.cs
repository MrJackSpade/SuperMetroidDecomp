namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    private static void WriteLayer(BinaryWriter writer, RenderLayer layer)
    {
        switch (layer)
        {
            case XrayGameplayRenderLayer gameplayXray:
                writer.Write((byte)RenderPacketLayerKind.XrayGameplay);
                WriteGameplayLayer(writer, gameplayXray.Gameplay);
                foreach (XrayWindowLine line in gameplayXray.Lines) { writer.Write(line.Left); writer.Write(line.Right); }
                writer.Write(gameplayXray.RevealBlocks); writer.Write((byte)gameplayXray.ColorMath);
                writer.Write(gameplayXray.AddSubscreen);
                writer.Write(gameplayXray.FixedRed); writer.Write(gameplayXray.FixedGreen); writer.Write(gameplayXray.FixedBlue);
                writer.Write(gameplayXray.Subscreen is not null);
                if (gameplayXray.Subscreen is { } bg3) WriteLayer(writer, bg3);
                break;
            case XrayWindowRenderLayer xray:
                writer.Write((byte)RenderPacketLayerKind.XrayWindow);
                foreach (XrayWindowLine line in xray.Lines) { writer.Write(line.Left); writer.Write(line.Right); }
                WriteMemory(writer, xray.Reveal.Memory);
                writer.Write(xray.Reveal.ObjectSelection); writer.Write(xray.Reveal.Brightness);
                WriteCount(writer, xray.Reveal.Layers.Length);
                foreach (RenderLayer child in xray.Reveal.Layers) WriteLayer(writer, child);
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
                writer.Write((byte)(sub.FourBpp ? RenderPacketLayerKind.Bg4SubscreenAdd : RenderPacketLayerKind.BgSubscreenAdd));
                writer.Write(sub.TilemapWord); writer.Write(sub.CharacterWord); writer.Write(sub.MainCoverage is not null);
                if (sub.MainCoverage is { } coverage) WriteLayer(writer, coverage);
                break;
            case Mode7GameplayRenderLayer gameplay7:
                writer.Write((byte)RenderPacketLayerKind.Mode7Gameplay);
                WriteMode7Gameplay(writer, gameplay7);
                break;
            case Bg2BppColorMathRenderLayer bgMath:
                writer.Write((byte)RenderPacketLayerKind.BgColorMath);
                writer.Write(bgMath.TilemapWord); writer.Write(bgMath.CharacterWord);
                writer.Write(bgMath.MapHeightTiles); writer.Write(bgMath.FirstScanline); writer.Write((byte)bgMath.Operation);
                foreach (BackgroundLineScroll line in bgMath.Scrolls) { writer.Write(line.X); writer.Write(line.Y); }
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
                writer.Write((byte)(layer7.SubtractObjSubscreen ? RenderPacketLayerKind.Mode7ObjSubtract : RenderPacketLayerKind.Mode7));
                Mode7RenderRegisters m = layer7.Registers;
                writer.Write(m.MatrixA); writer.Write(m.MatrixB); writer.Write(m.MatrixC); writer.Write(m.MatrixD);
                writer.Write(m.CenterX); writer.Write(m.CenterY);
                writer.Write(m.HorizontalOffset); writer.Write(m.VerticalOffset);
                writer.Write(m.FillOutsideWithCharacterZero);
                break;
            case ObjRenderLayer objLayer:
                writer.Write((byte)(objLayer.AddToScreen ? RenderPacketLayerKind.ObjSubscreenAdd : RenderPacketLayerKind.Obj));
                break;
            case ObjPriorityRenderLayer obj:
                writer.Write((byte)RenderPacketLayerKind.ObjPriority);
                writer.Write(obj.Priority);
                break;
            case Bg4BppRenderLayer bg:
                writer.Write((byte)RenderPacketLayerKind.Bg4Bpp);
                writer.Write(bg.TilemapWord); writer.Write(bg.CharacterWord);
                writer.Write(bg.HorizontalScroll); writer.Write(bg.VerticalScroll);
                writer.Write(bg.MapWidthTiles); writer.Write(bg.MapHeightTiles);
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

    private static RenderLayer ReadLayer(BinaryReader reader, ushort version, bool childScene = false) => (RenderPacketLayerKind)reader.ReadByte() switch
    {
        RenderPacketLayerKind.XrayGameplay when version >= RenderPacketFormat.XrayGameplayVersion => ReadXrayGameplay(reader),
        RenderPacketLayerKind.WindowedScene when version >= RenderPacketFormat.WindowedSceneLayerVersion && !childScene => ReadWindowedScene(reader, version),
        RenderPacketLayerKind.XrayWindow when version >= RenderPacketFormat.XrayWindowVersion && !childScene => ReadXrayWindow(reader, version),
        RenderPacketLayerKind.BgSubscreenAdd when version >= RenderPacketFormat.WindowedSceneLayerVersion => ReadSubscreen(reader),
        RenderPacketLayerKind.Bg4SubscreenAdd when version >= RenderPacketFormat.Bg4SubscreenAddVersion => ReadSubscreen(reader, true),
        RenderPacketLayerKind.Mode7Gameplay when version >= RenderPacketFormat.Mode7GameplayLayerVersion => ReadMode7Gameplay(reader),
        RenderPacketLayerKind.BgColorMath when version >= RenderPacketFormat.BgColorMathLayerVersion => ReadBgColorMath(reader),
        RenderPacketLayerKind.MessageBox when version >= RenderPacketFormat.MessageLayerVersion => ReadMessageLayer(reader),
        RenderPacketLayerKind.ScanlineColorAdd when version >= RenderPacketFormat.ScanlineColorLayerVersion =>
            ReadColorWindows(reader),
        RenderPacketLayerKind.OrdinaryGameplay when version >= RenderPacketFormat.OrdinaryGameplayLayerVersion =>
            ReadGameplayLayer(reader),
        RenderPacketLayerKind.Bg2Viewport when version >= RenderPacketFormat.Bg2ViewportLayerVersion =>
            new Bg2BppViewportRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(),
                ReadBoolean(reader), ReadPriority(reader)),
        RenderPacketLayerKind.FixedColorAdd when version >= RenderPacketFormat.FixedColorLayerVersion =>
            new FixedColorAddRenderLayer(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
        RenderPacketLayerKind.Mode7 when version >= RenderPacketFormat.Mode7LayerVersion => new Mode7RenderLayer(
            new(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(),
                reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), ReadBoolean(reader))),
        RenderPacketLayerKind.Mode7ObjSubtract when version >= RenderPacketFormat.Mode7ObjSubtractVersion => new Mode7RenderLayer(
            new(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(),
                reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), ReadBoolean(reader)), true),
        RenderPacketLayerKind.Obj => new ObjRenderLayer(),
        RenderPacketLayerKind.ObjSubscreenAdd when version >= RenderPacketFormat.ObjSubscreenAddVersion => new ObjRenderLayer(true),
        RenderPacketLayerKind.ObjPriority => new ObjPriorityRenderLayer(reader.ReadByte()),
        RenderPacketLayerKind.Bg4Bpp => new Bg4BppRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadInt32(), reader.ReadInt32(), ReadPriority(reader)),
        RenderPacketLayerKind.Bg2Bpp => new Bg2BppRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadInt32(), ReadBoolean(reader)),
        _ => throw new InvalidDataException("Unknown layer kind in display fixture."),
    };

    private static XrayGameplayRenderLayer ReadXrayGameplay(BinaryReader reader)
    {
        var gameplay = ReadGameplayLayer(reader);
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
        return new(gameplay, lines, reveal, control, addSubscreen, red, green, blue, sub);
    }

    private static XrayWindowRenderLayer ReadXrayWindow(BinaryReader reader, ushort version)
    {
        var lines = new XrayWindowLine[Hardware.SnesPpuLayout.ScreenHeightPixels];
        for (int i = 0; i < lines.Length; i++) lines[i] = new(reader.ReadByte(), reader.ReadByte());
        PpuMemorySnapshot memory = ReadMemory(reader);
        byte obsel = reader.ReadByte(), brightness = reader.ReadByte();
        var layers = new RenderLayer[ReadCount(reader)];
        for (int i = 0; i < layers.Length; i++) layers[i] = ReadLayer(reader, version, childScene: true);
        return new(new(memory, layers, obsel, brightness), lines);
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

    private static BgSubscreenAddRenderLayer ReadSubscreen(BinaryReader reader, bool fourBpp = false)
    {
        ushort map = reader.ReadUInt16(), characters = reader.ReadUInt16();
        Bg4BppRenderLayer? coverage = null;
        if (ReadBoolean(reader))
        {
            // Require the fixed-size BG4 descriptor before parsing any recursive data.
            if ((RenderPacketLayerKind)reader.ReadByte() != RenderPacketLayerKind.Bg4Bpp)
                throw new InvalidDataException("Subscreen coverage must be a BG4 descriptor.");
            coverage = new(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(),
                reader.ReadInt32(), reader.ReadInt32(), ReadPriority(reader));
        }
        return new(map, characters, coverage, fourBpp);
    }

    private static Bg2BppColorMathRenderLayer ReadBgColorMath(BinaryReader reader)
    {
        ushort map = reader.ReadUInt16(), characters = reader.ReadUInt16();
        int height = reader.ReadInt32(), firstLine = reader.ReadInt32();
        var operation = (ExpandedColorMathOperation)reader.ReadByte();
        var lines = new BackgroundLineScroll[Hardware.SnesPpuLayout.ScreenHeightPixels];
        for (int y = 0; y < lines.Length; y++) lines[y] = new(reader.ReadUInt16(), reader.ReadUInt16());
        return new(map, characters, height, firstLine, operation, lines);
    }

    private static MessageBoxRenderLayer ReadMessageLayer(BinaryReader reader)
    {
        int rows = reader.ReadByte();
        int radius = reader.ReadByte();
        if (rows < Game.GameplayMessageRomData.Layout.MinimumRows || rows > Game.GameplayMessageRomData.Layout.MaximumRows)
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
