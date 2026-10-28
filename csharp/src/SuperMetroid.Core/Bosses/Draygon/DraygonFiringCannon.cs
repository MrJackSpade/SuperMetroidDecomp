namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Four mutually exclusive cannon roles selected by HandleFiringWallTurret's low random bits.</summary>
internal enum DraygonFiringCannon
{
    /// <summary>$A5:87E4 HandleFiringWallTurret origin, controlled by $7E:8804.</summary>
    LowerLeft,
    /// <summary>$A5:87E8 HandleFiringWallTurret origin, controlled by $7E:8806.</summary>
    UpperRight,
    /// <summary>$A5:87EC HandleFiringWallTurret origin, controlled by $7E:8808.</summary>
    LowerRight,
    /// <summary>$A5:87F0 HandleFiringWallTurret origin, controlled by $7E:880A.</summary>
    UnusedBottom,
}
