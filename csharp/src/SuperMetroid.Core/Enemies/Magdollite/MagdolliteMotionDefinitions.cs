namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Where Magdollite's parts hide and surface, how long it waits between throws (#2043), how fast its lava flies and how
/// long each frame of its glow shows (#2156).
/// </summary>
internal static partial class MagdolliteMotionDefinitions
{
    /// <summary>How far below its origin a dormant arm or the hidden overlay waits, in pixels.</summary>
    internal const int HiddenDepth = 32;

    /// <summary>How far below its origin the head and arm sink as the parts show, in pixels.</summary>
    internal const int SunkDepth = 24;

    /// <summary>How far below its origin the head and arm come to rest as the parts hide, in pixels.</summary>
    internal const int SurfacedDepth = 4;

    /// <summary>How far past the tracking overlay the head's collision reaches, in pixels.</summary>
    internal const int HeadReachPastOverlay = 2;

    /// <summary>The head's least vertical collision radius, in pixels.</summary>
    internal const int SmallestHeadRadius = 8;

    /// <summary>How far below the throwing overlay its lava starts, in pixels.</summary>
    internal const int LavaDrop = 2;

    /// <summary>
    /// How long the head waits after a throw before it may attack again, in overlay updates, the head polling after
    /// the last of them.
    /// </summary>
    internal const int AttackCooldown = 257;

    /// <summary>
    /// The overlay updates a placed head, or one that has just polled an expired cooldown, waits before its next poll may
    /// attack.
    /// </summary>
    internal const int AttackPollWait = 1;

    /// <summary>How fast thrown lava flies sideways, three pixels an update, toward the side it is thrown.</summary>
    internal static SignedEightEight LavaThrowSpeed { get; } = SignedEightEight.FromPixels(3);

    /// <summary>How long each frame of the room's lava glow every Magdollite shows lasts, in updates.</summary>
    internal const int GlowFrameDuration = 8;
}
