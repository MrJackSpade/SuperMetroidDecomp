using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Native body-art allocations and transfer-row geometry, independent of chosen pixel content.</summary>
internal static class SamusBodyTransferDefinitions
{
    /// <summary>Bytes in one 4bpp OBJ tile, used to calculate transfer row sizes.</summary>
    private const int TileBytes = 32;
    /// <summary>Number of 8-pixel tiles across one large 16-pixel Samus sprite part.</summary>
    private const int LargeObjTilesAcross = 16 / 8;

    /// <summary>$9F:ED80, SamusTiles_Top_Set6_EntryC, relocated continuation of the same artwork group.</summary>
    private const int Top6Continuation = 0x9fed80;
    /// <summary>$92:D039 selects upper set6 entryC as the first record in the relocated source allocation.</summary>
    private const int Top6ContinuationPosition = 12;

    /// <summary>Each upper-body set's first tile allocation, indexed by set. 0: $9E:8000 packed general upper artwork; 1: $9E:A660 packed general upper artwork; 2: $9E:C820 packed general upper artwork; 3: $9C:9B00 clockwise upright grappling artwork; 4: $9C:B2C0 anticlockwise upright grappling artwork; 5: $9D:C980 clockwise inverted grappling artwork; 6: $9D:E140 first inverted anticlockwise allocation; 7: $9C:D2C0 standing/shinespark artwork; 8: $9B:E000 appearance electricity; 9: $9B:EE20 running horizontal cannon artwork; 10: $9C:8000 morph/spin/screw artwork; 11: $9C:9620 morph transitions and bounce artwork; 12: $9F:DB80 vertical shinespark/crystal-flash artwork.</summary>
    private static readonly SnesAddress[] TopSources =
        [SnesAddress.FromBusAddress(0x9e8000), SnesAddress.FromBusAddress(0x9ea660), SnesAddress.FromBusAddress(0x9ec820), SnesAddress.FromBusAddress(0x9c9b00), SnesAddress.FromBusAddress(0x9cb2c0), SnesAddress.FromBusAddress(0x9dc980), SnesAddress.FromBusAddress(0x9de140), SnesAddress.FromBusAddress(0x9cd2c0), SnesAddress.FromBusAddress(0x9be000), SnesAddress.FromBusAddress(0x9bee20), SnesAddress.FromBusAddress(0x9c8000), SnesAddress.FromBusAddress(0x9c9620), SnesAddress.FromBusAddress(0x9fdb80)];

    /// <summary>Each lower-body set's first tile allocation, indexed by set. 0: $9D:8000 general lower artwork; 1: $9D:A7E0 general lower artwork; 2: $9F:8000 clockwise upright grappling artwork; 3: $9E:E9C0 preserved cable glyph allocation; 4: $9F:9780 clockwise inverted grappling artwork; 5: $9F:ADC0 anticlockwise upright grappling artwork; 6: $9F:C540 anticlockwise inverted grappling artwork; 7: $9F:E680 crystal-flash lower artwork; 8: $9B:EA00 forward-facing lower artwork; 9: $9C:EA80 space-jump/screw rotational body artwork; 10: $9F:EB00 general lower artwork.</summary>
    private static readonly SnesAddress[] BottomSources =
        [SnesAddress.FromBusAddress(0x9d8000), SnesAddress.FromBusAddress(0x9da7e0), SnesAddress.FromBusAddress(0x9f8000), SnesAddress.FromBusAddress(0x9ee9c0), SnesAddress.FromBusAddress(0x9f9780), SnesAddress.FromBusAddress(0x9fadc0), SnesAddress.FromBusAddress(0x9fc540), SnesAddress.FromBusAddress(0x9fe680), SnesAddress.FromBusAddress(0x9bea00), SnesAddress.FromBusAddress(0x9cea80), SnesAddress.FromBusAddress(0x9feb00)];

    /// <summary>Locates a body tile allocation by summing earlier payload lengths, including set six's relocated continuation.</summary>
    internal static int SourceAddress(SamusBodyArtworkCatalog body, bool upper, int set, int position)
    {
        SnesAddress[] sources = upper ? TopSources : BottomSources;
        int start = (uint)set < (uint)sources.Length ? sources[set].Value : throw new ArgumentOutOfRangeException(nameof(set));
        int first = 0;
        if (upper && set == 6 && position >= Top6ContinuationPosition)
        {
            start = Top6Continuation;
            first = Top6ContinuationPosition;
        }
        IReadOnlyList<SamusBodyTileDefinition> group = upper ? body.TopSet(set) : body.BottomSet(set);
        for (int index = first; index < position; index++) start += group[index].PayloadLength;
        return start;
    }

    /// <summary>Import binding reuses its transient calculated snapshot across records; no snapshot is retained by the installed catalog.</summary>
    internal static bool TryFirstSize(SamusBodyArtworkCatalog body, SamusBodyTileSlot slot,
        int payloadBytes, ReadOnlySpan<int> poseFirstFrames, ReadOnlySpan<SamusBodyFrameSelection> frames, out ushort firstSize)
    {
        bool upper = slot.Upper;
        int set = slot.Set, position = slot.Position;
        if (TrySelectedPacking(upper, set, position, payloadBytes, out firstSize)) return true;
        int firstTiles = 0;
        for (int pose = 0; pose < poseFirstFrames.Length; pose++)
        {
            int start = poseFirstFrames[pose], end = SamusBodyArtworkCatalog.FrameCount;
            foreach (int next in poseFirstFrames) if (next > start && next < end) end = next;
            int baseIndex = upper ? body.Spritemaps.TopBase((SamusPoseId)pose) : body.Spritemaps.BottomBase((SamusPoseId)pose);
            for (int phase = 0; phase < end - start; phase++)
            {
                int frameIndex = start + phase;
                if ((uint)frameIndex >= frames.Length) continue;
                SamusBodyFrameSelection frame = frames[frameIndex];
                if ((upper ? frame.TopSet : frame.BottomSet) != set ||
                    (upper ? frame.TopPosition : frame.BottomPosition) != position) continue;
                int mapIndex = baseIndex + phase;
                if ((uint)mapIndex >= SamusSpritemapArtworkCatalog.PointerCount ||
                    !body.Spritemaps.TryGet((ushort)mapIndex, out SamusSpritemapDefinition? map)) continue;
                foreach (SamusSpritePart part in map.Parts)
                {
                    int tile = part.Attributes.TileNumber - (upper ? 0 : 8);
                    int width = part.X.IsLarge ? LargeObjTilesAcross : 1;
                    for (int x = 0; x < width; x++)
                        if ((uint)(tile + x) < 8) firstTiles = Math.Max(firstTiles, tile + x + 1);
                }
            }
        }
        firstSize = (ushort)Math.Min(firstTiles * TileBytes, payloadBytes);
        return firstSize != 0;
    }

    /// <summary>
    /// Selected packing for exact allocations without an in-group canonical OAM
    /// basis. Upper0/1E–1F and2/12–15 have one large body block; upper1/0–1,
    /// 2/2,3/10,5/10 have two. Remaining named allocations use balanced rows.
    /// Only this decomposition is retained; row magnitudes derive from hardware
    /// block geometry and the independently required pixel payload length.
    /// </summary>
    internal static bool TrySelectedPacking(bool upper, int set, int position, int payloadBytes, out ushort firstSize)
    {
        int largeBlocks = new SamusBodyTileSlot(upper, set, position) switch
        {
            (true, 0, 30 or 31) or (true, 2, >= 18 and <= 21) => 1,
            (true, 1, 0 or 1) or (true, 2, 2) or (true, 3 or 5, 16) => 2,
            _ => 0,
        };
        if (largeBlocks != 0)
        {
            int first = payloadBytes - largeBlocks * LargeObjTilesAcross * TileBytes;
            firstSize = (ushort)Math.Max(0, first);
            return first > 0;
        }
        bool balanced = new SamusBodyTileSlot(upper, set, position) switch
        {
            (true, 1, 4 or 22 or 23) or (true, 8, 1) or (true, 11, 0 or 2) or
                (true, 4 or 6, 16) => true,
            (false, 1, 16 or 29) or (false, 2 or 5, 10 or 23) or
                (false, 3, >= 0 and <= 16) or (false, 4, 10 or 11 or 12 or 22) or
                (false, 6, 10 or 11 or 12) or (false, 8, 2) => true,
            _ => false,
        };
        firstSize = balanced ? (ushort)(((payloadBytes / TileBytes + 1) / 2) * TileBytes) : (ushort)0;
        return balanced;
    }
}
