namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>White-fade values and cadence for Samus's flyaway in the ending sequence.</summary>
internal static class EndingFlyawayFadeDefinitions
{
    /// <summary>The steps of white wash Func120 starts the flyaway under: all of them, full white.</summary>
    public const int InitialWhite = WhiteWash.Steps;
    /// <summary>Func120 initializes the first Func122 fade countdown to one.</summary>
    public const int InitialCountdown = 1;
    /// <summary>Func122 decreases fixed white once every eight calls thereafter.</summary>
    public const int Interval = 8;
}
