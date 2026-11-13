namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Supported font layouts and colourings for ending text.</summary>
public enum EndingTextStyle
{
    /// <summary>One-row alphabet used by the producer/result panel.</summary>
    ResultSmall,
    /// <summary>Two-row alphabet and digits used by the copyright panel.</summary>
    CopyrightLarge,
    /// <summary>One-row coloured alphabet used by the percentage sequence.</summary>
    PercentageSmall,
    /// <summary>Two-row coloured alphabet used by the final message.</summary>
    FinalLarge,
}
