using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Development-tool members of <see cref="CartridgeImportAddressSpace"/>; never linked by player hosts.</summary>
internal static class CartridgeImportAddressSpaceTooling
{
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
    extension(CartridgeImportAddressSpace self)
    {
        public ReadOnlySpan<byte> Rom => self._rom;
    }
}
