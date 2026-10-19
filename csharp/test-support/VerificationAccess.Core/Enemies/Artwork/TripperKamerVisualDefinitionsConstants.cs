using SuperMetroid.Core.Enemies.Platforms;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values and helpers of <see cref="TripperKamerVisualDefinitions"/> that only verification reads.</summary>
internal static class TripperKamerVisualDefinitionsConstants
{
    /// <summary>$A3:9F14-9F25 tests the complete X-movement word for zero.
    /// Every nonzero value chooses the right map, including values outside the normal dispatcher domain.</summary>
    internal static ushort FrozenFrame(PlatformHorizontalMovement direction) =>
        direction == PlatformHorizontalMovement.Left ? TripperKamerVisualDefinitions.FrozenMovingLeft : TripperKamerVisualDefinitions.FrozenMovingRight;
}
