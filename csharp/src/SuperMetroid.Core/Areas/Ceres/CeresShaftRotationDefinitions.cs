namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres escape shaft's rotation steps and how far each tilts the shaft.</summary>
public static class CeresShaftRotationDefinitions
{

    /// <summary>The shaft has 69 rotation steps, from 34 steps tilted anticlockwise through 34 tilted clockwise.</summary>
    public const int StepCount = 69;

    /// <summary>The step whose rotation is level, the middle of the steps.</summary>
    public const int CenterStep = 34;

    /// <summary>The steps from level that tilt the shaft one radian: each step tilts it a further 256th of a radian.</summary>
    public const double StepsPerRadian = 256;
}
