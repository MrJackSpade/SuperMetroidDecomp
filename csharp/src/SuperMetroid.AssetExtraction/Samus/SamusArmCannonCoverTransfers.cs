using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The arm cannon's cover tiles by the native transfer that copies each (#1411), which only the importer recognizes.</summary>
internal static class SamusArmCannonCoverTransfers
{
    /// <summary>Recognizes cover-tile transfers.</summary>
    extension(SamusArmCannonArtworkCatalog self)
    {
        /// <summary>The installed cover tile a native <paramref name="byteCount"/>-byte transfer from <paramref name="sourceAddress"/> copies.</summary>
        internal bool TryResolveTile(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
        {
            if (byteCount == SamusDrawingDefinitions.ArmCannon.TileByteCount
                && SnesAddressMath.BankBase(sourceAddress) == SamusCartridgeTables.CharacterDataBank
                && SamusArmCannonArtworkFormat.CoverTileDmaSources.IndexOf(unchecked((ushort)SnesAddressMath.OffsetInBank(sourceAddress))) is int index and >= 0)
            {
                data = self.CoverTile(index);
                return true;
            }
            data = default;
            return false;
        }
    }
}
