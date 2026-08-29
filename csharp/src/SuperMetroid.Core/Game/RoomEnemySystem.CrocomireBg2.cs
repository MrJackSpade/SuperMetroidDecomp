namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's generic extended-tilemap writer and private BG2 scroll ownership.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort CrocomireBlankBg2Tile = 0x0338;
    private const ushort CrocomireBg2TilemapVramBase = 0x4800;
    private const ushort CrocomireBg2WorkingRamBase = 0x2000;
    private const int CrocomireVerticalCorrectionMapTable = 0xa48b79;

    /// <summary>
    /// Ports <c>ProcessExtendedTilemap</c> at $A0:96CA. Each command provides a WRAM byte
    /// destination, word count, and inline words; $FFFF terminates the stream. The original
    /// copies into $7E:2000 and queues one BG2 DMA. Writing the same addressed slices into
    /// both the typed working image and modeled VRAM produces the identical visible state.
    /// </summary>
    private void ProcessExtendedEnemyBg2Tilemap(byte bank, ushort streamPointer)
    {
        int cursor = (bank << 16) | unchecked((ushort)(streamPointer + 2));
        for (int command = 0; command < 128; command++)
        {
            ushort destination = ReadWord(_bus!, cursor);
            if (destination == 0xffff)
                return;

            int wordCount = ReadWord(_bus!, cursor + 2);
            if (wordCount is <= 0 or > CrocomireDeathState.Bg2WorkingWordCount)
            {
                throw new InvalidDataException(
                    $"Extended BG2 command ${bank:X2}:{cursor & 0xffff:X4} has invalid word count {wordCount}.");
            }

            int relativeByte = unchecked((ushort)(destination - CrocomireBg2WorkingRamBase));
            if ((relativeByte & 1) != 0)
            {
                throw new InvalidDataException(
                    $"Extended BG2 command destination ${destination:X4} is not word aligned.");
            }
            int destinationWord = relativeByte >> 1;
            if (destinationWord < 0 ||
                destinationWord + wordCount > CrocomireDeathState.Bg2WorkingWordCount)
            {
                throw new InvalidDataException(
                    $"Extended BG2 command destination ${destination:X4} exceeds the enemy tilemap buffer.");
            }

            ushort[] words = new ushort[wordCount];
            for (int word = 0; word < wordCount; word++)
                words[word] = ReadWord(_bus!, cursor + 4 + word * 2);

            if (_crocomireDeath is { } death)
                words.CopyTo(death.MutableBg2WorkingTilemap.Slice(destinationWord, wordCount));
            _vram!.ExecuteWordTransfer(
                words,
                unchecked((ushort)(CrocomireBg2TilemapVramBase + destinationWord)),
                wordIncrement: 1);
            cursor += 4 + wordCount * 2;
        }

        throw new InvalidDataException(
            $"Extended BG2 command stream ${bank:X2}:{streamPointer:X4} has no terminator.");
    }

    private void ClearCrocomireBg2WorkingTilemap()
    {
        Span<ushort> tilemap = RequireCrocomireDeath().MutableBg2WorkingTilemap;
        tilemap.Fill(CrocomireBlankBg2Tile);
    }

    private void TransferCrocomireBg2Words(int startWord, int wordCount)
    {
        if (startWord < 0 || wordCount < 0 ||
            startWord + wordCount > CrocomireDeathState.Bg2WorkingWordCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startWord));
        }

        _vram!.ExecuteWordTransfer(
            RequireCrocomireDeath().Bg2WorkingTilemap.Slice(startWord, wordCount),
            unchecked((ushort)(CrocomireBg2TilemapVramBase + startWord)),
            wordIncrement: 1);
    }

    private void FillCrocomireBg2ScrollTable(ushort verticalScroll) =>
        RequireCrocomireDeath().MutableBg2ScrollByScanline.Fill(verticalScroll);

    /// <summary>
    /// Erases the 32-tile circular-map row selected by <c>(BG2VOFS+288)&amp;$FFF8</c> while
    /// Crocomire sinks. This is the visual producer that makes the body disappear into acid;
    /// merely moving the enemy's Y word leaves a stale full-height BG2 image behind.
    /// </summary>
    private void ClearCrocomireBg2SinkRow()
    {
        int byteOffset = 8 * ((CrocomireBg2VerticalScroll + 288) & 0xfff8);
        int firstWord = byteOffset >> 1;
        Span<ushort> tilemap = RequireCrocomireDeath().MutableBg2WorkingTilemap;
        if (firstWord < 0 || firstWord + 32 > tilemap.Length)
        {
            throw new InvalidDataException(
                $"Crocomire sink selected BG2 word {firstWord}, outside the {tilemap.Length}-word buffer.");
        }
        tilemap.Slice(firstWord, 32).Fill(CrocomireBlankBg2Tile);
        TransferCrocomireBg2Words(firstWord, 32);
    }

    /// <summary>Ports $A4:8B5B and its fallthrough into $A4:8BA4.</summary>
    private void UpdateCrocomireBg2Scroll(
        CrocomireEnemyState state,
        bool includeVerticalPosition)
    {
        RoomEnemySlot body = state.Body;
        if (includeVerticalPosition)
        {
            ushort vertical = unchecked((ushort)(67 - body.YPosition));
            for (int tableIndex = 16; tableIndex >= 0; tableIndex--)
            {
                ushort authoredMap = ReadWord(
                    _bus!,
                    CrocomireVerticalCorrectionMapTable + tableIndex * 2);
                if (body.SpritemapPointer != authoredMap)
                    continue;

                vertical = unchecked((ushort)(vertical + ReadWord(
                    _bus!,
                    (body.Definition.Bank << 16) |
                    unchecked((ushort)(body.SpritemapPointer + 0x001c)))));
                break;
            }
            CrocomireBg2VerticalScroll = vertical;
        }

        // The tongue is a normal second enemy actor whose var A is the body-relative X
        // offset. $8BA4 updates it even while the body's bulk is represented by BG2.
        if (state.Tongue is { } tongue)
        {
            tongue.XPosition = unchecked((ushort)(body.XPosition + tongue.VariableA));
            tongue.YPosition = body.YPosition;
        }

        ushort horizontal;
        if (unchecked((short)(body.XPosition - _crocomireCameraX)) >= 0)
        {
            horizontal = unchecked((short)(
                body.XPosition - 128 - (_crocomireCameraX + 256))) < 0
                ? CalculateOnScreenCrocomireBg2Horizontal(body.XPosition)
                : (ushort)256;
        }
        else
        {
            horizontal = unchecked((short)(
                body.XPosition + 128 - _crocomireCameraX)) < 0
                ? (ushort)256
                : CalculateOnScreenCrocomireBg2Horizontal(body.XPosition);
        }
        CrocomireBg2HorizontalScroll = horizontal;
    }

    private ushort CalculateOnScreenCrocomireBg2Horizontal(ushort bodyX)
    {
        ushort relative = unchecked((ushort)(_crocomireCameraX - bodyX + 51));
        return WrappedMagnitude(relative) >= 284 ? (ushort)256 : relative;
    }
}
