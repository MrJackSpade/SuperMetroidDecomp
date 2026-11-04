namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Verification access to <see cref="SamusArmCannonSourceArtwork"/> members production does not use.</summary>
internal static class SamusArmCannonSourceArtworkAccess
{
    /// <summary>Exposes <see cref="SamusArmCannonSourceArtwork"/> state that only verification reads.</summary>
    extension(SamusArmCannonSourceArtwork self)
    {
        /// <summary>The bytes of installed cover tile <paramref name="index"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">No such cover tile is installed.</exception>
        internal ReadOnlyMemory<byte> CoverTile(int index) => (uint)index < SamusArmCannonArtworkFormat.CoverTileCount
            ? self.tiles.Characters.Slice(index * SamusArmCannonArtworkFormat.CoverTileByteCount, SamusArmCannonArtworkFormat.CoverTileByteCount)
            : throw new ArgumentOutOfRangeException(nameof(index), index, "No such arm-cannon cover tile is installed.");
    }
}
