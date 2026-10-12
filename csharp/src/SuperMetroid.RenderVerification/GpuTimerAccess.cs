using SuperMetroid.Rendering.Direct3D11;

/// <summary>Verification view of the private GPU timer query ring.</summary>
internal static class GpuTimerAccess
{
    extension(D3D11GpuTimer timer)
    {
        /// <summary>Reads the query-ring occupancy so verification can observe GPU samples still awaiting resolution.</summary>
        internal int PendingSamples => PrivateState.Field<int>(timer, "pending");
    }
}
