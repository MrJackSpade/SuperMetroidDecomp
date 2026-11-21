using SuperMetroid.Core.Assets;namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye's beam's colour and width, as its behaviour writes them.</summary>
internal static class MorphBallEyeBeamDefinitions
{
    /// <summary>The colour the beam adds when it appears: red and green at intensity 16, without blue.</summary>
    internal static RgbColor InitialColor { get; } = new FadeLevelColour(16, 16, 0).ToColour();

    /// <summary>
    /// The sixteen colours the full beam cycles through, one each update: red and green from intensity 16 down to 8 and
    /// back up to 15, without blue.
    /// </summary>
    internal static IReadOnlyList<RgbColor> CycleColors { get; } =
    [
        new FadeLevelColour(16, 16, 0).ToColour(),
        new FadeLevelColour(15, 15, 0).ToColour(),
        new FadeLevelColour(14, 14, 0).ToColour(),
        new FadeLevelColour(13, 13, 0).ToColour(),
        new FadeLevelColour(12, 12, 0).ToColour(),
        new FadeLevelColour(11, 11, 0).ToColour(),
        new FadeLevelColour(10, 10, 0).ToColour(),
        new FadeLevelColour(9, 9, 0).ToColour(),
        new FadeLevelColour(8, 8, 0).ToColour(),
        new FadeLevelColour(9, 9, 0).ToColour(),
        new FadeLevelColour(10, 10, 0).ToColour(),
        new FadeLevelColour(11, 11, 0).ToColour(),
        new FadeLevelColour(12, 12, 0).ToColour(),
        new FadeLevelColour(13, 13, 0).ToColour(),
        new FadeLevelColour(14, 14, 0).ToColour(),
        new FadeLevelColour(15, 15, 0).ToColour(),
    ];

    /// <summary>How much faster the widening beam's half-width grows each update: a quarter step.</summary>
    internal static RotationAngle WidthGrowthStep { get; } = RotationAngle.FromStepFraction(1, 4);

    /// <summary>The half-width at which a widening beam is full: four whole steps either side of its centre.</summary>
    internal static RotationAngle FullHalfWidth { get; } = RotationAngle.FromSteps(4);
}
