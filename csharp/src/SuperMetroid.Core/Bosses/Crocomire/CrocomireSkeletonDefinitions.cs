namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Where his skeleton flows, waits, breaks through the spike wall and falls apart, and what it leaves (#2182).
/// </summary>
internal static class CrocomireSkeletonDefinitions
{
    /// <summary>How far his skeleton flows left down the river each update, in pixels.</summary>
    internal const int RiverDrift = 2;

    /// <summary>The row his skeleton flows down the river on.</summary>
    internal const int RiverY = 220;

    /// <summary>Where his skeleton waits behind the hidden wall, which the river brings it to and the break-through starts from.</summary>
    internal static readonly Position16<RoomPixels> BehindWallPosition = new(480, 54);

    /// <summary>How far left Samus must come, exclusive, before the hidden wall starts to rumble.</summary>
    internal const int WallApproachX = 640;

    /// <summary>His skeleton's vertical reach while it waits behind the wall and breaks through.</summary>
    internal const int BehindWallReachY = 56;

    /// <summary>How long the wall shows its spikes after its rumble before his skeleton breaks through, in updates.</summary>
    internal const int SpikedWallDuration = 128;

    /// <summary>How long his skeleton holds after it breaks through before it falls, in updates.</summary>
    internal const int FallDelay = 80;

    /// <summary>How far right his breaking-out skeleton slides, exclusive, before it stops.</summary>
    internal const int BreakOutEndX = 224;

    /// <summary>How far right his arcing skeleton flies, exclusive, before it lands and falls apart.</summary>
    internal const int ArcEndX = 576;

    /// <summary>The sideways speed his skeleton gathers as it breaks out: half a pixel an update, up to two pixels.</summary>
    internal static readonly CrocomireSkeletonAcceleration BreakOut = new(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(128), 2);

    /// <summary>The sideways speed his skeleton gathers as it arcs: a thirty-second of a pixel an update, up to five pixels.</summary>
    internal static readonly CrocomireSkeletonAcceleration Arc = new(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(8), 5);

    /// <summary>The sideways speed his skeleton gathers as it falls apart: a sixteenth of a pixel an update, up to six pixels.</summary>
    internal static readonly CrocomireSkeletonAcceleration Collapse = new(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(16), 6);

    /// <summary>How far his dead skeleton lies from where it fell apart.</summary>
    internal static readonly Offset16<RoomPixels> CorpseOffset = new(64, 21);

    /// <summary>The drops his dead skeleton scatters.</summary>
    internal const int DropCount = 16;

    /// <summary>The left edge of the strip his skeleton's drops scatter across.</summary>
    internal const int DropLeft = 576;

    /// <summary>The top of the band his skeleton's drops scatter within.</summary>
    internal const int DropTop = 96;

    /// <summary>How far either side of his body the dust of his defeat rises, in pixels.</summary>
    internal const int DefeatDustOffset = 16;

    /// <summary>Where the spike wall's top piece stands as the wall breaks.</summary>
    internal static readonly Position16<RoomPixels> SpikeWallTop = new(528, 56);

    /// <summary>How far below the piece above it each spike-wall piece stands, in pixels.</summary>
    internal const int SpikeWallPieceSpacing = 16;
}
