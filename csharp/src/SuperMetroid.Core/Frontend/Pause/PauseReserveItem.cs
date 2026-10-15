namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The two items of the equipment screen's reserve-tank category, valued by their item index.</summary>
internal enum PauseReserveItem
{
    /// <summary>$82:AC87, first tank subdispatcher: mode selector.</summary>
    Mode = PauseReserveTransferRomData.ModeItem,
    /// <summary>$82:AC89, second tank subdispatcher: manual energy transfer.</summary>
    Transfer = PauseReserveTransferRomData.TransferItem,
}
