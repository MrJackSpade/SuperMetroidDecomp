using SuperMetroid.Rendering.Direct3D11;

namespace SuperMetroid.RenderVerification;

/// <summary>Verification view of the private GPU timer query ring.</summary>
internal static class GpuTimerAccess
{
    /// <summary>Exposes pending Direct3D query samples so verification can observe whether the GPU timer ring has drained.</summary>
    extension(D3D11GpuTimer timer)
    {
        /// <summary>Reads the query-ring occupancy so verification can observe GPU samples still awaiting resolution.</summary>
        internal int PendingSamples => timer.pending;
    }
}
