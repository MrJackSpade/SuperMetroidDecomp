namespace SuperMetroid.Core.Saves;

/// <summary>The gameplay-update-based clock persisted by the cartridge.</summary>
/// <param name="Hours">Hours component, normally saturated at 99.</param>
/// <param name="Minutes">Minutes component, normally from 0 through 59.</param>
/// <param name="Seconds">Seconds component, normally from 0 through 59.</param>
/// <param name="Frames">Accepted-update component, normally from 0 through 59.</param>
public sealed record SaveGameTimeJsonDocument(
    ushort Hours,
    ushort Minutes,
    ushort Seconds,
    ushort Frames);
