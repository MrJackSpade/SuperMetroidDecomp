namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Four mutually exclusive cannon roles selected by the wall-turret firing's low random bits.</summary>
internal enum DraygonFiringCannon
{
    /// <summary>The lower-left wall turret's origin.</summary>
    LowerLeft,
    /// <summary>The upper-right wall turret's origin.</summary>
    UpperRight,
    /// <summary>The lower-right wall turret's origin.</summary>
    LowerRight,
    /// <summary>The unused bottom wall turret's origin.</summary>
    UnusedBottom,
}
