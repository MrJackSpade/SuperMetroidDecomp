using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;
using System.Collections.Immutable;
using Vortice.Direct3D11;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Fx;
using System.Buffers.Binary;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The frame's room backgrounds packed for Tiles.hlsl at register t3 (#1346): a header of three plane records (BG1, BG2
/// and a door's source room), the door window, the BG1 overlay, sky, BG3 and BG2 overlay offsets; then each plane's level
/// words, each distinct set of block definitions as tilemap words, each plane's picture of each definition and the
/// pictures themselves with their sheets' standard colours (#1392), the overlay's words, the sky's rows (#1347), the
/// BG3 plane and the BG2 overlay's words (#1348), a Ceres Mode 7 map (#1349) and a boss body's BG2 tilemap (#1352).
/// The GPU resolves every room pixel from these, so nothing is streamed into VRAM. Repacked only when the published
/// planes, their pictures, offsets, door or overlay change.
/// </summary>
internal sealed class D3D11RoomLevelBuffer : IDisposable
{
    /// <summary>
    /// Words per plane record: level-word offset, width, height, definition offset, scroll offset X and Y, and the offset
    /// of the picture each definition shows, or zero for a plane drawn from characters.
    /// </summary>
    private const int RecordWords = 7;

    /// <summary>Words per packed block picture: its sheet's colours' offset, its colours, then its palette indices and priorities.</summary>
    private const int PictureWords = 1 + 2 * RoomBlockSheet.BlockPixels * RoomBlockSheet.BlockPixels;
    /// <summary>Plane slots: BG1, BG2 and a door's source room.</summary>
    private const int PlaneSlots = 3;
    /// <summary>Header words: the plane records, the door window origin, and the offsets of the BG1 overlay, the sky rows,
    /// the BG3 plane and the BG2 overlay.</summary>
    private const int HeaderWords = PlaneSlots * RecordWords + 8;
    /// <summary>Header word of the door window's scrolled origin.</summary>
    private const int DoorWindowWord = PlaneSlots * RecordWords;
    /// <summary>Header word of the overlay's first word, or zero without an overlay.</summary>
    private const int OverlayWord = DoorWindowWord + 1;
    /// <summary>Header word of the sky's first row word, or zero without a sky (#1347).</summary>
    private const int SkyWord = OverlayWord + 1;
    /// <summary>Header word of the BG3 plane's first word, or zero without one (#1348).</summary>
    private const int Bg3Word = SkyWord + 1;
    /// <summary>Header word of the BG2 overlay's first word, or zero without one (#1348).</summary>
    private const int Bg2OverlayWord = Bg3Word + 1;
    /// <summary>Header word of the Mode 7 map's first word, or zero without one (#1349); four tile numbers per word.</summary>
    private const int Mode7MapWord = Bg2OverlayWord + 1;
    /// <summary>Header word of a boss body's BG2 tilemap's first word, or zero without one (#1352).</summary>
    private const int BossBg2Word = Mode7MapWord + 1;
    /// <summary>
    /// Header word of the X-ray reveal's first block, or zero without one (#1392): each block its picture's offset, with its
    /// arrangement in the top four bits.
    /// </summary>
    private const int XrayRevealWord = BossBg2Word + 1;
    /// <summary>The bit an X-ray reveal block's arrangement starts at.</summary>
    private const int ArrangementShift = 28;
    /// <summary>A sky row word that selects no page: the row shows nothing.</summary>
    private const uint SkyRowHidden = uint.MaxValue;
    /// <summary>Initial buffer size; it grows to fit larger rooms.</summary>
    private const int InitialWords = 4096;

    /// <summary>The device the buffer lives on.</summary>
    private readonly ID3D11Device device;
    /// <summary>What the last <see cref="Pack"/> packed, so an unchanged frame skips the upload.</summary>
    private RoomLevelLayers? packed;

    /// <summary>Creates an empty buffer.</summary>
    internal D3D11RoomLevelBuffer(ID3D11Device device)
    {
        this.device = Ensure.NotNull(device);
        Create(InitialWords);
    }

    /// <summary>The packed words of the last <see cref="Pack"/>.</summary>
    internal uint[] Upload { get; private set; }
    /// <summary>The structured buffer.</summary>
    internal ID3D11Buffer Buffer { get; private set; }
    /// <summary>The shader view bound at t3.</summary>
    internal ID3D11ShaderResourceView View { get; private set; }

    /// <summary>Packs the frame's room planes; false when they match the last packing and need no upload.</summary>
    /// <param name="frame">The planes, overlays, BG3, Mode 7 map, boss tilemap and X-ray reveal the frame shows.</param>
    internal bool Pack(RoomLevelLayers frame)
    {
        RoomBackgroundLayer? bg1 = frame.Bg1, bg2 = frame.Bg2;
        RoomDoorBackground? door = frame.Door;
        RoomTilemapOverlay? overlay = frame.Overlay, bg2Overlay = frame.Bg2Overlay;
        SkyTilemap? sky = frame.Sky;
        Layer3Plane? bg3 = frame.Bg3;
        Mode7MapSnapshot? mode7 = frame.Mode7;
        BossBg2Tilemap? bossBg2 = frame.BossBg2;
        XrayRevealPlane? reveal = frame.XrayReveal;
        // No dispatch samples t3 without one of these, so such a scene keeps the last packed room unread.
        if (packed == frame || (bg1 is null && bg2 is null && sky is null && bg3 is null && mode7 is null && reveal is null))
            return false;
        RoomBackgroundLayer?[] planes = [bg1, bg2, door?.SourceRoom];
        var definitionWords = new Dictionary<ImmutableArray<byte>, int>();
        int words = HeaderWords;
        var blockOffsets = new int[PlaneSlots];
        for (int slot = 0; slot < PlaneSlots; slot++)
        {
            if (planes[slot] is not { } layer)
                continue;
            blockOffsets[slot] = words;
            words += layer.Plane.Blocks.Length;
        }
        foreach (RoomBackgroundLayer? layer in planes)
            if (layer is not null && definitionWords.TryAdd(layer.Plane.Definitions, words))
                words += layer.Plane.Definitions.Length / sizeof(ushort);
        // Each plane's picture of each definition, then each distinct picture and sheet palette once (#1392).
        var pictureTables = new int[PlaneSlots];
        for (int slot = 0; slot < PlaneSlots; slot++)
        {
            if (planes[slot]?.Pictures is not { } table)
                continue;
            pictureTables[slot] = words;
            words += table.Definitions;
        }
        var pictureWords = new Dictionary<SpriteSheetFrame, int>();
        var colourWords = new Dictionary<SpriteImage, int>();
        int revealOffset = reveal is null ? 0 : words;
        if (reveal is not null)
            words += reveal.Blocks.Length;
        foreach (RoomBlockPictureTable? table in planes.Select(layer => layer?.Pictures).Append(reveal?.Pictures))
        {
            if (table is null)
                continue;
            for (int definition = 0; definition < table.Definitions; definition++)
            {
                if (table.Picture(definition) is not { } picture)
                    continue;
                if (colourWords.TryAdd(picture.Sheet, words))
                    words += SpriteImage.SpriteColourCount;
                if (pictureWords.TryAdd(picture, words))
                    words += PictureWords;
            }
        }
        int overlayOffset = overlay is null ? 0 : words;
        if (overlay is not null)
            words += RoomTilemapOverlay.WordCount;
        int skyOffset = sky is null ? 0 : words;
        if (sky is not null)
            words += SkyTilemap.Rows * SkyTilemap.Columns;
        int bg3Offset = bg3 is null ? 0 : words;
        if (bg3 is not null)
            words += Layer3Plane.Rows * Layer3Plane.Columns;
        int bg2OverlayOffset = bg2Overlay is null ? 0 : words;
        if (bg2Overlay is not null)
            words += RoomTilemapOverlay.WordCount;
        int mode7Offset = mode7 is null ? 0 : words;
        if (mode7 is not null)
            words += Mode7Map.Cells / sizeof(uint);
        int bossBg2Offset = bossBg2 is null ? 0 : words;
        if (bossBg2 is not null)
            words += bossBg2.Words.Length;
        if (words > Upload.Length)
        {
            Dispose();
            Create(Math.Max(words, Upload.Length * 2));
        }

        Span<uint> upload = Upload;
        upload.Clear();
        for (int slot = 0; slot < PlaneSlots; slot++)
        {
            if (planes[slot] is not { } layer)
                continue;
            int record = slot * RecordWords;
            RoomLevelPlane plane = layer.Plane;
            upload[record] = (uint)blockOffsets[slot];
            upload[record + 1] = plane.Size.X;
            upload[record + 2] = plane.Size.Y;
            upload[record + 3] = (uint)definitionWords[plane.Definitions];
            upload[record + 4] = unchecked((ushort)layer.ScrollOffset.X);
            upload[record + 5] = unchecked((ushort)layer.ScrollOffset.Y);
            upload[record + 6] = (uint)pictureTables[slot];
            if (layer.Pictures is { } table)
                for (int definition = 0; definition < table.Definitions; definition++)
                    upload[pictureTables[slot] + definition] = table.Picture(definition) is { } shown ? (uint)pictureWords[shown] : 0;
            ReadOnlySpan<ushort> blocks = plane.Blocks;
            for (int block = 0; block < blocks.Length; block++)
                upload[blockOffsets[slot] + block] = blocks[block];
        }
        foreach ((ImmutableArray<byte> definitions, int offset) in definitionWords)
            for (int word = 0; word < definitions.Length / sizeof(ushort); word++)
                upload[offset + word] = SnesLittleEndian.Word(definitions[word * 2], definitions[word * 2 + 1]);
        foreach ((SpriteImage sheet, int offset) in colourWords)
            for (int colour = 0; colour < SpriteImage.SpriteColourCount; colour++)
                upload[offset + colour] = sheet.StandardColours[colour].ToWord();
        foreach ((SpriteSheetFrame picture, int offset) in pictureWords)
            PackPicture(upload.Slice(offset, PictureWords), picture, colourWords[picture.Sheet]);
        if (door is not null)
            upload[DoorWindowWord] = D3D11ConstantPacking.Halves(door.WindowOrigin.X, door.WindowOrigin.Y);
        upload[OverlayWord] = (uint)overlayOffset;
        if (overlay is not null)
            for (int word = 0; word < RoomTilemapOverlay.WordCount; word++)
                upload[overlayOffset + word] = unchecked((uint)overlay.Words[word]);
        upload[SkyWord] = (uint)skyOffset;
        if (sky is not null)
            for (int word = 0; word < SkyTilemap.Rows * SkyTilemap.Columns; word++)
                upload[skyOffset + word] = sky.RowShown(word / SkyTilemap.Columns) ? sky.Words[word] : SkyRowHidden;
        upload[Bg3Word] = (uint)bg3Offset;
        if (bg3 is not null)
            for (int word = 0; word < Layer3Plane.Rows * Layer3Plane.Columns; word++)
                upload[bg3Offset + word] = bg3.Words[word];
        upload[Bg2OverlayWord] = (uint)bg2OverlayOffset;
        upload[Mode7MapWord] = (uint)mode7Offset;
        if (mode7 is not null)
        {
            ReadOnlySpan<byte> cells = mode7.Cells;
            for (int word = 0; word < Mode7Map.Cells / sizeof(uint); word++)
                upload[mode7Offset + word] = BinaryPrimitives.ReadUInt32LittleEndian(cells[(word * sizeof(uint))..]);
        }
        if (bg2Overlay is not null)
            for (int word = 0; word < RoomTilemapOverlay.WordCount; word++)
                upload[bg2OverlayOffset + word] = unchecked((uint)bg2Overlay.Words[word]);
        upload[XrayRevealWord] = (uint)revealOffset;
        if (reveal is not null)
        {
            ReadOnlySpan<RevealedBlock> blocks = reveal.Blocks;
            for (int block = 0; block < blocks.Length; block++)
            {
                SpriteSheetFrame picture = reveal.Pictures.Picture(blocks[block].Definition)
                    ?? throw new InvalidOperationException($"The X-ray reveal's block definition {blocks[block].Definition} has no picture.");
                upload[revealOffset + block] = (uint)pictureWords[picture] | ShaderArrangement(blocks[block].Arrangement) << ArrangementShift;
            }
        }
        upload[BossBg2Word] = (uint)bossBg2Offset;
        if (bossBg2 is not null)
            for (int word = 0; word < bossBg2.Words.Length; word++)
                upload[bossBg2Offset + word] = bossBg2.Words[word];
        packed = frame;
        return true;
    }

    /// <summary>
    /// Packs one block picture: its sheet's colours' offset, then each pixel's colour as Tiles.hlsl packs one, then each
    /// pixel's palette index with its tile priority above it.
    /// </summary>
    private static void PackPicture(Span<uint> words, SpriteSheetFrame picture, int colours)
    {
        SpriteImage sheet = picture.Sheet;
        int pixels = RoomBlockSheet.BlockPixels * RoomBlockSheet.BlockPixels;
        words[0] = (uint)colours;
        for (int y = 0; y < RoomBlockSheet.BlockPixels; y++)
        {
            for (int x = 0; x < RoomBlockSheet.BlockPixels; x++)
            {
                int pixel = (picture.Corner.Y + y) * sheet.Size.X + picture.Corner.X + x;
                Rgba32 colour = sheet.Pixels[pixel];
                int at = y * RoomBlockSheet.BlockPixels + x;
                words[1 + at] = colour.R | (uint)colour.G << 8 | (uint)colour.B << 16 | (uint)colour.A << 24;
                words[1 + pixels + at] = sheet.PaletteIndices[pixel] | (uint)sheet.Priorities[pixel] << 8;
            }
        }
    }

    /// <summary>The code Tiles.hlsl's RevealPicture reads for a revealed block's arrangement.</summary>
    private static uint ShaderArrangement(RevealedBlockArrangement arrangement) => arrangement switch
    {
        RevealedBlockArrangement.AsDefined => 0,
        RevealedBlockArrangement.MirroredHorizontally => 1,
        RevealedBlockArrangement.MirroredVertically => 2,
        RevealedBlockArrangement.MirroredBoth => 3,
        RevealedBlockArrangement.RowsSwapped => 4,
        _ => throw new InvalidDataException($"Unknown revealed block arrangement {arrangement}."),
    };

    /// <summary>Creates the buffer and its view at <paramref name="words"/> words.</summary>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(Buffer), nameof(View), nameof(Upload))]
    private void Create(int words)
    {
        Buffer = device.CreateBuffer(new BufferDescription(checked((uint)(words * sizeof(uint))), BindFlags.ShaderResource,
            ResourceUsage.Default, CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, sizeof(uint)));
        View = device.CreateShaderResourceView(Buffer);
        Upload = new uint[words];
    }

    /// <summary>Releases the buffer and its view.</summary>
    public void Dispose()
    {
        View.Dispose();
        Buffer.Dispose();
    }
}
