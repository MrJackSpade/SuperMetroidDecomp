using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's private BG2 scroll and sinking-image ownership.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>BG2 tile index used to erase Crocomire imagery from the working tilemap.</summary>
    private const ushort CrocomireBlankBg2Tile = 0x0338;

    /// <summary>Initializes every word of Crocomire's mutable BG2 tilemap to the blank tile.</summary>
    private void ClearCrocomireBg2WorkingTilemap()
    {
        Span<ushort> tilemap = RequireCrocomireDeath().MutableBg2WorkingTilemap;
        tilemap.Fill(CrocomireBlankBg2Tile);
    }

    /// <summary>Copies a contiguous range of working BG2 tilemap words to the matching VRAM addresses.</summary>
    /// <param name="startWord">First word index in the working tilemap and VRAM layout.</param>
    /// <param name="wordCount">Number of consecutive words to transfer.</param>
    /// <exception cref="ArgumentOutOfRangeException">The requested range falls outside the working tilemap.</exception>
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

    /// <summary>Sets the per-scanline BG2 scroll table to one vertical offset for the whole screen.</summary>
    /// <param name="verticalScroll">BG2 vertical scroll value applied to every scanline.</param>
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
            CrocomireBg2VerticalScroll = CrocomireBg2ScrollDefinitions.VerticalScroll(
                body.YPosition, body.SpritemapPointer);

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

    /// <summary>Calculates the BG2 horizontal offset for the body while it lies within the camera's active view.</summary>
    /// <param name="bodyX">Crocomire body position in room coordinates.</param>
    /// <returns>The camera-relative offset when its wrapped magnitude is below 284; otherwise the cartridge fallback offset 256.</returns>
    private ushort CalculateOnScreenCrocomireBg2Horizontal(ushort bodyX)
    {
        ushort relative = unchecked((ushort)(_crocomireCameraX - bodyX + 51));
        return WrappedMagnitude(relative) >= 284 ? (ushort)256 : relative;
    }
}
