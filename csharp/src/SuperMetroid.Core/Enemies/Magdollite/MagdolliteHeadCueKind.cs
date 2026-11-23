namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What a signal cue of the Magdollite head's animations does: place the head and body as it sinks, surfaces or rises
/// again, or finish its submerge or emerge (#2554).
/// </summary>
internal enum MagdolliteHeadCueKind
{
    /// <summary>Finishes the head's emergence (#2439): its next update idles it.</summary>
    FinishEmerge,
    /// <summary>Finishes the head's submerge (#2439): its next update releases its arm.</summary>
    FinishSubmerge,
    /// <summary>Returns the head and body to the origin height.</summary>
    RestoreOrigins,
    /// <summary>Puts the head and body at their sunk depth (<see cref="MagdolliteMotionDefinitions.SunkDepth"/>) and shows the body and overlay.</summary>
    SinkAndShowParts,
    /// <summary>Puts the head and body at their surfaced depth (<see cref="MagdolliteMotionDefinitions.SurfacedDepth"/>) and hides the body and overlay.</summary>
    SurfaceAndHideParts,
}
