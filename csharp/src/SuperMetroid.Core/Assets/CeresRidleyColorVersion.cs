namespace SuperMetroid.Core.Assets;

/// <summary>Schema generations of the installed Ridley/Baby RGB5 palette document.</summary>
public enum CeresRidleyColorVersion
{
    /// <summary>Legacy schema version one, lacking Baby and alarm rows; loading requires a stock catalog to provide both families.</summary>
    PreBaby = 1,
    /// <summary>Legacy schema version two, containing Baby rows but lacking alarm rows; loading requires a stock catalog for the alarm family.</summary>
    PreAlarm = 2,
    /// <summary>Current schema version three, requiring the complete startup, fade, health, retreat, Baby, and alarm palette families.</summary>
    Current = 3,
}
