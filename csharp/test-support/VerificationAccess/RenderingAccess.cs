using SuperMetroid.Core.Spatial;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Verification views of private Direct3D 11 worker state.</summary>
internal static class RenderingAccess
{
    extension(D3D11RenderWorker worker)
    {
        internal bool IsSurfaceSuspended => worker.surfaceSuspended;
        internal long LastConsumedSequence => worker.lastConsumedSequence;
        internal D3D11DeviceLossDiagnostic? LastDeviceLoss => worker.lastDeviceLoss;
        internal long OccludedFrames => worker.occluded;
        internal long RetainedRedraws => worker.retainedRedraws;

        internal Extent16<DisplayPixels> LastDrawnSize
        {
            get
            {
                long size = worker.lastDrawnSize;
                return new((ushort)(size >> 32), (ushort)size);
            }
        }
    }
}
