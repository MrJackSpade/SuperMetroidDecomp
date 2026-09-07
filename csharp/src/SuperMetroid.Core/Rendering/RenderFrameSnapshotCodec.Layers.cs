namespace SuperMetroid.Core.Rendering;

public static partial class RenderFrameSnapshotCodec
{
    private static void WriteLayer(BinaryWriter writer, RenderLayer layer)
    {
        switch (layer)
        {
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
                writer.Write((byte)RenderPacketLayerKind.Mode7);
                Mode7RenderRegisters m = layer7.Registers;
                writer.Write(m.MatrixA); writer.Write(m.MatrixB); writer.Write(m.MatrixC); writer.Write(m.MatrixD);
                writer.Write(m.CenterX); writer.Write(m.CenterY);
                writer.Write(m.HorizontalOffset); writer.Write(m.VerticalOffset);
                writer.Write(m.FillOutsideWithCharacterZero);
                break;
            case ObjRenderLayer:
                writer.Write((byte)RenderPacketLayerKind.Obj);
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

    private static RenderLayer ReadLayer(BinaryReader reader, ushort version) => (RenderPacketLayerKind)reader.ReadByte() switch
    {
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
        RenderPacketLayerKind.Obj => new ObjRenderLayer(),
        RenderPacketLayerKind.ObjPriority => new ObjPriorityRenderLayer(reader.ReadByte()),
        RenderPacketLayerKind.Bg4Bpp => new Bg4BppRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadInt32(), reader.ReadInt32(), ReadPriority(reader)),
        RenderPacketLayerKind.Bg2Bpp => new Bg2BppRenderLayer(reader.ReadUInt16(), reader.ReadUInt16(),
            reader.ReadInt32(), ReadBoolean(reader)),
        _ => throw new InvalidDataException("Unknown layer kind in display fixture."),
    };

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
