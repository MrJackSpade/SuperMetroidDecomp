using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye's tracking poses and its beam's colour and width, as its bank-$A3 routines write them.</summary>
internal static class MorphBallEyeBeamDefinitions
{
    /// <summary>The tracking poses the eye's angle chooses among, one for each equal arc of the turn.</summary>
    internal const int TrackingPoses = 16;

    /// <summary>The colour the beam adds when it appears: red and green at intensity 16, without blue.</summary>
    internal static RgbColor InitialColor { get; } = new(16, 16, 0);

    /// <summary>The beam cycles through sixteen colours.</summary>
    internal const int ColorCycleLength = 16;

    /// <summary>How much faster the widening beam's half-width grows each update: a quarter angle unit.</summary>
    internal static SignedSixteenSixteen WidthGrowthStep { get; } = SignedSixteenSixteen.FromParts(0, 16384);

    /// <summary>The whole angular half-width, in angle units, at which a widening beam is full.</summary>
    internal const short FullAngularWidth = 4;
}
