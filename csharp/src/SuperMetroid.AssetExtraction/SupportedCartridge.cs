using System.Security.Cryptography;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>The supported Japan/USA NTSC v1.0 cartridge identity, independent of a user's filename.</summary>
public static class SupportedCartridge
{
    /// <summary>Unheadered retail image: 96 physical LoROM banks, 3 MiB total.</summary>
    public const int RomByteCount = SuperMetroidAddressSpace.RetailRomByteCount;
    /// <summary>Optional copier metadata preceding the first physical ROM bank.</summary>
    public const int CopierHeaderByteCount = 512;
    /// <summary>SHA-256 of the verified unmodified Japan/USA NTSC v1.0 image.</summary>
    public const string Sha256 = "12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72";
    /// <summary>Human-readable revision accepted by the installer.</summary>
    public const string Description = "Super Metroid (Japan/USA, NTSC v1.0)";

    /// <summary>Reads bounded input, strips an optional copier header, and rejects other revisions or modifications.</summary>
    public static byte[] Read(Stream source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        byte[] buffer = new byte[RomByteCount + CopierHeaderByteCount + 1];
        int length = 0;
        while (length < buffer.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int read = source.Read(buffer, length, buffer.Length - length);
            if (read == 0) break;
            length += read;
        }
        cancellationToken.ThrowIfCancellationRequested();
        int offset = length switch
        {
            RomByteCount => 0,
            RomByteCount + CopierHeaderByteCount => CopierHeaderByteCount,
            _ => throw new InvalidDataException("Choose a 3 MiB Super Metroid ROM (.smc or .sfc), optionally with a 512-byte copier header. ZIP files are not supported."),
        };
        ReadOnlySpan<byte> rom = buffer.AsSpan(offset, RomByteCount);
        if (!Convert.ToHexString(SHA256.HashData(rom)).Equals(Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"This file is not the supported {Description} ROM. Other revisions, PAL images, and modified ROMs are not supported.");
        return rom.ToArray();
    }
}
