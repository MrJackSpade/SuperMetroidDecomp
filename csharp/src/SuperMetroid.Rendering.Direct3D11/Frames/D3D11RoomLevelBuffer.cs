using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Backgrounds;
using System.Collections.Immutable;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The frame's room backgrounds packed for Tiles.hlsl at register t3 (#1346): a header of three plane records (BG1, BG2
/// and a door's source room), the door window and the BG1 overlay offset; then each plane's level words, each distinct
/// set of block definitions as tilemap words, and the overlay's words. The GPU resolves every room tile from these, so
/// nothing is streamed into VRAM. Repacked only when the published planes, offsets, door or overlay change.
/// </summary>
internal sealed class D3D11RoomLevelBuffer : IDisposable
{
    /// <summary>Words per plane record: level-word offset, width, height, definition offset, scroll offset X and Y.</summary>
    private const int RecordWords = 6;
    /// <summary>Plane slots: BG1, BG2 and a door's source room.</summary>
    private const int PlaneSlots = 3;
    /// <summary>Header words: the plane records, the door window origin and the overlay offset.</summary>
    private const int HeaderWords = PlaneSlots * RecordWords + 2;
    /// <summary>Header word of the door window's scrolled origin.</summary>
    private const int DoorWindowWord = PlaneSlots * RecordWords;
    /// <summary>Header word of the overlay's first word, or zero without an overlay.</summary>
    private const int OverlayWord = DoorWindowWord + 1;
    /// <summary>Initial buffer size; it grows to fit larger rooms.</summary>
    private const int InitialWords = 4096;

    /// <summary>The device the buffer lives on.</summary>
    private readonly ID3D11Device device;
    /// <summary>What the last <see cref="Pack"/> packed, so an unchanged frame skips the upload.</summary>
    private (RoomBackgroundLayer? Bg1, RoomBackgroundLayer? Bg2, RoomDoorBackground? Door, RoomTilemapOverlay? Overlay)? packed;

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
    /// <param name="bg1">The room's BG1, or null when the scene shows none.</param>
    /// <param name="bg2">The room's layer-2 plane on BG2, or null.</param>
    /// <param name="door">A door's source room screen, or null.</param>
    /// <param name="overlay">Words written over BG1, or null.</param>
    internal bool Pack(RoomBackgroundLayer? bg1, RoomBackgroundLayer? bg2, RoomDoorBackground? door, RoomTilemapOverlay? overlay)
    {
        var frame = (bg1, bg2, door, overlay);
        // No dispatch samples t3 without a room plane, so a scene without one keeps the last packed room unread.
        if (packed == frame || (bg1 is null && bg2 is null))
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
        int overlayOffset = overlay is null ? 0 : words;
        if (overlay is not null)
            words += RoomTilemapOverlay.WordCount;
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
            ReadOnlySpan<ushort> blocks = plane.Blocks;
            for (int block = 0; block < blocks.Length; block++)
                upload[blockOffsets[slot] + block] = blocks[block];
        }
        foreach ((ImmutableArray<byte> definitions, int offset) in definitionWords)
            for (int word = 0; word < definitions.Length / sizeof(ushort); word++)
                upload[offset + word] = SnesLittleEndian.Word(definitions[word * 2], definitions[word * 2 + 1]);
        if (door is not null)
            upload[DoorWindowWord] = D3D11ConstantPacking.Halves(door.WindowOrigin.X, door.WindowOrigin.Y);
        upload[OverlayWord] = (uint)overlayOffset;
        if (overlay is not null)
            for (int word = 0; word < RoomTilemapOverlay.WordCount; word++)
                upload[overlayOffset + word] = unchecked((uint)overlay.Words[word]);
        packed = frame;
        return true;
    }

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
