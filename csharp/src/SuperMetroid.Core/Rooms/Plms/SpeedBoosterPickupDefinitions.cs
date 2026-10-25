namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The continuation of the Chozo Speed Booster pickup.</summary>
internal static class SpeedBoosterPickupDefinitions
{
    /// <summary>
    /// The lava's upward speed once the Speed Booster acquisition message returns: an eighth of a pixel per update
    /// (32/256 of a pixel, upward).
    /// </summary>
    internal static SignedEightEight LavaRiseVelocity { get; } = SignedEightEight.FromSignedRaw(-32);
}
