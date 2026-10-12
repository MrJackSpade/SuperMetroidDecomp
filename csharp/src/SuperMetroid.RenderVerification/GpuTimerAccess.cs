using SuperMetroid.Rendering.Direct3D11;

/// <summary>Verification view of the private GPU timer query ring.</summary>
internal static class GpuTimerAccess
{
    /// <summary>Provides read access to GPU timer results for render verification.</summary>
    extension(D3D11GpuTimer timer)
    {
        /// <summary>Gets pending samples.</summary>
        internal int PendingSamples => PrivateState.Field<int>(timer, "pending");
    }
}
