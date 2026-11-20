namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One stretch of Ceres Ridley's getaway zoom curve (#2247): a zoom held for its updates, or narrowed by a fixed step each
/// update from its first, so the floor shows larger as Ridley climbs away.
/// </summary>
/// <param name="From">The zoom the stretch's first update shows.</param>
/// <param name="Updates">How many updates the stretch lasts, at least one.</param>
internal abstract record CeresRidleyGetawayZoomStretch(FloorZoom From, int Updates)
{
    /// <summary>A stretch that holds <paramref name="zoom"/> for <paramref name="updates"/> updates.</summary>
    internal static CeresRidleyGetawayZoomStretch Hold(FloorZoom zoom, int updates) => new Holding(zoom, updates);

    /// <summary>A stretch that starts at <paramref name="from"/> and narrows by <paramref name="step"/> each update for <paramref name="updates"/> updates.</summary>
    internal static CeresRidleyGetawayZoomStretch Narrowing(FloorZoom from, FloorZoomStep step, int updates) =>
        new NarrowingStretch(from, step, updates);

    /// <summary>The zoom <paramref name="updatesIn"/> updates into the stretch.</summary>
    internal abstract FloorZoom ZoomAfter(int updatesIn);

    /// <summary>A stretch that holds its zoom.</summary>
    /// <param name="From">The zoom the stretch shows.</param>
    /// <param name="Updates">How many updates the stretch lasts, at least one.</param>
    private sealed record Holding(FloorZoom From, int Updates) : CeresRidleyGetawayZoomStretch(From, Updates)
    {
        /// <inheritdoc/>
        internal override FloorZoom ZoomAfter(int updatesIn) => From;
    }

    /// <summary>A stretch that narrows its zoom by <paramref name="NarrowedEachUpdate"/> each update after its first.</summary>
    /// <param name="From">The zoom the stretch's first update shows.</param>
    /// <param name="NarrowedEachUpdate">How far each later update of the stretch narrows the zoom.</param>
    /// <param name="Updates">How many updates the stretch lasts, at least one.</param>
    private sealed record NarrowingStretch(FloorZoom From, FloorZoomStep NarrowedEachUpdate, int Updates)
        : CeresRidleyGetawayZoomStretch(From, Updates)
    {
        /// <inheritdoc/>
        internal override FloorZoom ZoomAfter(int updatesIn) => From.Narrowed(NarrowedEachUpdate, updatesIn);
    }
}
