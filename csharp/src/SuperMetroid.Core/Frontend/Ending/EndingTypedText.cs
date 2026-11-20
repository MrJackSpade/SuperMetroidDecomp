namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's typed texts (#1513, #2195), each typed one letter at a time after the planet explodes. Installed artwork
/// names each text by its key in <see cref="EndingTypedTextKeys"/>; the values are arbitrary.
/// </summary>
public enum EndingTypedText
{
    /// <summary>THE OPERATION WAS.</summary>
    OperationWas,
    /// <summary>COMPLETED SUCCESSFULLY.</summary>
    CompletedSuccessfully,
    /// <summary>CLEAR TIME, beside which the clear time is spelled out.</summary>
    ClearTime,
}
