namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's fall through his bridge (#2171): where it breaks, how he sinks and rises, and where his sinking body disappears.</summary>
internal static class CrocomireBridgeCollapseDefinitions
{
    /// <summary>The body X from which Crocomire's weight breaks the bridge and he falls through it.</summary>
    internal const int CollapseStartX = 1600;

    /// <summary>The dust clouds the collapsing bridge bursts into, a block apart along it.</summary>
    internal const int CollapseDustCount = 7;

    /// <summary>Starting acceleration of every sink, each update (#2299).</summary>
    internal static readonly SignedSixteenSixteen SinkStartingAcceleration = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(8);

    /// <summary>Acceleration the sink adds each update (#2299).</summary>
    internal static readonly SignedSixteenSixteen SinkAccelerationIncrement = SignedSixteenSixteen.FromParts(0, 896);

    /// <summary>The most the sink's acceleration grows to (#2299).</summary>
    internal static readonly SignedSixteenSixteen MaximumSinkAcceleration = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(48);

    /// <summary>
    /// The sink's fastest whole pixels an update, three (#2299): a speed past them keeps them and its own fraction
    /// (<see cref="CrocomireSpeedGain"/>). Each rise starts at this speed.
    /// </summary>
    internal static readonly SignedEightEight MaximumSinkSpeed = SignedEightEight.FromPixels(3);

    /// <summary>The acceleration the rise adds each update (#2299).</summary>
    internal static readonly SignedSixteenSixteen RiseAccelerationIncrement = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(1);

    /// <summary>The most the rise's acceleration grows to (#2299).</summary>
    internal static readonly SignedSixteenSixteen MaximumRiseAcceleration = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(31);

    /// <summary>The speed a rise keeps when its deceleration would take it below zero: 255/256 pixel per frame.</summary>
    internal static readonly SignedEightEight RiseSpeedAfterStalling = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(255);

    /// <summary>How far below the top of Crocomire's view, in pixels, the row from which his sinking hides his body lies.</summary>
    internal const int SinkHiddenRowDepth = 288;

    /// <summary>The bridge's left edge, where the first stage of his approach onto it begins (#2182).</summary>
    internal const int ApproachStartX = 1536;

    /// <summary>How far each stage of his approach onto the bridge lies past the one before, a block, in pixels.</summary>
    internal const int ApproachStageWidth = 16;

    /// <summary>The row the approach's dust and the collapse's raised puffs rise on.</summary>
    internal const int DustY = 176;

    /// <summary>The row the collapse's other dust puffs lie on.</summary>
    internal const int LowDustY = 192;

    /// <summary>How often the acid's burning sound plays while he sinks and rises, in updates.</summary>
    internal const int AcidSoundInterval = 32;

    /// <summary>The row from which his sinking body has sunk, which ends each sink.</summary>
    internal const int SunkenY = 280;

    /// <summary>The row above which his rising body has risen, exclusive, which ends each rise.</summary>
    internal const int RisenY = 218;

    /// <summary>The updates he rests sunk between a sink and the next rise: he starts to rise on the last.</summary>
    internal const int SubmergedPauseUpdates = 49;

    /// <summary>The height of each row of his melting body above the acid, in pixels.</summary>
    internal const int MeltRowHeight = 16;

    /// <summary>How often the acid smokes about his body, in updates.</summary>
    internal const int AcidSmokeInterval = 6;

    /// <summary>How far below the acid's surface its smoke starts before its rise, in pixels.</summary>
    internal const int AcidSmokeDepth = 16;
}
