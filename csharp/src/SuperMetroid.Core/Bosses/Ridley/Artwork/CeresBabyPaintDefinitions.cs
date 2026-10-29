namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The selected channel limits of the Ceres Baby's paint; <see cref="CeresBabyPaint"/> calculates the paint from them.</summary>
internal static class CeresBabyPaintDefinitions
{
    /// <summary>Maximum value of a five-bit RGB channel.</summary>
    internal const int Maximum = (1 << 5) - 1;
    /// <summary>Per-pose increase applied to the Baby/container paint channels.</summary>
    internal const int BrightnessStep = 3;
    /// <summary>The brightness steps the round pose adds to the Baby's resting colours.</summary>
    internal const int RoundBrighteningSteps = 1;
    /// <summary>The brightness steps the vertical squish, the brightest shade, adds to the Baby's resting colours.</summary>
    internal const int VerticalSquishBrighteningSteps = 2;
}
