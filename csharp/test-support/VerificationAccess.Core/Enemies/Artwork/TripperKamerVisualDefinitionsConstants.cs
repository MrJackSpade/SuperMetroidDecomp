using SuperMetroid.Core.Enemies.Platforms;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values and helpers of <see cref="TripperKamerVisualDefinitions"/> that only verification reads.</summary>
internal static class TripperKamerVisualDefinitionsConstants
{
    /// <summary>The frozen Tripper's map for the way it moves: $A3:9F14-9F25 chooses the left map for leftward movement and the right map otherwise.</summary>
    internal static ushort FrozenFrame(PlatformHorizontalMovement direction) =>
        direction == PlatformHorizontalMovement.Left ? TripperKamerVisualDefinitions.FrozenMovingLeft : TripperKamerVisualDefinitions.FrozenMovingRight;
}
