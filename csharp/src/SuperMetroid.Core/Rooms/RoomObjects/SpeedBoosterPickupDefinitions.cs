namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The continuation of the Chozo Speed Booster pickup.</summary>
internal static partial class SpeedBoosterPickupDefinitions
{
    /// <summary>
    /// The lava's upward speed once the Speed Booster acquisition message returns: an eighth of a pixel upward per
    /// update.
    /// </summary>
    internal static SignedEightEight LavaRiseVelocity { get; } = SignedEightEight.FromPixelFraction(-1, 8);
}
