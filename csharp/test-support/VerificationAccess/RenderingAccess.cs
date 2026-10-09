using SuperMetroid.Rendering.Direct3D11;

/// <summary>Verification views of private Direct3D 11 worker state.</summary>
internal static class RenderingAccess
{
    /// <summary>Exposes selected render-worker state to verification code without widening production APIs.</summary>
    extension(D3D11RenderWorker worker)
    {
        /// <summary>Whether the worker has suspended rendering for its current surface.</summary>
        internal bool IsSurfaceSuspended => PrivateState.Field<bool>(worker, "surfaceSuspended");

        /// <summary>Sequence number of the most recent frame consumed by the worker.</summary>
        internal long LastConsumedSequence => PrivateState.Field<long>(worker, "lastConsumedSequence");

        /// <summary>Diagnostic captured when the worker last encountered device loss, if any.</summary>
        internal D3D11DeviceLossDiagnostic? LastDeviceLoss => PrivateState.Field<D3D11DeviceLossDiagnostic?>(worker, "lastDeviceLoss");

        /// <summary>Number of frames the worker observed the surface as occluded.</summary>
        internal long OccludedFrames => PrivateState.Field<long>(worker, "occluded");

        /// <summary>Number of redraws retained for later presentation while immediate drawing was unavailable.</summary>
        internal long RetainedRedraws => PrivateState.Field<long>(worker, "retainedRedraws");

        /// <summary>Width and height of the most recently drawn surface, unpacked from the worker's stored size.</summary>
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
