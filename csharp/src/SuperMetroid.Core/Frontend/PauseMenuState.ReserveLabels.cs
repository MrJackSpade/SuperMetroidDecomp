using System.Buffers.Binary;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    private void WriteReserveLabels()
    {
        // Absence of capacity leaves the original blank template intact. Restoring
        // the template before rebuilding also removes labels if inventory is changed.
        if (samus.MaxReserveEnergy == 0) return;
        for (int row = 0; row < PauseReserveLabelRomData.LabelCount; row++)
        {
            int destination = RomDataReader.ReadWordFixedBank(bus,
                PauseReserveLabelRomData.DestinationTable + row * sizeof(ushort)) - PauseReserveLabelRomData.TilemapBase;
            ushort source = RomDataReader.ReadWordFixedBank(bus,
                PauseReserveLabelRomData.SourceTable + row * sizeof(ushort));
            CopyBank82Words(source, equipmentTilemap.AsSpan(destination, PauseReserveLabelRomData.LabelByteCount));
        }
        // Native setup retains the initial MANUAL label for the zero/uninitialized
        // mode; it only performs the four-word substitution for a nonzero mode.
        if (samus.ReserveTankMode == 0) return;
        int modeSource = samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode
            ? PauseReserveLabelRomData.AutoTilemap : PauseReserveLabelRomData.ManualTilemap;
        for (int index = 0; index < PauseReserveLabelRomData.ModeWordCount; index++)
        {
            Span<byte> destination = equipmentTilemap.AsSpan(PauseReserveLabelRomData.ModeByteOffset + index * sizeof(ushort), sizeof(ushort));
            ushort before = BinaryPrimitives.ReadUInt16LittleEndian(destination);
            ushort source = RomDataReader.ReadWordFixedBank(bus, modeSource + index * sizeof(ushort));
            BinaryPrimitives.WriteUInt16LittleEndian(destination,
                (ushort)((before & PauseReserveLabelRomData.AttributeMask) | source));
        }
    }
}
