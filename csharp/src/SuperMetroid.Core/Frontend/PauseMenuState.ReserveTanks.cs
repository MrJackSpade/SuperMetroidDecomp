using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    // Retained at the simulation boundary so redraws cannot advance fill flicker.
    private byte pauseNmiFrameCounter8;

    /// <summary>Ports $82:B2AA's full/partial/empty strip and final cap.</summary>
    private void DrawReserveTanks()
    {
        if (samus.MaxReserveEnergy == 0) return;
        int tank = 0;
        int full = samus.ReserveEnergy / PauseReserveTankRomData.EnergyPerTank;
        int remainder = samus.ReserveEnergy % PauseReserveTankRomData.EnergyPerTank;
        for (; tank < full; tank++) Draw(PauseReserveTankRomData.FullMap, tank);
        if (remainder != 0)
        {
            int quotient = remainder / PauseReserveTankRomData.EnergyPerFillStep;
            int tableOffset = quotient * sizeof(ushort);
            if (tableOffset < PauseReserveTankRomData.FlickerDoubledQuotientLimit &&
                remainder % PauseReserveTankRomData.EnergyPerFillStep != 0 &&
                (pauseNmiFrameCounter8 & PauseReserveTankRomData.FlickerFrameMask) == 0)
                tableOffset += sizeof(ushort);
            if (samus.ReserveEnergy >= PauseReserveTankRomData.EnergyPerTank)
                tableOffset += PauseReserveTankRomData.SecondTableOffset;
            Draw(PauseReserveTankDefinitions.PartialMap(tableOffset / sizeof(ushort)), tank++);
        }
        for (; tank < samus.MaxReserveEnergy / PauseReserveTankRomData.EnergyPerTank; tank++)
            Draw(PauseReserveTankRomData.EmptyMap, tank);
        Draw(PauseReserveTankRomData.EndCapMap, tank);

        void Draw(ushort map, int index)
        {
            (mapPresentation ?? throw new InvalidOperationException(
                "Reserve tanks require installed presentation assets."))
                .PauseReserveTanks.Draw(oam, map, index);
        }
    }
}
