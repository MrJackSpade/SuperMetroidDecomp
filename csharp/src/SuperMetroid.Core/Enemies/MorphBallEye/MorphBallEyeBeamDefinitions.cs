using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye's beam's colour and width, as its behaviour writes them.</summary>
internal static class MorphBallEyeBeamDefinitions
{
    /// <summary>The colour the beam adds when it appears: red and green at intensity 16, without blue.</summary>
    internal static RgbColor InitialColor { get; } = new(132, 132, 0);

    /// <summary>The beam cycles through sixteen colours.</summary>
    internal const int ColorCycleLength = 16;

    /// <summary>How much faster the widening beam's half-width grows each update: a quarter step.</summary>
    internal static RotationAngle WidthGrowthStep { get; } = RotationAngle.FromSteps(0, 64);

    /// <summary>The half-width at which a widening beam is full: four whole steps either side of its centre.</summary>
    internal static RotationAngle FullHalfWidth { get; } = RotationAngle.FromSteps(4);
}
