using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Development-tool members of <see cref="CartridgeImportAddressSpace"/>; never linked by player hosts.</summary>
internal static class CartridgeImportAddressSpaceTooling
{
    /// <summary>Loads a retail ROM image, accepting either a bare image or a 512-byte copier header before the ROM data.</summary>
    /// <param name="path">File path to the retail Super Metroid ROM.</param>
    /// <returns>An address space backed by the validated ROM image.</returns>
    /// <exception cref="InvalidDataException">The file size or LoROM header title does not match the supported retail image.</exception>
    public static CartridgeImportAddressSpace LoadRetailRom(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        // Read through a pooled buffer: the shared image cache copies only content it has not seen.
        using var file = File.OpenRead(path);
        long length = file.Length;
        if (length is not (CartridgeImportAddressSpace.RetailRomByteCount or CartridgeImportAddressSpace.RetailRomByteCount + 512))
            throw new InvalidDataException(
                $"Expected a ${CartridgeImportAddressSpace.RetailRomByteCount:X} byte retail ROM (optionally plus a 512-byte copier header), " +
                $"but '{path}' contains ${length:X} bytes.");
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent((int)length);
        try
        {
            file.ReadExactly(buffer, 0, (int)length);
            ReadOnlySpan<byte> romBytes = buffer.AsSpan((int)length - CartridgeImportAddressSpace.RetailRomByteCount, CartridgeImportAddressSpace.RetailRomByteCount);
            if (!romBytes.Slice(0x7fc0, "Super Metroid"u8.Length).SequenceEqual("Super Metroid"u8))
                throw new InvalidDataException($"'{path}' does not contain the expected SUPER METROID LoROM header title.");
            return new CartridgeImportAddressSpace(romBytes);
        }
        finally { System.Buffers.ArrayPool<byte>.Shared.Return(buffer); }
    }
}

/// <summary>Development-tool instance members of <see cref="CartridgeImportAddressSpace"/>.</summary>
internal static class CartridgeImportAddressSpaceToolingExtensions
{
    /// <summary>Provides development-tool access to the immutable ROM bytes owned by an imported address space.</summary>
    extension(CartridgeImportAddressSpace self)
    {
        /// <summary>Gets the imported cartridge image without exposing mutable storage.</summary>
        public ReadOnlySpan<byte> Rom => self._rom;
    }
}
