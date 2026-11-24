using Engine.Shared;

namespace Engine.Actors;

/// <summary>The engine's own reading of the player's speed against its limits: one number compared with another.</summary>
public sealed class PlainSpeedReadings : ISpeedReadings
{
    /// <inheritdoc/>
    public bool IsPastTop(Pixels speed, Pixels top) => speed > top;

    /// <inheritdoc/>
    public bool IsAtCap(Pixels dash, Pixels cap) => dash >= cap;
}
