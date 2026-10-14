using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="PauseReserveTransferRomData"/> that only verification reads.</summary>
internal static class PauseReserveTransferRomDataConstants
{
    /// <summary>$82:BF04, ReserveTank_TransferEnergyPerFrame, consumed as a ROM word.</summary>
    public const int TransferAmount = 0x82bf04;

    /// <summary>Provides the native reserve transfer amount used to check pause menu resource movement.</summary>
    extension(PauseReserveTransferRomData)
    {
        /// <inheritdoc cref="PauseReserveTransferRomDataConstants.TransferAmount"/>
        internal static int TransferAmount => PauseReserveTransferRomDataConstants.TransferAmount;
    }
}
