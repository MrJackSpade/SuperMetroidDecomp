namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One stretch of Ceres Ridley's getaway zoom curve (#2247): a zoom held for its updates, or narrowed by a fixed step each
/// update from its first, so the floor shows larger as Ridley climbs away.
/// </summary>
/// <param name="From">The zoom the stretch's first update shows.</param>
/// <param name="NarrowedEachUpdate">How far each later update of the stretch narrows the zoom, or null while it holds.</param>
/// <param name="Updates">How many updates the stretch lasts, at least one.</param>
internal readonly record struct CeresRidleyGetawayZoomStretch(FloorZoom From, FloorZoomStep? NarrowedEachUpdate, int Updates)
{
    /// <summary>A stretch that holds <paramref name="zoom"/> for <paramref name="updates"/> updates.</summary>
    internal static CeresRidleyGetawayZoomStretch Hold(FloorZoom zoom, int updates) => new(zoom, null, updates);

    /// <summary>A stretch that starts at <paramref name="from"/> and narrows by <paramref name="step"/> each update for <paramref name="updates"/> updates.</summary>
    internal static CeresRidleyGetawayZoomStretch Narrowing(FloorZoom from, FloorZoomStep step, int updates) => new(from, step, updates);

    /// <summary>The zoom <paramref name="updatesIn"/> updates into the stretch.</summary>
    internal FloorZoom ZoomAfter(int updatesIn) => NarrowedEachUpdate is { } step
        ? From.Narrowed(step, updatesIn)
        : From;
}
