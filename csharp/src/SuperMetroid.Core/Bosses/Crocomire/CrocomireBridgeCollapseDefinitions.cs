namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's fall through his bridge (#2171): how his tongue waits to hide and where his sinking body disappears.</summary>
internal static class CrocomireBridgeCollapseDefinitions
{
    /// <summary>
    /// How long the collapsing bridge's tongue holds its last frame, in updates: the longest wait a timer is given, so it
    /// holds until its hidden animation takes over. It is kept exact (#1627): the tongue keeps counting it down, and its
    /// frame and new-frame flag follow that count until the hidden animation takes over.
    /// </summary>
    internal const int TongueHoldsUntilHidden = 32767;

    /// <summary>The body X from which Crocomire's weight breaks the bridge and he falls through it.</summary>
    internal const int CollapseStartX = 1600;

    /// <summary>The dust clouds the collapsing bridge bursts into, a block apart along it.</summary>
    internal const int CollapseDustCount = 7;

    /// <summary>Starting acceleration of every sink: eight pixels per frame per frame.</summary>
    internal static readonly SignedEightEight SinkStartingAcceleration = SignedEightEight.FromPixels(8);

    /// <summary>Acceleration the sink adds each frame: three and a half pixels per frame per frame.</summary>
    internal static readonly SignedEightEight SinkAccelerationIncrement = SignedEightEight.FromParts(3, 128);

    /// <summary>The sink's acceleration stops growing at forty-eight whole pixels, keeping its fraction.</summary>
    internal const int MaximumSinkAccelerationWhole = 48;

    /// <summary>The sink's speed stops growing at three whole pixels per frame, keeping its fraction.</summary>
    internal const int MaximumSinkSpeedWhole = 3;

    /// <summary>The rise's acceleration stops growing at thirty-one pixels per frame per frame.</summary>
    internal static readonly SignedEightEight MaximumRiseAcceleration = SignedEightEight.FromPixels(31);

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

    /// <summary>How long he stays sunk between a sink and the next rise, in updates.</summary>
    internal const int SubmergedPause = 48;

    /// <summary>The height of each row of his melting body above the acid, in pixels.</summary>
    internal const int MeltRowHeight = 16;

    /// <summary>How often the acid smokes about his body, in updates.</summary>
    internal const int AcidSmokeInterval = 6;

    /// <summary>How far below the acid's surface its smoke starts before its rise, in pixels.</summary>
    internal const int AcidSmokeDepth = 16;
}
