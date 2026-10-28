using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Bank-$94 slope response and alignment tables shared by movement/collision.</summary>
internal static class SamusMovementDefinitionsSlopes
{
    /// <summary>Non-square-slope horizontal velocity multipliers.</summary>
    public const int HorizontalMultipliers = 0x948586;

    /// <summary>Sixteen-pixel height profiles indexed by non-square slope shape.</summary>
    /// <remarks>Physical alias of <see cref="SlopeHeights.Read"/>. Proof: #625 / #914.</remarks>
    public const int AlignmentHeights = 0x948b2b;
}
