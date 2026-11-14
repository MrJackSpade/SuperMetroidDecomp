namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One Rinka of the Mother Brain flashback: where it appears, how it drifts sideways and how it flies.</summary>
/// <param name="Position">Where the Rinka appears on screen.</param>
/// <param name="Drift">How far the Rinka moves sideways each update: half a pixel right or left.</param>
/// <param name="Flight">
/// How the Rinka flies once it starts moving: into Samus, knocking her back, or past her until it leaves the screen.
/// </param>
internal readonly record struct IntroRinkaLaunch(
    Position16<ScreenPixels> Position,
    SignedSixteenSixteen Drift,
    CinematicSpriteFrameBehavior Flight);
