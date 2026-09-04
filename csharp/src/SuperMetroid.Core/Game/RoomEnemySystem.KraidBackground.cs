using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's private BG2 working surface and its cartridge-authored DMA boundaries.
/// The arm, foot, lints, and nails are ordinary OAM, but the body and animated head are
/// tilemap graphics; recording an "upload count" without moving these words makes only
/// the OAM arm visible.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Ports the working-map construction performed by <c>$A7:AAC6</c>.</summary>
    private void InitializeKraidBackground(KraidEnemyState state)
    {
        byte[] upper = RomDataReader.Decompress(
            _bus!,
            KraidBackgroundRomData.UpperTilemap,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        byte[] lower = RomDataReader.Decompress(
            _bus!,
            KraidBackgroundRomData.LowerTilemap,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        if (upper.Length != KraidBackgroundRomData.DecompressedTilemapBytes ||
            lower.Length != KraidBackgroundRomData.DecompressedTilemapBytes)
        {
            throw new InvalidDataException(
                $"Kraid BG2 sources must each decompress to " +
                $"${KraidBackgroundRomData.DecompressedTilemapBytes:X} bytes; " +
                $"upper=${upper.Length:X}, lower=${lower.Length:X}.");
        }

        Span<ushort> working = state.BackgroundTilemapWords;
        working.Clear();
        for (int word = 0; word < KraidBackgroundRomData.LowerSourceCopyWords; word++)
        {
            working[KraidBackgroundRomData.WorkingLowerHalfWord + word] =
                WithoutKraidBg2Priority(ReadLittleEndianWord(lower, word));
        }
        for (int word = 0; word < KraidBackgroundRomData.VisiblePageWords; word++)
            working[word] = WithoutKraidBg2Priority(ReadLittleEndianWord(upper, word));
        working.Slice(KraidBackgroundRomData.BlankRowWorkingWord, 32)
            .Fill(KraidBackgroundRomData.BlankTile);

        state.BackgroundTilemapsPrepared = true;
        state.OwnsBg2Tilemap = true;
    }

    /// <summary>Uploads the upper 32x32 page exactly as <c>$A7:C874</c> requests.</summary>
    private void TransferKraidTopTilemap(KraidEnemyState state)
    {
        _vram!.ExecuteWordTransfer(
            state.BackgroundTilemapWords.AsSpan(0, KraidBackgroundRomData.VisiblePageWords),
            KraidBackgroundRomData.LiveBg2TilemapWord,
            wordIncrement: 1);
        state.TopTilemapUploadCount++;
    }

    /// <summary>Uploads the lower 32x32 page exactly as <c>$A7:C8B6</c> requests.</summary>
    private void TransferKraidBottomTilemap(KraidEnemyState state)
    {
        _vram!.ExecuteWordTransfer(
            state.BackgroundTilemapWords.AsSpan(
                KraidBackgroundRomData.WorkingLowerHalfWord,
                KraidBackgroundRomData.VisiblePageWords),
            KraidBackgroundRomData.LiveLowerBg2TilemapWord,
            wordIncrement: 1);
        state.BottomTilemapUploadCount++;
    }

    /// <summary>
    /// Installs the 704-byte head tilemap selected by Kraid's private eight-byte
    /// instruction record at <c>$A7:AF3D</c>.
    /// </summary>
    private void TransferKraidHeadTilemap(KraidEnemyState state, ushort sourcePointer)
    {
        Span<ushort> working = state.BackgroundTilemapWords;
        int sourceAddress = KraidBackgroundRomData.EnemyBankBase | sourcePointer;
        for (int word = 0; word < KraidBackgroundRomData.HeadTilemapWords; word++)
            working[word] = ReadWord(_bus!, sourceAddress + word * 2);
        _vram!.ExecuteWordTransfer(
            working[..KraidBackgroundRomData.HeadTilemapWords],
            KraidBackgroundRomData.LiveBg2TilemapWord,
            wordIncrement: 1);
        state.HeadTilemapUploadCount++;
    }

    /// <summary>Ports <c>$A7:AD3A</c>'s priority-bit pass before second phase.</summary>
    private static void SetKraidBg2Priority(KraidEnemyState state)
    {
        Span<ushort> working = state.BackgroundTilemapWords;
        for (int word = 0; word < working.Length; word++)
            working[word] |= KraidBackgroundRomData.PriorityBit;
        state.Bg2PriorityBitsSet = true;
    }

    /// <summary>
    /// Restores the ordinary-room BG2 base used after the body has sunk. Native clears
    /// the two visible 32x32 pages on consecutive frames before reloading BG3 graphics.
    /// </summary>
    private void ClearKraidTopTilemapForDeath(KraidEnemyState state)
    {
        state.BackgroundTilemapWords.AsSpan(0, KraidBackgroundRomData.VisiblePageWords)
            .Fill(KraidBackgroundRomData.BlankTile);
        _vram!.ExecuteWordTransfer(
            state.BackgroundTilemapWords.AsSpan(0, KraidBackgroundRomData.VisiblePageWords / 2),
            KraidBackgroundRomData.OrdinaryRoomBg2TilemapWord,
            wordIncrement: 1);
        state.OwnsBg2Tilemap = false;
        state.TopTilemapUploadCount++;
    }

    /// <summary>Clears the second half of the restored ordinary 32x32 BG2 map.</summary>
    private void ClearKraidBottomTilemapForDeath(KraidEnemyState state)
    {
        _vram!.ExecuteWordTransfer(
            state.BackgroundTilemapWords.AsSpan(0, KraidBackgroundRomData.VisiblePageWords / 2),
            unchecked((ushort)(
                KraidBackgroundRomData.OrdinaryRoomBg2TilemapWord +
                KraidBackgroundRomData.VisiblePageWords / 2)),
            wordIncrement: 1);
        state.BottomTilemapUploadCount++;
    }

    private static ushort WithoutKraidBg2Priority(ushort word) =>
        unchecked((ushort)(word & ~KraidBackgroundRomData.PriorityBit));

    private static ushort ReadLittleEndianWord(ReadOnlySpan<byte> source, int word)
    {
        int offset = checked(word * 2);
        return unchecked((ushort)(source[offset] | (source[offset + 1] << 8)));
    }
}
