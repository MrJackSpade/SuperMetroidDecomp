using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="DrawColours"/> members production does not use.</summary>
internal static class DrawColoursAccess
{
    /// <summary>Exposes <see cref="DrawColours"/> state that only verification reads.</summary>
    extension(DrawColours self)
    {
        /// <summary>The key a pixel of ink row <paramref name="inkRow"/> and part row <paramref name="partRow"/> takes.</summary>
        public int Key(int inkRow, int partRow) => self.FollowsPartRows
            ? Ensure.BetweenInclusive(0, DrawColours.Keys - 1, partRow)
            : Ensure.BetweenInclusive(0, DrawColours.InkRows - 1, inkRow);
    }
}
