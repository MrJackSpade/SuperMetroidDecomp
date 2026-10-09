namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    /// <summary>Writes a layer kind followed by the payload defined for that kind in the render-packet format.</summary>
    /// <param name="writer">Destination stream writer receiving the layer tag and payload.</param>
    /// <param name="layer">Render layer whose variant determines the encoded fields.</param>
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
                foreach (BackgroundLineScroll line in sub.Scrolls) { writer.Write(line.X); writer.Write(line.Y); }
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
                writer.Write((byte)(layer7.AddBg1Subscreen ? RenderPacketLayerKind.Mode7Bg1Add
                    : layer7.SubtractObjSubscreen ? RenderPacketLayerKind.Mode7ObjSubtract : RenderPacketLayerKind.Mode7));
                Mode7RenderRegisters m = layer7.Registers;
                writer.Write(m.MatrixA); writer.Write(m.MatrixB); writer.Write(m.MatrixC); writer.Write(m.MatrixD);
                writer.Write(m.CenterX); writer.Write(m.CenterY);
                writer.Write(m.HorizontalOffset); writer.Write(m.VerticalOffset);
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

    /// <summary>Reads a tagged layer payload using only fields supported by the packet version.</summary>
    /// <param name="reader">Source stream positioned at the layer-kind byte.</param>
    /// <param name="version">Render-packet version controlling optional layer fields.</param>
    /// <param name="childScene">Whether this layer is nested in a windowed scene, where another windowed scene is disallowed.</param>
    /// <returns>The decoded render-layer variant.</returns>
    /// <exception cref="InvalidDataException">The kind is unknown, unavailable in this version, or has an invalid nested descriptor.</exception>
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
            reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadInt32(), reader.ReadInt32(), ReadPriority(reader)),
        RenderPacketLayerKind.Bg2Bpp => new Bg2BppRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadInt32(), ReadBoolean(reader)),
        _ => throw new InvalidDataException("Unknown layer kind in display fixture."),
    };

    /// <summary>Reads object-priority selection and, in supported packet versions, its optional fixed-color addition.</summary>
    /// <param name="reader">Reader positioned after the object-priority kind tag.</param>
    /// <param name="version">Packet version determining whether fixed-color data is present.</param>
    /// <returns>The decoded object-priority layer.</returns>
    private static ObjPriorityRenderLayer ReadObjPriorityLayer(BinaryReader reader, ushort version)
    {
        byte priority = reader.ReadByte();
        FixedColorAddRenderLayer? color = null;
        if (version >= RenderPacketFormat.ObjPriorityFixedColorVersion && ReadBoolean(reader))
            color = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(priority, color);
    }

    /// <summary>Reads an object layer and its versioned optional fixed-color addition.</summary>
    /// <param name="reader">Reader positioned after the object-layer kind tag.</param>
    /// <param name="version">Packet version determining whether fixed-color data is present.</param>
    /// <param name="add">Whether the decoded objects are added through the subscreen path.</param>
    /// <returns>The decoded object layer.</returns>
    private static ObjRenderLayer ReadObjLayer(BinaryReader reader, ushort version, bool add)
    {
        FixedColorAddRenderLayer? fixedColor = null;
        if (version >= RenderPacketFormat.ObjFixedColorVersion && ReadBoolean(reader))
            fixedColor = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(add, fixedColor);
    }

    /// <summary>Reads the gameplay layer, scanline windows, color-math settings, and optional subscreens for X-ray rendering.</summary>
    /// <param name="reader">Reader positioned after the X-ray gameplay kind tag.</param>
    /// <param name="version">Packet version controlling combined and BG2 subscreen fields.</param>
    /// <returns>The reconstructed X-ray gameplay layer.</returns>
    /// <exception cref="InvalidDataException">The subscreen descriptor or its version-dependent combination is invalid.</exception>
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
        return new(gameplay, lines, reveal, control, addSubscreen, red, green, blue, sub, bg2);
    }

    /// <summary>Reads scene bounds, PPU snapshot data, and child layers for a clipped windowed scene.</summary>
    /// <param name="reader">Reader positioned after the windowed-scene kind tag.</param>
    /// <param name="version">Packet version used when decoding nested layers.</param>
    /// <returns>The windowed scene and its decoded scene payload.</returns>
    private static WindowedSceneRenderLayer ReadWindowedScene(BinaryReader reader, ushort version)
    {
        int left = reader.ReadInt32(), top = reader.ReadInt32(), right = reader.ReadInt32(), bottom = reader.ReadInt32();
        PpuMemorySnapshot memory = ReadMemory(reader);
        byte obsel = reader.ReadByte(), brightness = reader.ReadByte();
        var layers = new RenderLayer[ReadCount(reader)];
        for (int i = 0; i < layers.Length; i++) layers[i] = ReadLayer(reader, version, childScene: true);
        return new(new(memory, layers, obsel, brightness), left, top, right, bottom);
    }

    /// <summary>Reads a BG subscreen layer, including optional coverage, object data, vertical scroll, and per-line scroll.</summary>
    /// <param name="reader">Reader positioned after the subscreen kind tag.</param>
    /// <param name="version">Packet version controlling optional subscreen fields.</param>
    /// <param name="fourBpp">Whether coverage and tiles use the BG4 variant.</param>
    /// <param name="scrolled">Whether this variant carries an additional vertical-scroll word.</param>
    /// <returns>The decoded subscreen layer.</returns>
    /// <exception cref="InvalidDataException">The optional coverage descriptor is not a BG4 layer.</exception>
    private static BgSubscreenAddRenderLayer ReadSubscreen(BinaryReader reader, ushort version, bool fourBpp = false, bool scrolled = false)
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
        var layer = new BgSubscreenAddRenderLayer(map, characters, coverage, fourBpp,
            version >= RenderPacketFormat.SubscreenObjectsVersion && ReadBoolean(reader),
            version >= RenderPacketFormat.SubscreenMainObjectsVersion && ReadBoolean(reader),
            scrolled ? reader.ReadUInt16() : (ushort)0);
        if (version >= RenderPacketFormat.SubscreenLineScrollVersion && ReadBoolean(reader))
        {
            var scrolls = new BackgroundLineScroll[224];
            for (int line = 0; line < scrolls.Length; line++)
                scrolls[line] = new(reader.ReadUInt16(), reader.ReadUInt16());
            layer = layer.WithScrolls(scrolls);
        }
        return layer;
    }

    /// <summary>Reads a BG2 color-math plane and its scanline scroll table.</summary>
    /// <param name="reader">Reader positioned after the BG color-math kind tag.</param>
    /// <returns>The reconstructed BG2 color-math layer.</returns>
    private static Bg2BppColorMathRenderLayer ReadBgColorMath(BinaryReader reader)
    {
        ushort map = reader.ReadUInt16(), characters = reader.ReadUInt16();
        int height = reader.ReadInt32(), firstLine = reader.ReadInt32();
        var operation = (ExpandedColorMathOperation)reader.ReadByte();
        var lines = new BackgroundLineScroll[Hardware.SnesPpuLayout.ScreenHeightPixels];
        for (int y = 0; y < lines.Length; y++) lines[y] = new(reader.ReadUInt16(), reader.ReadUInt16());
        return new(map, characters, height, firstLine, operation, lines);
    }

    /// <summary>Reads a message-box tilemap after validating its row count against the gameplay layout.</summary>
    /// <param name="reader">Reader positioned after the message-layer kind tag.</param>
    /// <returns>The message box with its tilemap and corner radius.</returns>
    /// <exception cref="InvalidDataException">The encoded row count is outside the supported layout.</exception>
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

    /// <summary>Reads the per-scanline color-add window boundaries and RGB values.</summary>
    /// <param name="reader">Reader positioned after the scanline-color kind tag.</param>
    /// <returns>The color-add layer containing one window record per screen line.</returns>
    private static ScanlineColorAddRenderLayer ReadColorWindows(BinaryReader reader)
    {
        var windows = new ColorAddWindow[Hardware.SnesPpuLayout.ScreenHeightPixels];
        for (int y = 0; y < windows.Length; y++)
            windows[y] = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(windows);
    }

    /// <summary>Decodes the packet's tile-priority selector into an unrestricted, low-only, or high-only setting.</summary>
    /// <param name="reader">Reader positioned at the priority selector byte.</param>
    /// <returns><see langword="null"/> for all priorities, otherwise <see langword="false"/> for low or <see langword="true"/> for high.</returns>
    /// <exception cref="InvalidDataException">The selector byte is not a defined priority value.</exception>
    private static bool? ReadPriority(BinaryReader reader) => (RenderPacketPriority)reader.ReadByte() switch
    {
        RenderPacketPriority.All => null,
        RenderPacketPriority.Low => false,
        RenderPacketPriority.High => true,
        _ => throw new InvalidDataException("Unknown tile-priority selector in display fixture."),
    };
}
