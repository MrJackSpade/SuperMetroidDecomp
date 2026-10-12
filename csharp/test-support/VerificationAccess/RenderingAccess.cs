using SuperMetroid.Core.Spatial;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Verification views of private Direct3D 11 worker state.</summary>
internal static class RenderingAccess
{
    /// <summary>Exposes worker lifecycle and presentation counters to focused Direct3D verification.</summary>
    extension(D3D11RenderWorker worker)
    {
        /// <summary>Reports whether zero-sized or unavailable surfaces have suspended present work.</summary>
        internal bool IsSurfaceSuspended => PrivateState.Field<bool>(worker, "surfaceSuspended");
        /// <summary>Most recent render packet sequence consumed by the worker.</summary>
        internal long LastConsumedSequence => PrivateState.Field<long>(worker, "lastConsumedSequence");
        /// <summary>Last classified device-loss result retained for failure reporting.</summary>
        internal D3D11DeviceLossDiagnostic? LastDeviceLoss => PrivateState.Field<D3D11DeviceLossDiagnostic?>(worker, "lastDeviceLoss");
        /// <summary>Number of frames skipped while the presentation surface was occluded.</summary>
        internal long OccludedFrames => PrivateState.Field<long>(worker, "occluded");
        /// <summary>Number of redraws made from a retained packet without consuming new game state.</summary>
        internal long RetainedRedraws => PrivateState.Field<long>(worker, "retainedRedraws");

        /// <summary>Decodes the packed width and height of the last surface size the worker actually drew.</summary>
        internal Extent16<DisplayPixels> LastDrawnSize
        {
            get
            {
                long size = PrivateState.Field<long>(worker, "lastDrawnSize");
                return new((ushort)(size >> 32), (ushort)size);
            }
        }
    }
}
