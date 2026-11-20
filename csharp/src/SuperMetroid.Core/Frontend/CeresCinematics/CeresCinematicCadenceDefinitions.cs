namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The counter cadence of the Ceres destruction cinematic's planet reveal (#1606).</summary>
internal static class CeresCinematicCadenceDefinitions
{
    /// <summary>
    /// The updates on which the planet reveal's pixelation narrows its blocks one pixel (#1774), counted on the cinematic
    /// frame counter at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence RevealPixelationStep = new(4, 0);
}
