using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="GrappleTileAtlas"/> members production does not use.</summary>
internal static class GrappleTileAtlasAccess
{
    /// <summary>Exposes <see cref="GrappleTileAtlas"/> state that only verification reads.</summary>
    extension(GrappleTileAtlas self)
    {
        /// <summary>Resolves native endpoint/segment queue records without requiring an eager queue rewrite.</summary>
        public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
        {
            foreach (var transfer in GrappleTileDefinitions.Transfers)
                if (transfer.SourceAddress == sourceAddress && transfer.ByteCount == byteCount)
                {
                    data = self.Resolve(transfer.Asset);
                    return true;
                }
            data = default;
            return false;
        }
    }
}
