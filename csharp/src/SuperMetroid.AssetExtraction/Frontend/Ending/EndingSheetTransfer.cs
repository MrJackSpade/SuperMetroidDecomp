namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>One of the ending's fixed sheet transfers, which load the same characters whenever they run.</summary>
/// <param name="Kind">The transfer.</param>
internal sealed record EndingSheetTransfer(EndingTransferKind Kind) : EndingTransfer;
