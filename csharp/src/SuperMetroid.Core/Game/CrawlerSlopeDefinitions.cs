namespace SuperMetroid.Core.Game;

/// <summary>Native non-square-slope tangent scaling shared by crawlers and Yard.</summary>
public static class CrawlerSlopeDefinitions
{
    /// <summary>$A3:E931, adjustedSpeedMultiplier: second word of each four-byte slope record.</summary>
    public const int ReferenceAddress = 0xa3e931;

    /// <summary>The unused additive words are not consumed by the enemy movement routine.</summary>
    public static ushort Multiplier(int shape) => shape switch
    {
        14 or 15 => 0xb0,
        18 or 20 or 21 => 0xc0,
        22 or 23 => 0xd8,
        24 or 25 or 26 => 0xf0,
        27 or 28 => 0x80,
        29 or 30 or 31 => 0x50,
        >= 0 and < 32 => 0x100,
        _ => throw new InvalidDataException($"Crawler slope shape {shape} exceeds the 32 native records."),
    };

    /// <summary>Converts signed 8.8 tangent speed into signed 16.16 displacement without truncating the product.</summary>
    public static int Scale(ushort velocity, int shape) => unchecked((short)velocity) * Multiplier(shape);
}
