using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable two-bit characters for simple room-FX and Wrecked Ship treadmill frames.</summary>
public sealed class RoomFxAnimatedTileAtlas : IInstalledArtworkTransferSource
{
    private readonly byte[] transfer;

    private RoomFxAnimatedTileAtlas(byte[] transfer)
    {
        this.transfer = transfer;
        int offset = 0;
        foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFormat.Segments)
            offset += segment.ByteCount;
        if (offset != transfer.Length)
            throw new InvalidDataException(
                $"Room-FX artwork has {transfer.Length} bytes, expected {offset}.");
    }

    /// <summary>Compiles indexed pixels back into their ordered native 2-bpp DMA frames.</summary>
    public static RoomFxAnimatedTileAtlas Load(Stream png,
        RoomFxAnimatedTileAtlas? stockForLegacyOverride = null)
    {
        ArgumentNullException.ThrowIfNull(png);
        long start = png.CanSeek ? png.Position : 0;
        try
        {
            IndexedPngImage image = IndexedPng.Read(png,
                RoomFxAnimatedTileAtlasFormat.Width, RoomFxAnimatedTileAtlasFormat.Height);
            return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height,
                RoomFxAnimatedTileAtlasFormat.BitsPerPixel));
        }
        catch (InvalidDataException) when (stockForLegacyOverride is not null && png.CanSeek)
        {
            // Preserve previous user-edited PNGs from before the treadmill,
            // statue, spores or spike extension; inherit only the newly introduced tail.
            return TryLegacy(RoomFxAnimatedTileAtlasFormat.PreSpikesWidth)
                ?? TryLegacy(RoomFxAnimatedTileAtlasFormat.PreSporesWidth)
                ?? TryLegacy(RoomFxAnimatedTileAtlasFormat.PreStatueWidth)
                ?? TryLegacy(RoomFxAnimatedTileAtlasFormat.LegacyWidth)
                ?? throw new InvalidDataException("Room-FX PNG matches neither current nor supported legacy sheet geometry.");

            RoomFxAnimatedTileAtlas? TryLegacy(int width)
            {
                png.Position = start;
                try
                {
                    IndexedPngImage legacy = IndexedPng.Read(png, width,
                        RoomFxAnimatedTileAtlasFormat.Height);
                    byte[] legacyPlanar = SnesPlanarTileEncoder.Encode(legacy.Pixels,
                        legacy.Width, legacy.Height, RoomFxAnimatedTileAtlasFormat.BitsPerPixel);
                    var combined = new byte[RoomFxAnimatedTileAtlasFormat.TotalByteCount];
                    legacyPlanar.CopyTo(combined, 0);
                    stockForLegacyOverride.transfer.AsSpan(legacyPlanar.Length).CopyTo(
                        combined.AsSpan(legacyPlanar.Length));
                    return new(combined);
                }
                catch (InvalidDataException) { return null; }
            }
        }
    }

    /// <summary>Returns one complete native frame, rejecting altered transfer geometry.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        int frameOffset = 0;
        foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFormat.Segments)
        {
            if (segment.IsFrame && segment.SourceAddress == sourceAddress)
            {
                if (byteCount != segment.ByteCount)
                    throw new InvalidDataException(
                        $"Room-FX artwork source ${sourceAddress:X6} requires {segment.ByteCount} bytes, not {byteCount}.");
                data = transfer.AsMemory(frameOffset, byteCount);
                return true;
            }
            frameOffset += segment.ByteCount;
        }
        // Statue operands name overlapping windows of the shared strip.
        foreach (TourianStatueAnimatedTileProgramDefinition definition in
                 TourianStatueAnimatedTileMechanicsDefinitions.All)
        foreach (ushort operand in definition.SourceOperandPointers)
        {
            if (TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(definition,
                    operand) != sourceAddress)
                continue;
            if (byteCount != definition.TransferByteCount)
                throw new InvalidDataException(
                    $"Tourian statue art ${sourceAddress:X6} requires {definition.TransferByteCount} bytes, not {byteCount}.");
            int offset = RoomFxAnimatedTileAtlasFormat.PreStatueTileCount * 16 +
                sourceAddress - TourianStatueAnimatedTileArtworkDefinitions.FirstSource;
            data = transfer.AsMemory(offset, byteCount);
            return true;
        }
        data = default;
        return false;
    }

    /// <summary>Publishes a selected installed frame through the synchronous liquid/rain owner.</summary>
    public void LoadFrame(SnesVram vram, int sourceAddress, ushort byteCount,
        ushort encodedVramDestination)
    {
        ArgumentNullException.ThrowIfNull(vram);
        if (!TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> data))
            throw new InvalidDataException(
                $"Room-FX frame ${sourceAddress:X6} is missing from installed artwork.");
        vram.ExecuteQueuedAssetWrite(data.Span, encodedVramDestination);
    }
}

/// <summary>One ordered strip of native 2-bpp room-FX, treadmill, and statue characters.</summary>
public static class RoomFxAnimatedTileAtlasFormat
{
    /// <summary>Installed indexed PNG filename for the append-only strip of room-FX and related animated characters.</summary>
    public const string FileName = "room-fx-animated-tiles.png";
    /// <summary>Two native planar bitplanes per pixel, producing sixteen bytes for each 8-by-8 character.</summary>
    public const int BitsPerPixel = 2;
    /// <summary>Four palette-index values representable by the two-bit artwork; visible palette colors remain separately owned.</summary>
    public const int ColorCount = 4;
    /// <summary>89 characters in the original simple room-FX prefix, before treadmill, statue, spore, and spike extensions.</summary>
    public const int LegacyTileCount = 89;
    /// <summary>Four native Wrecked Ship treadmill images beginning at $87:8E64, each containing two 2bpp characters; direction reverses their runtime order.</summary>
    public const int TreadmillFrameCount = 4;
    /// <summary>Character count of the historical prefix after its eight treadmill characters, before the shared Tourian-statue strip.</summary>
    public const int PreStatueTileCount = LegacyTileCount + TreadmillFrameCount * 2;
    /// <summary>Character count in the contiguous Tourian-statue source strip beginning at $87:9364; runtime frame operands select overlapping windows within it.</summary>
    public const int StatueTileCount = TourianStatueAnimatedTileArtworkDefinitions.TransferByteCount / 16;
    /// <summary>Character count of the historical prefix including the statue strip but preceding the spore animation extension.</summary>
    public const int PreSporesTileCount = PreStatueTileCount + StatueTileCount;
    /// <summary>Nine 2bpp characters across the native spore animation images beginning at $87:A7E4.</summary>
    public const int SporesTileCount = 9;
    /// <summary>Character count of the historical prefix including spores but preceding horizontal-spike artwork.</summary>
    public const int PreSpikesTileCount = PreSporesTileCount + SporesTileCount;
    /// <summary>24 characters in the three distinct horizontal-spike images beginning at $87:9D84; the fourth animation step reuses the second image rather than adding tiles.</summary>
    public const int SpikeTileCount = 24;
    /// <summary>Total 8-by-8 characters in the current ordered strip, including all historical prefixes and the spike extension.</summary>
    public const int TileCount = PreSpikesTileCount + SpikeTileCount;
    /// <summary>Legacy PNG width in pixels for the original 89-character prefix, admitted only with a seekable stream and current-stock tail fallback.</summary>
    public const int LegacyWidth = LegacyTileCount * 8;
    /// <summary>Legacy PNG width in pixels for the simple-FX/treadmill prefix, whose missing statue and later characters are inherited from current stock.</summary>
    public const int PreStatueWidth = PreStatueTileCount * 8;
    /// <summary>Legacy PNG width in pixels for the prefix through statue characters, whose missing spore and spike tail is inherited from current stock.</summary>
    public const int PreSporesWidth = PreSporesTileCount * 8;
    /// <summary>Legacy PNG width in pixels for the prefix through spore characters, whose missing spike tail is inherited from current stock.</summary>
    public const int PreSpikesWidth = PreSpikesTileCount * 8;
    /// <summary>Required current PNG width in pixels, with every character placed horizontally in stable transfer-segment order.</summary>
    public const int Width = TileCount * 8;
    /// <summary>Required PNG height in pixels: one 8-pixel character row.</summary>
    public const int Height = 8;
    /// <summary>Total compiled planar transfer bytes, sixteen per character, including the shared statue strip rather than duplicating its overlapping frames.</summary>
    public const int TotalByteCount = TileCount * 16;

    /// <summary>
    /// Stable PNG strip order shared by importer and runtime: existing simple frames,
    /// treadmills, the statue strip, spores, then spikes. Never insert new frames
    /// into the historical prefix or older replacements would shift unrelated art.
    /// </summary>
    public static IEnumerable<RoomFxAtlasSegment> Segments => EnumerateSegments();

    private static IEnumerable<RoomFxAtlasSegment> EnumerateSegments()
    {
        int total = 0;
        foreach (RoomFxAnimatedTileObjectDefinition definition in RoomFxAnimatedTileMechanicsDefinitions.All)
            if (definition.ObjectPointer is not (AnimatedTileObjectPointers.Spores or AnimatedTileObjectPointers.HorizontalSpikes))
                foreach (var segment in Frames(definition))
                {
                    total += segment.ByteCount;
                    yield return segment;
                }
        for (int frame = 0; frame < TreadmillFrameCount; frame++)
        {
            total += WreckedShipTreadmillRomData.TransferByteCount;
            yield return new(WreckedShipTreadmillRomData.FrameSource(frame),
                WreckedShipTreadmillRomData.TransferByteCount, true);
        }
        total += TourianStatueAnimatedTileArtworkDefinitions.TransferByteCount;
        yield return new(TourianStatueAnimatedTileArtworkDefinitions.FirstSource,
            TourianStatueAnimatedTileArtworkDefinitions.TransferByteCount, false);
        RoomFxAnimatedTileMechanicsDefinitions.TryResolve(AnimatedTileObjectPointers.Spores, out var spores);
        foreach (var segment in Frames(spores))
        {
            total += segment.ByteCount;
            yield return segment;
        }
        RoomFxAnimatedTileMechanicsDefinitions.TryResolve(AnimatedTileObjectPointers.HorizontalSpikes, out var spikes);
        foreach (var segment in Frames(spikes))
        {
            total += segment.ByteCount;
            yield return segment;
        }
        if (total != TotalByteCount)
            throw new InvalidOperationException("Compiled room-FX atlas segments do not match the PNG geometry.");

        static IEnumerable<RoomFxAtlasSegment> Frames(RoomFxAnimatedTileObjectDefinition definition)
        {
            foreach (RoomFxAnimatedTileFrameDefinition frame in definition.Frames)
            {
                // The last spike step reuses image 1, already present in the strip.
                if (definition.ObjectPointer == AnimatedTileObjectPointers.HorizontalSpikes &&
                    frame.InstructionPointer == definition.Frames[3].InstructionPointer) continue;
                yield return new(RoomFxAnimatedTileArtworkDefinitions.SourceAddress(definition, frame.InstructionPointer),
                    definition.TransferByteCount, true);
            }
        }
    }
}

/// <summary>One compiled source segment in the installed room-FX artwork strip.</summary>
/// <param name="SourceAddress">Full native SNES artwork identity, used to resolve installed frame transfers without cartridge reads.</param>
/// <param name="ByteCount">Number of ordered 2bpp planar bytes contributed to the PNG strip, not a pixel count.</param>
/// <param name="IsFrame">True for a complete directly resolvable native DMA frame; false for the shared statue strip whose overlapping transfer windows are resolved separately.</param>
public readonly record struct RoomFxAtlasSegment(int SourceAddress, int ByteCount, bool IsFrame);
