using SuperMetroid.Core.Spatial;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Verification views of private Direct3D 11 worker state.</summary>
internal static class RenderingAccess
{
    /// <summary>Provides extension members for <c>D3D11RenderWorker</c>.</summary>
    extension(D3D11RenderWorker worker)
    {
        /// <summary>Gets or sets is surface suspended.</summary>
        internal bool IsSurfaceSuspended => PrivateState.Field<bool>(worker, "surfaceSuspended");
        /// <summary>Gets or sets last consumed sequence.</summary>
        internal long LastConsumedSequence => PrivateState.Field<long>(worker, "lastConsumedSequence");
        /// <summary>Gets or sets last device loss.</summary>
        internal D3D11DeviceLossDiagnostic? LastDeviceLoss => PrivateState.Field<D3D11DeviceLossDiagnostic?>(worker, "lastDeviceLoss");
        /// <summary>Gets or sets occluded frames.</summary>
        internal long OccludedFrames => PrivateState.Field<long>(worker, "occluded");
        /// <summary>Gets or sets retained redraws.</summary>
        internal long RetainedRedraws => PrivateState.Field<long>(worker, "retainedRedraws");

        /// <summary>Gets or sets last drawn size.</summary>
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
