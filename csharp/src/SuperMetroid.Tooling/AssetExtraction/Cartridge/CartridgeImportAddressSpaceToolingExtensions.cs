namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>Development-tool instance members of <see cref="CartridgeImportAddressSpace"/>.</summary>
internal static class CartridgeImportAddressSpaceToolingExtensions
{
    /// <summary>Exposes normalized retail image bytes only to asset extraction tools.</summary>
    extension(CartridgeImportAddressSpace self)
    {
        /// <summary>Exposes the normalized image bytes to extraction tools without copying them.</summary>
        public ReadOnlySpan<byte> Rom => self.RomImage;
    }
}
