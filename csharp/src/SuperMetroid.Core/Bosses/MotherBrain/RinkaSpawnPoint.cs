namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The eleven places in Mother Brain's room where her special Rinkas appear, which those Rinkas share: each place is
/// held by at most one Rinka at a time (#1699). The values are arbitrary; <see cref="RinkaSpawnPointDefinitions"/>
/// places each point and orders them, and verification's catalog relates each to its source.
/// </summary>
public enum RinkaSpawnPoint
{
    /// <summary>High in the room's rightmost pair of points.</summary>
    FarRightUpper,

    /// <summary>Low in the room's rightmost pair of points.</summary>
    FarRightLower,

    /// <summary>High in the second pair of points from the right.</summary>
    RightUpper,

    /// <summary>Low in the second pair of points from the right.</summary>
    RightLower,

    /// <summary>High in the middle pair of points.</summary>
    CentreUpper,

    /// <summary>Low in the middle pair of points.</summary>
    CentreLower,

    /// <summary>High in the second pair of points from the left.</summary>
    LeftUpper,

    /// <summary>Low in the second pair of points from the left.</summary>
    LeftLower,

    /// <summary>High in the leftmost pair of points.</summary>
    FarLeftUpper,

    /// <summary>Low in the leftmost pair of points.</summary>
    FarLeftLower,

    /// <summary>The lone point left of every pair, low in the room.</summary>
    Leftmost,
}
