namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>White-fade values and cadence for Samus's flyaway in the ending sequence.</summary>
internal static partial class EndingFlyawayFadeDefinitions
{
    /// <summary>The updates the fast emergence holds its full white before the white fade begins.</summary>
    public const int WhiteHoldFrames = 192;
    /// <summary>The white fade takes its first step of white off on its first update, once the hold is over.</summary>
    public const int FirstStepUpdate = 1;
    /// <summary>The updates the white fade waits between each step of white it takes off thereafter.</summary>
    public const int UpdatesPerStep = 8;
}
