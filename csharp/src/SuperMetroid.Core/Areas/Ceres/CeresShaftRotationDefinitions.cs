namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres escape shaft's rotation steps and how far each tilts the shaft.</summary>
public static class CeresShaftRotationDefinitions
{

    /// <summary>The steps of the shaft's full tilt either way from level (#1895).</summary>
    public const int FullTiltSteps = 34;

    /// <summary>The steps from level that tilt the shaft one radian: each step tilts it a further 256th of a radian.</summary>
    public const double StepsPerRadian = 256;
}
