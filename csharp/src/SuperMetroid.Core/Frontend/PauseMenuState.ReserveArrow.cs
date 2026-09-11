using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    /// <summary>Runs $82:AD0A after the tank input handler and D-pad response.</summary>
    private void UpdateReserveArrow(byte nmiFrameCounter8)
    {
        bool inTanks = selectedCategory == PauseEquipmentCategories.Reserves;
        bool animated = inTanks && selectedItem == PauseReserveTransferRomData.ModeItem &&
            samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode;
        bool enabled = animated || inTanks && selectedItem == PauseReserveTransferRomData.TransferItem;
        SetReserveArrow(enabled, animated, nmiFrameCounter8);
    }

    private void SetReserveArrow(bool enabled, bool animated = false, byte nmiFrameCounter8 = 0)
    {
        int offset = (nmiFrameCounter8 & PauseReserveArrowRomData.FrameMask) * sizeof(ushort);
        cgram.SetColor(PauseReserveArrowRomData.Color6Index, animated
            ? RomDataReader.ReadWordFixedBank(bus, PauseReserveArrowRomData.Color6Table + offset)
            : PauseReserveArrowRomData.SolidColor6);
        cgram.SetColor(PauseReserveArrowRomData.Color11Index, animated
            ? RomDataReader.ReadWordFixedBank(bus, PauseReserveArrowRomData.Color11Table + offset)
            : PauseReserveArrowRomData.SolidColor11);
        int palette = enabled ? PauseReserveArrowRomData.EnabledPalette : PauseReserveArrowRomData.DisabledPalette;
        for (int row = 0; row < PauseReserveArrowRomData.VerticalCount; row++)
            SetPalette(PauseReserveArrowRomData.VerticalStart + row * PauseReserveArrowRomData.RowStride);
        for (int column = 0; column < PauseReserveArrowRomData.HorizontalCount; column++)
            SetPalette(PauseReserveArrowRomData.HorizontalStart + column * sizeof(ushort));

        void SetPalette(int byteOffset)
        {
            Span<byte> destination = equipmentTilemap.AsSpan(byteOffset, sizeof(ushort));
            var tile = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(destination));
            BinaryPrimitives.WriteUInt16LittleEndian(destination, tile.WithPaletteIndex(palette).Raw);
        }
    }
}
