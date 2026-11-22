namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How a Dachora wakes, runs, crouches to charge its shinespark, launches and falls (#2149, #2279).</summary>
internal static partial class DachoraMotionDefinitions
{
    /// <summary>How far a Dachora drops as it crouches to charge, and rises again as it launches, in pixels.</summary>
    internal const int ChargeCrouchDepth = 8;

    /// <summary>The horizontal distance, in pixels, Samus must come strictly within to wake a waiting Dachora.</summary>
    internal const int ActivationXDistance = 96;

    /// <summary>The vertical distance, in pixels, Samus must come strictly within to wake a waiting Dachora.</summary>
    internal const int ActivationYDistance = 64;

    /// <summary>How far a waiting Dachora probes down each update to stay on the floor: one pixel.</summary>
    internal static readonly SignedSixteenSixteen WaitingFloorProbe = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>
    /// How far a running Dachora presses itself down each update before its speed changes, keeping it on the room's slopes:
    /// one pixel.
    /// </summary>
    internal static readonly SignedSixteenSixteen RunningFloorPress = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>The run's speed cap: eight pixels per update.</summary>
    internal static readonly SignedSixteenSixteen MaximumSpeed = SignedSixteenSixteen.FromWholePixels(8);

    /// <summary>The run's acceleration: a sixteenth of a pixel per update, every update.</summary>
    internal static readonly SignedSixteenSixteen Acceleration = SignedSixteenSixteen.FromPixelFraction(1, 16);

    /// <summary>
    /// The run speed at which the run's animation changes to its fast clone: four pixels per update. At
    /// <see cref="MaximumSpeed"/> it changes to the very fast clone.
    /// </summary>
    internal static readonly SignedSixteenSixteen FastRunSpeed = SignedSixteenSixteen.FromWholePixels(4);

    /// <summary>The shinespark's upward speed cap in whole pixels per update; the fraction is kept.</summary>
    internal const int ShinesparkMaximumWholeSpeed = 15;

    /// <summary>The fall's speed cap: ten pixels per update.</summary>
    internal static readonly SignedSixteenSixteen MaximumFallSpeed = SignedSixteenSixteen.FromWholePixels(10);

    /// <summary>The room column at which the tutorial Dachora's leftward run turns back to the right.</summary>
    internal const int LeftTurnX = 96;

    /// <summary>The room column at which the tutorial Dachora's rightward run stops to charge its shinespark.</summary>
    internal const int ShinesparkX = 1152;
}
