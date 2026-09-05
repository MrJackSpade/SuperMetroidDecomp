using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Exports the unchanged cartridge SRAM captured at a recording's start for emulator comparison.</summary>
internal static class ReplaySaveRamExporter
{
    public static int Run(string recordingPath, string romPath, string destination)
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
        // Never overwrite an emulator's live battery save or modify the captured SRAM.
        using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write);
        output.Write(recording.InitialSaveRam);
        Console.WriteLine($"Exported {recording.InitialSaveRam.Length} bytes to {Path.GetFullPath(destination)}");
        return 0;
    }
}
