using Engine.Shared;
using Engine.Actors;

namespace Engine.Tests.Core;

/// <summary>The stand-in game's reading of the player's speed against its limits: one number compared with another.</summary>
public sealed class PlainSpeedReadings : ISpeedReadings
{
    /// <inheritdoc/>
    public bool IsPastTop(Pixels speed, Pixels top) => speed > top;

    /// <inheritdoc/>
    public bool IsAtCap(Pixels dash, Pixels cap) => dash >= cap;

    /// <summary>However the player is held to be going, it is how fast they move that is compared.</summary>
    public bool ReachesWhole(Pixels size, int whole) => size.Abs().Whole >= whole;
}
