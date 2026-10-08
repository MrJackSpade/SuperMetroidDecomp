using SuperMetroid.Rendering.Direct3D11;

/// <summary>Verification views of private Direct3D 11 worker state.</summary>
internal static class RenderingAccess
{
    extension(D3D11RenderWorker worker)
    {
        internal bool IsSurfaceSuspended => PrivateState.Field<bool>(worker, "surfaceSuspended");
        internal long LastConsumedSequence => PrivateState.Field<long>(worker, "lastConsumedSequence");
        internal D3D11DeviceLossDiagnostic? LastDeviceLoss => PrivateState.Field<D3D11DeviceLossDiagnostic?>(worker, "lastDeviceLoss");
        internal long OccludedFrames => PrivateState.Field<long>(worker, "occluded");
        internal long RetainedRedraws => PrivateState.Field<long>(worker, "retainedRedraws");

        internal (int Width, int Height) LastDrawnSize
        {
            get
            {
                long size = PrivateState.Field<long>(worker, "lastDrawnSize");
                return ((int)(size >> 32), (int)size);
            }
        }
    }
}
