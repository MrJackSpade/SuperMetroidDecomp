namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Native Yard vertical launch words indexed by the player's whole horizontal displacement.</summary>
public static class YardKickDefinitions
{
    /// <summary>The highest whole speed that selects its own launch; faster speeds share the last.</summary>
    internal const int FastestKickSpeed = 15;

    /// <summary>
    /// The upward launch each whole horizontal speed selects: three pixels per frame at rest, two thirds of a pixel
    /// more for each further whole pixel of speed, rounded to the sixteenth.
    /// </summary>
    internal static readonly SignedSixteenSixteen[] Kicks =
    [
        SignedSixteenSixteen.FromParts(-3, 0),
        SignedSixteenSixteen.FromParts(-3, 0xa000),
        SignedSixteenSixteen.FromParts(-3, 0x4000),
        SignedSixteenSixteen.FromParts(-4, 0),
        SignedSixteenSixteen.FromParts(-4, 0xa000),
        SignedSixteenSixteen.FromParts(-4, 0x4000),
        SignedSixteenSixteen.FromParts(-5, 0),
        SignedSixteenSixteen.FromParts(-5, 0xa000),
        SignedSixteenSixteen.FromParts(-5, 0x4000),
        SignedSixteenSixteen.FromParts(-6, 0),
        SignedSixteenSixteen.FromParts(-6, 0xa000),
        SignedSixteenSixteen.FromParts(-6, 0x4000),
        SignedSixteenSixteen.FromParts(-7, 0),
        SignedSixteenSixteen.FromParts(-7, 0xa000),
        SignedSixteenSixteen.FromParts(-7, 0x4000),
        SignedSixteenSixteen.FromParts(-8, 0),
    ];
}
