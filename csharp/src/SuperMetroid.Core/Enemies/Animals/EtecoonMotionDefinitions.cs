namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The speeds an Etecoon hops, jumps and runs at (#2279).</summary>
internal static class EtecoonMotionDefinitions
{
    /// <summary>The vertical speed an ordinary hop starts at: three pixels per update upward.</summary>
    internal static readonly SignedSixteenSixteen HopSpeed = SignedSixteenSixteen.FromParts(-3, 0);

    /// <summary>The vertical speed the long demonstration jump starts at: four pixels per update upward.</summary>
    internal static readonly SignedSixteenSixteen LongJumpSpeed = SignedSixteenSixteen.FromParts(-4, 0);

    /// <summary>The vertical speed of the hop that lands the long route: a pixel per update upward.</summary>
    internal static readonly SignedSixteenSixteen LongJumpLandingSpeed = SignedSixteenSixteen.FromParts(-1, 0);

    /// <summary>The horizontal speed of a rightward run: two pixels per update.</summary>
    internal static readonly SignedSixteenSixteen RightRunSpeed = SignedSixteenSixteen.FromParts(2, 0);

    /// <summary>The horizontal speed of a leftward run: two pixels per update.</summary>
    internal static readonly SignedSixteenSixteen LeftRunSpeed = SignedSixteenSixteen.FromParts(-2, 0);

    /// <summary>How far ahead, in pixels, a rightward run looks for the wall it jumps at.</summary>
    internal static readonly SignedSixteenSixteen RightWallLookahead = SignedSixteenSixteen.FromWholePixels(32);
}
