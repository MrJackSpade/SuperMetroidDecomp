namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>What a wall the Shaktool chain's trailing saw strikes does to the chain (#2452). Its values are arbitrary.</summary>
internal enum ShaktoolWallResponse
{
    /// <summary>
    /// The chain turns end for end: the saws trade places, the mirrored segments trade orbit angles and the head's facing
    /// reverses. The next wall then reverses the orbit, until the trailing saw reaches the chain's target.
    /// </summary>
    TurnEndForEnd,

    /// <summary>The chain's orbit turns the other way round; the next wall turns the chain end for end again.</summary>
    ReverseOrbit,
}
