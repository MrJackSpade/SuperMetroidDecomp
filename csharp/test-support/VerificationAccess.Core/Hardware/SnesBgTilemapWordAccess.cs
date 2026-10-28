using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SnesBgTilemapWord"/> members production does not use.</summary>
internal static class SnesBgTilemapWordAccess
{
    /// <summary>Changes BG tile priority while retaining every other tilemap bit.</summary>
    extension(SnesBgTilemapWord self)
    {
        /// <summary>Replaces only the priority bit.</summary>
        internal SnesBgTilemapWord WithPriority(bool priority) =>
            new(unchecked((ushort)(priority ? self.Raw | SnesBgTilemapWord.PriorityMask : self.Raw & ~SnesBgTilemapWord.PriorityMask)));
    }
}
