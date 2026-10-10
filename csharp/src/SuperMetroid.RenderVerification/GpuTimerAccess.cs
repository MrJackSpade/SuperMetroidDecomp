using SuperMetroid.Rendering.Direct3D11;

/// <summary>Verification view of the private GPU timer query ring.</summary>
internal static class GpuTimerAccess
{
    /// <summary>Exposes the GPU timer's private query-ring count to render verification.</summary>
    extension(D3D11GpuTimer timer)
    {
        /// <summary>Number of timing-query samples still pending completion in the GPU timer ring.</summary>
        internal int PendingSamples => PrivateState.Field<int>(timer, "pending");
    }
}
