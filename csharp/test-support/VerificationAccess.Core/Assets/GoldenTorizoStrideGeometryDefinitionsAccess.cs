using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="GoldenTorizoStrideGeometryDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoStrideGeometryDefinitionsAccess
{
    /// <summary>Calculates native frame addresses for the ten stride phases.</summary>
    extension(GoldenTorizoStrideGeometryDefinitions)
    {
        /// <summary>Offsets the first frame by the fixed record width after validating the phase.</summary>
        internal static int NativeFrameIdentity(int phase)
        {
            if ((uint)phase >= 10) throw new ArgumentOutOfRangeException(nameof(phase));
            return GoldenTorizoStrideGeometryDefinitions.FirstFrame + GoldenTorizoStrideGeometryDefinitions.FrameBytes * phase;
        }
    }
}
