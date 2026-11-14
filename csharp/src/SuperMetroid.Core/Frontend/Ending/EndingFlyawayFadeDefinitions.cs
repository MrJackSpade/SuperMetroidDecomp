namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>White-fade values and cadence for Samus's flyaway in the ending sequence.</summary>
internal static class EndingFlyawayFadeDefinitions
{
    /// <summary>The steps of white wash the flyaway starts under: all of them, full white.</summary>
    public const int InitialWhite = WhiteWash.Steps;
    /// <summary>The flyaway's setup starts the first white-fade countdown at one.</summary>
    public const int InitialCountdown = 1;
    /// <summary>The white fade takes one step of white off once every eight calls thereafter.</summary>
    public const int Interval = 8;
}
