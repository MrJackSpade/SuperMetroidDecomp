namespace SuperMetroid.Core.Assets;

/// <summary>Editable result-credit tilemap based on $8C:DC9B, with a bounded producer label layered over the first row.</summary>
public sealed record EndingResultPanel
{
    /// <summary>Exactly 288 packed BG tilemap cells in nine 32-cell rows; the producer-label region is cleared and replaced during loading.</summary>
    public required EndingTextCell[] Template { get; init; }
    /// <summary>One through eleven uppercase letters or spaces drawn in the small font beginning at row 0, column 10 of the result panel.</summary>
    public required string Text { get; init; }
}
