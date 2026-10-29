using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.AssetExtraction.Samus.SamusBodyTransferDefinitions;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The size and packing each stock body transfer selects.</summary>
internal static class SamusBodyTransfers
{
    /// <summary>Import binding reuses its transient calculated snapshot across records; no snapshot is retained by the installed catalog.</summary>
    internal static bool TryFirstSize(InstalledSamusBodyArtwork body, SamusBodyTileSlot slot,
        int payloadBytes, ReadOnlySpan<int> poseFirstFrames, ReadOnlySpan<SamusBodyFrameSelection> frames, out int firstSize)
    {
        bool upper = slot.Upper;
        int set = slot.Set, position = slot.Position;
        if (TrySelectedPacking(upper, set, position, payloadBytes, out firstSize)) return true;
        int firstTiles = 0;
        for (int pose = 0; pose < poseFirstFrames.Length; pose++)
        {
            int start = poseFirstFrames[pose], end = InstalledSamusBodyArtwork.FrameCount;
            foreach (int next in poseFirstFrames) if (next > start && next < end) end = next;
            int baseIndex = upper ? body.Spritemaps.TopBase(SamusPoseNumbering.FromNumber(pose)) : body.Spritemaps.BottomBase(SamusPoseNumbering.FromNumber(pose));
            for (int phase = 0; phase < end - start; phase++)
            {
                int frameIndex = start + phase;
                if ((uint)frameIndex >= frames.Length) continue;
                SamusBodyFrameSelection frame = frames[frameIndex];
                if ((upper ? frame.TopSet : frame.BottomSet) != set ||
                    (upper ? frame.TopPosition : frame.BottomPosition) != position) continue;
                int mapIndex = baseIndex + phase;
                if ((uint)mapIndex >= InstalledSamusSpritemaps.EntryCount ||
                    !body.Spritemaps.Source.TryGet((ushort)mapIndex, out SamusSpritemapDefinition? map)) continue;
                foreach (SpritePart part in map.Parts)
                {
                    int tile = part.Character - (upper ? 0 : 8);
                    int width = part.Large ? LargePartTilesAcross : 1;
                    for (int x = 0; x < width; x++)
                        if ((uint)(tile + x) < 8) firstTiles = Math.Max(firstTiles, tile + x + 1);
                }
            }
        }
        firstSize = Math.Min(firstTiles * TileBytes, payloadBytes);
        return firstSize != 0;
    }

    /// <summary>
    /// Selected packing for exact allocations without an in-group canonical OAM
    /// basis. Upper0/1E–1F and2/12–15 have one large body block; upper1/0–1,
    /// 2/2,3/10,5/10 have two. Remaining named allocations use balanced rows.
    /// Only this decomposition is retained; row magnitudes derive from hardware
    /// block geometry and the independently required pixel payload length.
    /// </summary>
    internal static bool TrySelectedPacking(bool upper, int set, int position, int payloadBytes, out int firstSize)
    {
        int largeBlocks = new SamusBodyTileSlot(upper, set, position) switch
        {
            (true, 0, 30 or 31) or (true, 2, >= 18 and <= 21) => 1,
            (true, 1, 0 or 1) or (true, 2, 2) or (true, 3 or 5, 16) => 2,
            _ => 0,
        };
        if (largeBlocks != 0)
        {
            int first = payloadBytes - largeBlocks * LargePartTilesAcross * TileBytes;
            firstSize = Math.Max(0, first);
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
        firstSize = balanced ? ((payloadBytes / TileBytes + 1) / 2) * TileBytes : 0;
        return balanced;
    }
}
