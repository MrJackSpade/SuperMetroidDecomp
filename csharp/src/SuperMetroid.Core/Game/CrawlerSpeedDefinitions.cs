namespace SuperMetroid.Core.Game;

/// <summary>Shared NTSC crawling magnitudes, in 8.8 pixels per frame.</summary>
public static class CrawlerSpeedDefinitions
{
    /// <summary>$A3:E5F0, CrawlersSpeedTable: 32 population-selected magnitudes.</summary>
    public const int ReferenceAddress = 0xa3e5f0;
    /// <summary>$A3:CCA2, YardCrawlingSpeeds: identical NTSC copy used by Yard.</summary>
    public const int YardReferenceAddress = 0xa3cca2;
    /// <summary>$A3:E67A parameter sentinel: preserve existing velocity before applying property signs.</summary>
    public const ushort PreserveVelocity = 0xff;
    /// <summary>Number of authored speed records; trailing zero is an intentional stationary entry.</summary>
    public const int Count = 32;

    private static ReadOnlySpan<ushort> Speeds =>
    [
        0x40, 0x80, 0xc0, 0x100, 0x140, 0x180, 0x1c0, 0x200,
        0x240, 0x280, 0x2c0, 0x300, 0x340, 0x380, 0x400, 0x440,
        0x540, 0x580, 0x5c0, 0x600, 0x640, 0x680, 0x6c0, 0x700,
        0x740, 0x780, 0x7c0, 0x800, 0x840, 0x880, 0x800, 0,
    ];

    /// <summary>Returns an authored magnitude, retaining the native gaps and repeated entries.</summary>
    public static ushort ForParameter(ushort parameter) => parameter < Count
        ? Speeds[parameter]
        : throw new InvalidDataException($"Crawler speed parameter ${parameter:X4} exceeds the 32 authored records.");
}
