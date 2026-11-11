namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The counter cadence of the Ceres destruction cinematic's planet reveal (#1606).</summary>
internal static class CeresCinematicCadenceDefinitions
{
    /// <summary>
    /// The updates on which the planet reveal's mosaic shrinks one size: every fourth update of the cinematic frame
    /// counter, on its first place.
    /// </summary>
    internal static readonly UpdateCadence ZebesMosaicStepEveryFourthUpdate = new(4, 0);
}
