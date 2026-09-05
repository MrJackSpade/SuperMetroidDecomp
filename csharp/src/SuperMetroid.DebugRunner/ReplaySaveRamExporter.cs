using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using System.Buffers.Binary;

/// <summary>Exports captured SRAM, optionally repairing an explicitly selected main-game slot's native entry point.</summary>
internal static class ReplaySaveRamExporter
{
    public static int Run(string recordingPath, string romPath, string destination, int? mainGameSlot = null)
    {
        var recording = ControllerInputRecording.Read(recordingPath);
        using (var rom = File.OpenRead(romPath))
        {
            if (!System.Security.Cryptography.SHA256.HashData(rom).AsSpan().SequenceEqual(recording.RomSha256))
                throw new InvalidDataException("ROM digest does not match the recording's cartridge.");
        }
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var saves = new SuperMetroidSaveRam(bus);
        if (mainGameSlot is int requestedSlot)
        {
            if ((uint)requestedSlot >= SuperMetroidSaveRam.SlotCount)
                throw new ArgumentOutOfRangeException(nameof(mainGameSlot));
            var selected = saves.ReadSlot(requestedSlot)
                ?? throw new InvalidDataException("Requested slot has no valid cartridge save.");
            if (selected.Area >= SaveRamLayout.PackedMapAreaCount)
                throw new InvalidDataException("Main-game export cannot infer a Ceres/cinematic entry point.");
            SetMainGameEntryPoint(bus.SaveRam, requestedSlot);
            if (saves.ReadSlot(requestedSlot) is null)
                throw new InvalidDataException("Exported slot checksum verification failed.");
            Console.WriteLine($"Set FILE {(char)('A' + requestedSlot)} native load entry point to main game.");
        }
        int validSlots = 0;
        for (int index = 0; index < SuperMetroidSaveRam.SlotCount; index++)
        {
            var slot = saves.ReadSlot(index);
            if (slot is null)
                continue;
            var station = LoadStationEntry.Load(bus, (AreaId)slot.Area, checked((byte)slot.SaveStation));
            Console.WriteLine($"File {(char)('A' + index)}: room ${station.RoomPointer:X4}, equipment ${slot.EquippedItems:X4}, health {slot.Health}/{slot.MaxHealth}");
            validSlots++;
        }
        if (validSlots == 0)
            throw new InvalidDataException("Recording contains no checksum-valid battery save slots.");
        // Never overwrite an emulator's live battery save or modify the recording itself.
        using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write);
        output.Write(bus.SaveRam);
        Console.WriteLine($"Exported {recording.InitialSaveRam.Length} bytes to {Path.GetFullPath(destination)}");
        return 0;
    }

    /// <summary>Changes only the native entry-point word and its four redundant checksum directory words.</summary>
    private static void SetMainGameEntryPoint(Span<byte> sram, int slot)
    {
        int start = SaveRamLayout.SlotOffsets[slot];
        BinaryPrimitives.WriteUInt16LittleEndian(sram[(start + SaveRamLayout.LoadingGameStateOffset)..], CartridgeSaveLoadStates.MainGame);
        ushort checksum = 0;
        for (int offset = 0; offset < SaveRamLayout.SlotByteCount; offset += SaveRamLayout.WordByteCount)
            checksum = unchecked((ushort)(checksum + BinaryPrimitives.ReadUInt16LittleEndian(sram[(start + offset)..])));
        int index = slot * SaveRamLayout.WordByteCount;
        BinaryPrimitives.WriteUInt16LittleEndian(sram[(SaveRamLayout.PrimaryChecksumOffset + index)..], checksum);
        BinaryPrimitives.WriteUInt16LittleEndian(sram[(SaveRamLayout.BackupChecksumOffset + index)..], checksum);
        ushort complement = unchecked((ushort)~checksum);
        BinaryPrimitives.WriteUInt16LittleEndian(sram[(SaveRamLayout.PrimaryComplementOffset + index)..], complement);
        BinaryPrimitives.WriteUInt16LittleEndian(sram[(SaveRamLayout.BackupComplementOffset + index)..], complement);
    }
}
