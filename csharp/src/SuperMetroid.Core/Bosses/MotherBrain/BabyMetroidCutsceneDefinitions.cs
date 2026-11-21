namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid's health and waits through Mother Brain's last fight (#2216): its health as it arrives, through the
/// final charge and for the last volley, and how long each of its waits lasts, in the updates it lasts, its last being the
/// update it runs out on (#2311).
/// </summary>
internal static partial class BabyMetroidCutsceneDefinitions
{
    /// <summary>The Baby's health as it arrives; Mother Brain's onion rings wear it down while it shields Samus.</summary>
    internal const int ArrivalHealth = 3200;

    /// <summary>The updates the Baby holds still at its arrival point; on the last its dash onto the screen curves in.</summary>
    internal const int ArrivalWaitUpdates = 249;

    /// <summary>The updates of its curve toward Mother Brain's head.</summary>
    internal const int CurveUpdates = 11;

    /// <summary>The updates of its approach right up into her face, unless it reaches her brain first.</summary>
    internal const int FaceApproachUpdates = 10;

    /// <summary>The updates it stays pinned to her brain, no longer shaking, once she has turned to a corpse.</summary>
    internal const int StopDrainingUpdates = 65;

    /// <summary>The updates it rises toward the ceiling letting go of her; on the last the dust bursts on her head.</summary>
    internal const int LetGoUpdates = 33;

    /// <summary>The health the Baby is given once Mother Brain's volleys have worn it out, carried into its last charge.</summary>
    internal const int FinalChargeHealth = 320;

    /// <summary>The health the Baby is given at the final charge's start, so the final four-ring volley kills it on its first hit.</summary>
    internal const int FinalVolleyHealth = 79;

    /// <summary>The updates an onion ring's hit shakes the Baby for, the count its hit timer starts at.</summary>
    internal const int OnionRingHitShake = 16;

    /// <summary>How far above Samus the Baby may be, falling as it dies, for her rainbow to start.</summary>
    internal const int SamusRainbowAboveBaby = 16;

    /// <summary>The longest Samus's rainbow glow is held on each colour as it slows.</summary>
    internal const int SamusRainbowLongestHold = 10;

    /// <summary>The height the dying Baby's centre must reach before its fade to black starts.</summary>
    internal const int FadeToBlackLineY = 128;

    /// <summary>The updates the fatal blow shakes the Baby.</summary>
    internal const int FatalBlowShakeUpdates = 17;

    /// <summary>The updates from the end of the fatal blow's shake to Samus's theme.</summary>
    internal const int SamusThemeUpdates = 57;

    /// <summary>The updates the Hyper Beam's preparation lasts before Samus freezes to take it.</summary>
    internal const int HyperBeamPreparationUpdates = 13;

    /// <summary>The updates between the images of the Baby's fade to black.</summary>
    internal const int FadeToBlackStepUpdates = 9;

    /// <summary>The updates between the dust explosions about the dying Baby.</summary>
    internal const int DeathExplosionIntervalUpdates = 5;

    /// <summary>
    /// The updates of the pause after its fade to black ends, Samus's rainbow cycling, before her rainbow runs on; the last
    /// is the first of <see cref="RainbowSomeMoreUpdates"/>.
    /// </summary>
    internal const int AfterFadeUpdates = 132;

    /// <summary>The updates Samus's rainbow runs on after the pause, before the room's lights return.</summary>
    internal const int RainbowSomeMoreUpdates = 177;

    /// <summary>
    /// Where the three dust clouds burst on Mother Brain's head as the Baby lets go, from her brain, in the order they
    /// spawn.
    /// </summary>
    internal static IReadOnlyList<Offset16<RoomPixels>> ReleaseDustOffsets { get; } = [new(-16, -8), new(0, -16), new(16, -8)];
}
