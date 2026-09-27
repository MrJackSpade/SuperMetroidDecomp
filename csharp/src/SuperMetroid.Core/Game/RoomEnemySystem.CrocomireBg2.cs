using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's private BG2 scroll and sinking-image ownership.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort CrocomireBlankBg2Tile = 0x0338;
    private const int CrocomireVerticalCorrectionMapTable = 0xa48b79;

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
            unchecked((ushort)(EnemyBg2FrameLayout.VramBase + startWord)),
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
