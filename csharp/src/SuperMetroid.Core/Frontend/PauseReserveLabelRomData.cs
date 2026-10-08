namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$82 reserve-label data used by pause setup, independent of menu control flow.</summary>
internal static class PauseReserveLabelRomData
{
    /// <summary>$82:AB47 recognizes reserve-health mode 1 as AUTO; other nonzero modes use MANUAL.</summary>
    public const ushort AutoMode = 1;
}
