namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>White-fade values and cadence for Samus's flyaway in the ending sequence.</summary>
internal static class EndingFlyawayFadeDefinitions
{
    /// <summary>The steps of white wash the flyaway starts under: all of them, full white.</summary>
    public const int InitialWhite = WhiteWash.Steps;
    /// <summary>The updates the fast emergence holds its full white before the white fade begins.</summary>
    public const int WhiteHoldFrames = 192;
    /// <summary>The flyaway's setup starts the first white-fade countdown at one.</summary>
    public const int InitialCountdown = 1;
    /// <summary>The white fade takes one step of white off once every eight updates thereafter.</summary>
    public const int Interval = 8;
}
