namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The area an elevator map label leads to (#2553): one of the five Zebes areas from Crateria to Maridia, each with its own
/// label picture; no elevator label leads to Tourian or Ceres.
/// </summary>
public enum MapElevatorDestination
{
    /// <summary>An elevator to Crateria.</summary>
    Crateria,

    /// <summary>An elevator to Brinstar.</summary>
    Brinstar,

    /// <summary>An elevator to Norfair.</summary>
    Norfair,

    /// <summary>An elevator to the Wrecked Ship.</summary>
    WreckedShip,

    /// <summary>An elevator to Maridia.</summary>
    Maridia,
}
