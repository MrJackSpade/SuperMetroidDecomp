namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>When the Speed Booster escape's lavaquake starts and how fast it begins.</summary>
public static class SpeedBoosterEscapeDefinitions
{
    /// <summary>The whole-pixel X Samus must reach, moving left, before the liquid starts moving.</summary>
    public const int StartFxMotionSamusX = 2784;

    /// <summary>The lavaquake's initial vertical speed: half a pixel upward per update.</summary>
    public static SignedEightEight InitialLavaquakeVelocity { get; } = SignedEightEight.FromSignedRaw(-128);
}
