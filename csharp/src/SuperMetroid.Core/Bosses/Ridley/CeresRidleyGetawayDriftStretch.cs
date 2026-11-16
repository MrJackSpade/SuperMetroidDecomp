namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One stretch of Ceres Ridley's getaway drift curve (#2247): how far the floor drifts each update of the stretch. Its X is
/// subtracted from the floor's horizontal offset, so the image moves right, and its Y added to the vertical offset.
/// </summary>
/// <param name="Drift">The floor's drift each update of the stretch.</param>
/// <param name="Updates">How many updates the stretch lasts, at least one.</param>
internal readonly record struct CeresRidleyGetawayDriftStretch(Offset16<FloorPixels> Drift, int Updates);
