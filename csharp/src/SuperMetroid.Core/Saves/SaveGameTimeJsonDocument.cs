namespace SuperMetroid.Core.Saves;

/// <summary>The gameplay-update-based clock persisted by the cartridge.</summary>
/// <remarks>
/// Each component is a JSON number the codec accepts only inside the retail clock: hours 0 through 99, the others
/// 0 through 59.
/// </remarks>
/// <param name="Hours">Hours component, normally saturated at 99.</param>
/// <param name="Minutes">Minutes component, normally from 0 through 59.</param>
/// <param name="Seconds">Seconds component, normally from 0 through 59.</param>
/// <param name="Frames">Accepted-update component, normally from 0 through 59.</param>
public sealed record SaveGameTimeJsonDocument(
    int Hours,
    int Minutes,
    int Seconds,
    int Frames);
