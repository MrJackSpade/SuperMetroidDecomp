namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>How a Shaktool chain's segments turn on their orbits (#2452). Its values are arbitrary.</summary>
internal enum ShaktoolChainPhase
{
    /// <summary>Each segment turns at its own speed, curling the chain, until the trailing saw has turned far enough.</summary>
    Curling,

    /// <summary>Every segment turns a whole step an update, straightening the chain, until a wall stops it.</summary>
    Straightening,
}
