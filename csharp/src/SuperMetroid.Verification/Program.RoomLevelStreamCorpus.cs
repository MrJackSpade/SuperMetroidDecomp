using System.Buffers.Binary;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Development-only extraction of the native decompressed level allocations.
    /// The shipped resource is immutable engine baseline data, not an editable
    /// replacement for the separately installed room-layout artwork.
    /// </summary>
    private static void GenerateRoomLevelStreamCorpus()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        string path = Path.GetFullPath(
            "csharp/src/SuperMetroid.Core/Rooms/RoomLevelStreams.bin");
        using var file = new FileStream(path, FileMode.Create, FileAccess.Write);
        file.Write("SMLV"u8);
        Span<byte> word = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(word, 1);
        file.Write(word);
        file.Write(Convert.FromHexString(SupportedCartridge.Sha256));
        BinaryPrimitives.WriteInt32LittleEndian(word, RoomVisualLayoutFiles.RetailSources.Count);
        file.Write(word);
        long payloadBytes = 0;
        foreach (int source in RoomVisualLayoutFiles.RetailSources.Keys.Order())
        {
            byte[] decoded = RomDataReader.Decompress(bus, source);
            BinaryPrimitives.WriteInt32LittleEndian(word, source);
            file.Write(word);
            BinaryPrimitives.WriteInt32LittleEndian(word, decoded.Length);
            file.Write(word);
            file.Write(decoded);
            payloadBytes += decoded.Length;
        }
        Console.WriteLine($"Generated {RoomVisualLayoutFiles.RetailSources.Count} " +
            $"room-level streams ({payloadBytes} payload bytes) at {path}.");
    }
}
