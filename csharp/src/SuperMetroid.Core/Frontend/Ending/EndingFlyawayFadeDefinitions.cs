namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Native white-fade values and cadence for Samus's flyaway in the ending sequence.</summary>
internal static class EndingFlyawayFadeDefinitions
{
    /// <summary>The full white intensity, out of 31, of the white wash Func120 starts the flyaway under.</summary>
    public const int InitialWhite = 31;
    /// <summary>Func120 initializes the first Func122 fade countdown to one.</summary>
    public const int InitialCountdown = 1;
    /// <summary>Func122 decreases fixed white once every eight calls thereafter.</summary>
    public const int Interval = 8;
}
