namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One update of Ceres Ridley's getaway (#2247, #2553).</summary>
/// <param name="Zoom">The floor's zoom on the update.</param>
/// <param name="Drift">
/// The floor's drift on the update: its X is subtracted from the floor's horizontal offset, so the image moves right, and
/// its Y added to the vertical offset.
/// </param>
public readonly partial record struct CeresRidleyGetawayStep(FloorZoom Zoom, Offset16<FloorPixels> Drift);
