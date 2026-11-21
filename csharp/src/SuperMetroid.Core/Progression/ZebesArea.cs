namespace SuperMetroid.Core.Progression;

/// <summary>
/// One of the six areas of Zebes (#2553): every area but Ceres, the prologue station, which has no elevators, no world-map
/// label and no area-select window. <see cref="AreaIds.Area"/> names the area it is.
/// </summary>
public enum ZebesArea
{
    /// <summary>The planetary surface and adjoining caverns, where Samus lands.</summary>
    Crateria,

    /// <summary>The upper and lower Brinstar region.</summary>
    Brinstar,

    /// <summary>The upper and lower Norfair region.</summary>
    Norfair,

    /// <summary>The derelict spacecraft region.</summary>
    WreckedShip,

    /// <summary>The flooded Maridia region.</summary>
    Maridia,

    /// <summary>The final Tourian region.</summary>
    Tourian,
}
