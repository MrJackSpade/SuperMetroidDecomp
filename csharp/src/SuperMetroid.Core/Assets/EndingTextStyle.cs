namespace SuperMetroid.Core.Assets;

/// <summary>Supported native font layouts and tile attributes for ending text.</summary>
public enum EndingTextStyle : byte
{
    /// <summary>Single-tile alphabet used by the producer/result panel.</summary>
    ResultSmall,
    /// <summary>Two-row alphabet and digits used by the copyright panel.</summary>
    CopyrightLarge,
    /// <summary>Single-tile attributed alphabet used by the percentage sequence.</summary>
    PercentageSmall,
    /// <summary>Two-row attributed alphabet used by the final message.</summary>
    FinalLarge,
}
