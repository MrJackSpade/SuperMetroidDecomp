
namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Verification access to <see cref="RoomScrollGrid"/> members production does not use.</summary>
internal static class RoomScrollGridAccess
{
    /// <summary>Exposes all 50 copied scroll bytes, including data beyond a short room's dimensions.</summary>
    extension(RoomScrollGrid self)
    {
        /// <summary>
        /// All 50 bytes, including bytes beyond the room dimensions. The original explicit
        /// loader at <c>$82:E878-$82:E889</c> copies 25 words unconditionally, so edge reads can
        /// observe data following a shorter ROM table rather than an invented zero padding.
        /// </summary>
        internal ReadOnlySpan<byte> Storage => self._cells;
    }
}
