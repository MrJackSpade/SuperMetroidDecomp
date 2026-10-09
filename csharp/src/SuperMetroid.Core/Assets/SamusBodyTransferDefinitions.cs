namespace SuperMetroid.Core.Assets;

/// <summary>Native body-art allocations and transfer-row geometry, independent of chosen pixel content.</summary>
internal static class SamusBodyTransferDefinitions
{
    /// <summary>Byte size of one planar 4-bpp 8-by-8 Samus OBJ tile.</summary>
    private const int TileBytes = 32;

    /// <summary>Number of eight-pixel tiles across a sixteen-pixel-wide large OBJ block.</summary>
    private const int LargeObjTilesAcross = 16 / 8;

    /// <summary>$9E:8000, SamusTiles_Top_Set0_Entry0, packed general upper artwork.</summary>
    private const int Top0 = 0x9e8000;
    /// <summary>$9E:A660, SamusTiles_Top_Set1_Entry0, packed general upper artwork.</summary>
    private const int Top1 = 0x9ea660;
    /// <summary>$9E:C820, SamusTiles_Top_Set2_Entry0, packed general upper artwork.</summary>
    private const int Top2 = 0x9ec820;
    /// <summary>$9C:9B00, SamusTiles_Top_Set3_Entry0, clockwise upright grappling artwork.</summary>
    private const int Top3 = 0x9c9b00;
    /// <summary>$9C:B2C0, SamusTiles_Top_Set4_Entry0, anticlockwise upright grappling artwork.</summary>
    private const int Top4 = 0x9cb2c0;
    /// <summary>$9D:C980, SamusTiles_Top_Set5_Entry0, clockwise inverted grappling artwork.</summary>
    private const int Top5 = 0x9dc980;
    /// <summary>$9D:E140, SamusTiles_Top_Set6_Entry0, first inverted anticlockwise allocation.</summary>
    private const int Top6 = 0x9de140;
    /// <summary>$9F:ED80, SamusTiles_Top_Set6_EntryC, relocated continuation of the same artwork group.</summary>
    private const int Top6Continuation = 0x9fed80;
    /// <summary>$92:D039 selects upper set6 entryC as the first record in the relocated source allocation.</summary>
    private const int Top6ContinuationPosition = 12;
    /// <summary>$9C:D2C0, SamusTiles_Top_Set7_Entry0, standing/shinespark artwork.</summary>
    private const int Top7 = 0x9cd2c0;
    /// <summary>$9B:E000, SamusTiles_Top_Set8_Entry0, appearance electricity.</summary>
    private const int Top8 = 0x9be000;
    /// <summary>$9B:EE20, SamusTiles_Top_Set9_Entry0, running horizontal cannon artwork.</summary>
    private const int Top9 = 0x9bee20;
    /// <summary>$9C:8000, SamusTiles_Top_SetA_Entry0, morph/spin/screw artwork.</summary>
    private const int TopA = 0x9c8000;
    /// <summary>$9C:9620, SamusTiles_Top_SetB_Entry0, morph transitions and bounce artwork.</summary>
    private const int TopB = 0x9c9620;
    /// <summary>$9F:DB80, SamusTiles_Top_SetC_Entry0, vertical shinespark/crystal-flash artwork.</summary>
    private const int TopC = 0x9fdb80;
    /// <summary>$9D:8000, SamusTiles_Bottom_Set0_Entry0, general lower artwork.</summary>
    private const int Bottom0 = 0x9d8000;
    /// <summary>$9D:A7E0, SamusTiles_Bottom_Set1_Entry0, general lower artwork.</summary>
    private const int Bottom1 = 0x9da7e0;
    /// <summary>$9F:8000, SamusTiles_Bottom_Set2_Entry0, clockwise upright grappling artwork.</summary>
    private const int Bottom2 = 0x9f8000;
    /// <summary>$9E:E9C0, UNUSED_SamusTiles_Bottom_Set3_Entry0, preserved cable glyph allocation.</summary>
    private const int Bottom3 = 0x9ee9c0;
    /// <summary>$9F:9780, SamusTiles_Bottom_Set4_Entry0, clockwise inverted grappling artwork.</summary>
    private const int Bottom4 = 0x9f9780;
    /// <summary>$9F:ADC0, SamusTiles_Bottom_Set5_Entry0, anticlockwise upright grappling artwork.</summary>
    private const int Bottom5 = 0x9fadc0;
    /// <summary>$9F:C540, SamusTiles_Bottom_Set6_Entry0, anticlockwise inverted grappling artwork.</summary>
    private const int Bottom6 = 0x9fc540;
    /// <summary>$9F:E680, SamusTiles_Bottom_Set7_Entry0, crystal-flash lower artwork.</summary>
    private const int Bottom7 = 0x9fe680;
    /// <summary>$9B:EA00, SamusTiles_Bottom_Set8_Entry0, forward-facing lower artwork.</summary>
    private const int Bottom8 = 0x9bea00;
    /// <summary>$9C:EA80, SamusTiles_Bottom_Set9_Entry0, space-jump/screw rotational body artwork.</summary>
    private const int Bottom9 = 0x9cea80;
    /// <summary>$9F:EB00, SamusTiles_Bottom_SetA_Entry0, general lower artwork.</summary>
    private const int BottomA = 0x9feb00;

    /// <summary>Resolves a selected upper- or lower-body tile group's position to its cartridge source address.</summary>
    /// <param name="body">Artwork catalog providing the selected group's ordered payload lengths.</param>
    /// <param name="upper"><see langword="true"/> to address an upper-body set; otherwise, a lower-body set.</param>
    /// <param name="set">Set index within the selected upper- or lower-body groups.</param>
    /// <param name="position">Zero-based tile position within that set.</param>
    /// <returns>The SNES source address after summing preceding tile payloads; upper set 6 positions 12 and later use its relocated continuation.</returns>
    internal static int SourceAddress(SamusBodyArtworkCatalog body, bool upper, int set, int position)
    {
        int start = (upper, set) switch
        {
            (true, 0) => Top0, (true, 1) => Top1, (true, 2) => Top2,
            (true, 3) => Top3, (true, 4) => Top4, (true, 5) => Top5,
            (true, 6) => Top6, (true, 7) => Top7, (true, 8) => Top8,
            (true, 9) => Top9, (true, 10) => TopA, (true, 11) => TopB,
            (true, 12) => TopC,
            (false, 0) => Bottom0, (false, 1) => Bottom1, (false, 2) => Bottom2,
            (false, 3) => Bottom3, (false, 4) => Bottom4, (false, 5) => Bottom5,
            (false, 6) => Bottom6, (false, 7) => Bottom7, (false, 8) => Bottom8,
            (false, 9) => Bottom9, (false, 10) => BottomA,
            _ => throw new ArgumentOutOfRangeException(nameof(set)),
        };
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
    internal static bool TryFirstSize(SamusBodyArtworkCatalog body, bool upper, int set, int position,
        int payloadBytes, ReadOnlySpan<ushort> pointers, ReadOnlySpan<SamusBodyFrameSelection> frames, out ushort firstSize)
    {
        if (TrySelectedPacking(upper, set, position, payloadBytes, out firstSize)) return true;
        int firstTiles = 0;
        for (int pose = 0; pose < pointers.Length; pose++)
        {
            int start = pointers[pose], end = SamusBodyArtworkCatalog.FrameEndOffset;
            foreach (ushort next in pointers) if (next > start && next < end) end = next;
            int baseIndex = upper ? body.Spritemaps.TopBase((byte)pose) : body.Spritemaps.BottomBase((byte)pose);
            for (int phase = 0; phase < (end - start) / 4; phase++)
            {
                int frameIndex = (start - SamusBodyArtworkCatalog.FirstFrameOffset) / 4 + phase;
                if ((uint)frameIndex >= frames.Length) continue;
                SamusBodyFrameSelection frame = frames[frameIndex];
                if ((upper ? frame.TopSet : frame.BottomSet) != set ||
                    (upper ? frame.TopPosition : frame.BottomPosition) != position) continue;
                int mapIndex = baseIndex + phase;
                if ((uint)mapIndex >= SamusSpritemapArtworkCatalog.PointerCount ||
                    !body.Spritemaps.TryGet((ushort)mapIndex, out SamusSpritemapDefinition? map)) continue;
                foreach (SamusSpritePart part in map!.Parts)
                {
                    int tile = (part.Attributes & 511) - (upper ? 0 : 8);
                    int width = (part.X & 0x8000) != 0 ? LargeObjTilesAcross : 1;
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
    private static bool TrySelectedPacking(bool upper, int set, int position, int payloadBytes, out ushort firstSize)
    {
        int largeBlocks = (upper, set, position) switch
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
        bool balanced = (upper, set, position) switch
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
