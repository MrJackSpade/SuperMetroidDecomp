namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The Zeb, Zebbo, Gamet and Geega Pipe Bugs' rising, shooting, flying and arcing loops (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="PipeBugAnimations"/>.
/// </summary>
public enum PipeBugSegment
{
    /// <summary>The zeb rising left animation.</summary>
    ZebRisingLeft,
    /// <summary>The zeb shooting left animation.</summary>
    ZebShootingLeft,
    /// <summary>The zeb rising right animation.</summary>
    ZebRisingRight,
    /// <summary>The zeb shooting right animation.</summary>
    ZebShootingRight,
    /// <summary>The zebbo rising left animation.</summary>
    ZebboRisingLeft,
    /// <summary>The zebbo shooting left animation.</summary>
    ZebboShootingLeft,
    /// <summary>The zebbo rising right animation.</summary>
    ZebboRisingRight,
    /// <summary>The zebbo shooting right animation.</summary>
    ZebboShootingRight,
    /// <summary>The gamet rising left animation.</summary>
    GametRisingLeft,
    /// <summary>The gamet flying left animation.</summary>
    GametFlyingLeft,
    /// <summary>The gamet rising right animation.</summary>
    GametRisingRight,
    /// <summary>The gamet flying right animation.</summary>
    GametFlyingRight,
    /// <summary>The geega flying left animation.</summary>
    GeegaFlyingLeft,
    /// <summary>The geega arcing left animation.</summary>
    GeegaArcingLeft,
    /// <summary>The geega flying right animation.</summary>
    GeegaFlyingRight,
    /// <summary>The geega arcing right animation.</summary>
    GeegaArcingRight,
}
